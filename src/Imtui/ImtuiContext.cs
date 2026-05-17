// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Collections.Concurrent;
using System.Diagnostics;

public sealed class ImtuiContext
{
    private readonly List<string> _focusableIds = [];
    private readonly Stack<LayoutNode> _layoutStack = [];
    private readonly Queue<ConsoleKeyInfo> _unconsumedKeys = new Queue<ConsoleKeyInfo>();
    private readonly ConcurrentDictionary<Task, byte> _wakeTasks =
        new ConcurrentDictionary<Task, byte>();
    private Action? _wakeHandler;
    private bool _activateFocusedWidget;
    private string? _focusedWidgetId;
    private LayoutNode _root = new LayoutNode();
    private Dimensions _dimensions;
    private TimeSpan _time;
    private TimeSpan? _nextRedrawDelay;
    private long _frameStartTimestamp;
    private TimeSpan _lastFrameTime;

    // Total number of frames begun on this context. Incremented at the start
    // of every call to BeginLayout.
    public long FrameCount { get; private set; }

    // The dimensions passed to the most recent BeginLayout call.
    public Dimensions Dimensions => _dimensions;

    // Monotonic time associated with the current frame, as supplied by the
    // most recent BeginLayout call. Animated widgets read this to compute
    // their state — frames are pure functions of (input, time).
    public TimeSpan Time => _time;

    // The shortest follow-up redraw delay requested by any widget during the
    // current (or just-completed) frame, or null if no widget requested a
    // redraw. The run loop uses this to schedule its next wake-up; tests can
    // read it after EndLayout to assert animation behavior without a clock.
    public TimeSpan? NextScheduledRedraw => _nextRedrawDelay;

    // Attempts to dequeue the next unconsumed keypress for this frame.
    // Returns true and outputs the key if one was available, false otherwise.
    // Tab/Shift-Tab and Enter are handled internally by the context (focus
    // navigation and widget activation) and are never delivered through this
    // queue. Any keys still in the queue at the end of the frame are
    // discarded — a frame is a pure function of input received during it.
    public bool TryConsumeKey(out ConsoleKeyInfo key) => _unconsumedKeys.TryDequeue(out key);

    // Registers a Task whose completion should trigger another frame.
    // Idempotent: calling WakeOn with the same Task across many frames
    // attaches at most one continuation. If the task is already completed
    // the call is a no-op — widgets read task.IsCompleted directly during
    // the frame to decide what to render.
    //
    // Outside the run loop (for example, in unit tests with no wake handler
    // attached) the task is still tracked for idempotency but no frame is
    // triggered on completion.
    public void WakeOn(Task? task)
    {
        if (task is null || task.IsCompleted)
            return;

        // TryAdd is an atomic test-and-set: returns false if this task is
        // already registered, so we attach at most one continuation per task
        // without needing an external lock.
        if (!_wakeTasks.TryAdd(task, 0))
            return;

        Action? handler = _wakeHandler;

        task.ContinueWith(
            completed =>
            {
                _wakeTasks.TryRemove(completed, out _);
                handler?.Invoke();
            },
            TaskScheduler.Default
        );
    }

    // Sets the delegate invoked when a tracked Task completes. The run loop
    // wires this to push a WakeEvent into the wake channel. Tests can leave
    // it unset (the default) or inject a stub to observe wake-ups.
    internal void SetWakeHandler(Action? wakeHandler)
    {
        _wakeHandler = wakeHandler;
    }

    // Identifier of the currently focused widget, or null if no widget has
    // registered for focus.
    public string? FocusedWidgetId => _focusedWidgetId;

    // Wall-clock time spent on the most recently completed frame, measured
    // from BeginLayout until EndFrame. Excludes any time the application
    // spends waiting for input between frames. Zero until the first frame
    // has been completed with a call to EndFrame.
    public TimeSpan LastFrameTime => _lastFrameTime;

    public void BeginLayout(Dimensions dimensions, FrameInput input = default)
    {
        _frameStartTimestamp = Stopwatch.GetTimestamp();
        FrameCount += 1;
        _dimensions = dimensions;
        _time = input.Time;
        _nextRedrawDelay = null;
        _root = new LayoutNode { Dimensions = dimensions, Position = new Position(0, 0) };
        _layoutStack.Clear();
        _layoutStack.Push(_root);
        ProcessInput(input.Keys);
        _focusableIds.Clear();
    }

    // Marks the end of a frame's rendering work. Call this after presenting
    // the frame and before waiting for the next input so that LastFrameTime
    // measures only the work spent producing the frame.
    public void EndFrame()
    {
        _lastFrameTime = Stopwatch.GetElapsedTime(_frameStartTimestamp);
    }

    // Request that another frame be rendered after at most `delay` has
    // elapsed. Multiple requesters within a single frame are aggregated by
    // taking the minimum — five spinners each asking for 100ms = one 100ms
    // wake-up. Negative delays are clamped to zero ("redraw as soon as
    // possible"). Reset at the start of every BeginLayout.
    public void RequestRedrawIn(TimeSpan delay)
    {
        TimeSpan clamped = delay < TimeSpan.Zero ? TimeSpan.Zero : delay;

        if (_nextRedrawDelay is null || clamped < _nextRedrawDelay.Value)
            _nextRedrawDelay = clamped;
    }

    // Push a new child element onto the layout stack, making it the current parent.
    public void OpenElement(IWidget? widget = null, LayoutStyle? style = null)
    {
        LayoutStyle layoutStyle = style ?? LayoutStyle.Default;

        if (layoutStyle.ChildGap < 0)
            throw new ArgumentOutOfRangeException(
                nameof(LayoutStyle.ChildGap),
                "Child gap cannot be negative."
            );

        LayoutNode node = new LayoutNode
        {
            Direction = layoutStyle.Direction,
            Widget = widget,
            Padding = layoutStyle.Padding,
            Gap = layoutStyle.ChildGap,
            WidthLayout = layoutStyle.Width,
            HeightLayout = layoutStyle.Height,
        };
        _layoutStack.Peek().Children.Add(node);
        _layoutStack.Push(node);
    }

    // Pop the current element off the layout stack.
    public void CloseElement()
    {
        if (_layoutStack.Peek() == _root)
            throw new InvalidOperationException("Cannot pop root element");

        _layoutStack.Pop();
    }

    // Finalize the layout tree. Sets the root to the given dimensions, then
    // distributes remaining space to any growable children at each level.
    public LayoutNode EndLayout()
    {
        if (_layoutStack.Count != 1)
            throw new InvalidOperationException("Unclosed node scope.");

        // Layout algorithm steps:
        // 1. Fit sizing widths, to determine the remaining horizontal space
        //    available for growable children
        // 2. Grow and shrink sizing widths
        // 3. Layout widgets, so width-dependent content can update its height
        // 4. Fit sizing heights
        // 5. Grow and shrink sizing heights
        // 6. Calculate final positions and alignments of elements

        MeasurePreferredSizes(_root);
        _root.Dimensions = _dimensions;
        _root.Position = new Position(0, 0);

        SizeWidths(_root);
        LayoutWidgets(_root);
        FitHeights(_root, isRoot: true);

        _root.Dimensions = _dimensions;
        SizeHeights(_root);
        PositionChildren(_root);
        EnsureFocusedWidgetExists();

        return _root;
    }

    internal WidgetInputState RegisterFocusable(string id)
    {
        _focusableIds.Add(id);

        // Focus the first widget that's registered.
        _focusedWidgetId ??= id;

        bool focused = _focusedWidgetId == id;
        return new WidgetInputState(focused, focused && _activateFocusedWidget);
    }

    private void ProcessInput(IReadOnlyList<ConsoleKeyInfo>? keys)
    {
        _activateFocusedWidget = false;
        _unconsumedKeys.Clear();

        if (keys is null)
            return;

        // Process keys in the order they were pressed. Tab navigation and
        // Enter activation are handled here eagerly so that, for example, a
        // Tab followed by Enter activates the post-Tab focus target. Any
        // other key is queued for widgets to consume via TryConsumeKey.
        foreach (ConsoleKeyInfo key in keys)
        {
            if (IsTab(key))
            {
                int direction = IsShiftTab(key) ? -1 : 1;
                MoveFocus(direction);
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                _activateFocusedWidget = true;
            }
            else
            {
                _unconsumedKeys.Enqueue(key);
            }
        }
    }

    private static bool IsTab(ConsoleKeyInfo input) =>
        input.Key == ConsoleKey.Tab || IsShiftTab(input);

    private static bool IsShiftTab(ConsoleKeyInfo input) =>
        input.Modifiers.HasFlag(ConsoleModifiers.Shift)
        && input.Key is ConsoleKey.Tab or ConsoleKey.F2;

    private void MoveFocus(int direction)
    {
        if (_focusableIds.Count == 0)
        {
            _focusedWidgetId = null;
            return;
        }

        int index = _focusedWidgetId is null ? -1 : _focusableIds.IndexOf(_focusedWidgetId);

        if (index < 0)
            index = direction > 0 ? -1 : 0;

        int nextIndex = (index + direction + _focusableIds.Count) % _focusableIds.Count;
        _focusedWidgetId = _focusableIds[nextIndex];
    }

    private void EnsureFocusedWidgetExists()
    {
        if (_focusableIds.Count == 0)
        {
            _focusedWidgetId = null;
            return;
        }

        if (_focusedWidgetId is null || !_focusableIds.Contains(_focusedWidgetId))
            _focusedWidgetId = _focusableIds[0];
    }

    private static void MeasurePreferredSizes(LayoutNode node)
    {
        foreach (LayoutNode child in node.Children)
            MeasurePreferredSizes(child);

        node.SetPreferredDimensions();
    }

    private static void SizeWidths(LayoutNode node)
    {
        node.SizeChildrenAlongWidth();

        foreach (LayoutNode child in node.Children)
            SizeWidths(child);
    }

    private static void SizeHeights(LayoutNode node)
    {
        node.SizeChildrenAlongHeight();

        foreach (LayoutNode child in node.Children)
            SizeHeights(child);
    }

    private static void LayoutWidgets(LayoutNode node)
    {
        node.LayoutWidget();

        foreach (LayoutNode child in node.Children)
            LayoutWidgets(child);
    }

    private static void FitHeights(LayoutNode node, bool isRoot = false)
    {
        foreach (LayoutNode child in node.Children)
            FitHeights(child);

        if (isRoot || node.HeightLayout.Kind == LayoutLengthKind.Fixed)
            return;

        node.SetPreferredHeight();
    }

    private static void PositionChildren(LayoutNode parent)
    {
        Position childPosition = parent.FirstChildPosition;

        foreach (LayoutNode child in parent.Children)
        {
            child.Position = childPosition;
            PositionChildren(child);
            childPosition = parent.NextChildPosition(childPosition, child);
        }
    }
}

internal readonly record struct WidgetInputState(bool Focused, bool Activated);
