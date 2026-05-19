// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Collections.Concurrent;
using System.Diagnostics;

public sealed class ImtuiContext
{
    // Stack of scope ids currently open during widget construction. Cleared
    // every BeginLayout. Combined with a widget's id, this gives the widget's
    // full root-to-leaf path.
    private readonly Stack<string> _focusScopeStack = [];

    // Full root-to-leaf paths for every focusable widget registered this frame,
    // in registration order. The last element of each path is the widget id;
    // preceding elements are the enclosing scope ids from root to innermost.
    private readonly List<string[]> _focusableWidgets = [];

    // Root-to-leaf id path identifying the focused widget. Empty when no
    // widget is focused. Persists across frames; reconciled at EndLayout
    // against this frame's registered widgets.
    private string[] _focusedPath = [];

    private readonly Stack<LayoutNode> _layoutStack = [];
    private readonly KeyboardDispatcher _keyboard = new KeyboardDispatcher();
    private readonly Dictionary<string, object> _stateById = [];
    private readonly HashSet<string> _stateIdsUsedThisFrame = [];
    private readonly ConcurrentDictionary<Task, byte> _wakeTasks =
        new ConcurrentDictionary<Task, byte>();

    private Action? _wakeHandler;
    private LayoutNode _root = new LayoutNode();
    private Dimensions _dimensions;
    private TimeSpan _time;
    private TimeSpan? _nextRedrawDelay;
    private long _frameStartTimestamp;
    private TimeSpan _lastFrameTime;
    private bool _quitRequested;

    // The active theme. Built-in widget extension methods read from this
    // whenever the caller does not supply an explicit color. May be reassigned
    // at any time — including mid-frame from inside the run loop callback —
    // and the change takes effect on the next widget construction.
    public Theme Theme { get; set; } = Theme.Default;

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

    // Consumes keypresses not consumed by a focused widget. Call this after
    // constructing focusable widgets to handle application-level shortcuts.
    // Return true from the handler to consume the key; return false to leave it
    // available for another handler. Any remaining Tab/Shift-Tab keys move
    // focus at EndLayout.
    public bool ConsumeKeys(Func<ConsoleKeyInfo, bool> handleKey) =>
        _keyboard.ConsumeKeys(handleKey);

    // Consumes every keypress not consumed by a focused widget.
    public bool ConsumeKeys(Action<ConsoleKeyInfo> handleKey) => _keyboard.ConsumeKeys(handleKey);

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

    /// <summary>
    /// Identifier of the focused widget, or null when no widget is focused.
    /// </summary>
    public string? FocusedWidgetId => _focusedPath.Length == 0 ? null : _focusedPath[^1];

    // Wall-clock time spent on the most recently completed frame, measured
    // from BeginLayout until EndFrame. Excludes any time the application
    // spends waiting for input between frames. Zero until the first frame
    // has been completed with a call to EndFrame.
    public TimeSpan LastFrameTime => _lastFrameTime;

    // Whether the application has requested to quit after the current frame.
    // The run loop reads this after the frame callback returns and exits the
    // loop if true. Reset at the start of every BeginLayout.
    internal bool QuitRequested => _quitRequested;

    // Request that the run loop exit after the current frame finishes
    // rendering. The frame in progress completes normally — including
    // layout, render, and present — so the user sees the final state of the
    // UI before the application returns. Has no effect when called outside
    // of a frame.
    public void QuitAfterThisFrame()
    {
        _quitRequested = true;
    }

    public void BeginLayout(Dimensions dimensions, FrameInput input = default)
    {
        _frameStartTimestamp = Stopwatch.GetTimestamp();
        FrameCount += 1;
        _dimensions = dimensions;
        _time = input.Time;
        _nextRedrawDelay = null;
        _quitRequested = false;
        _root = new LayoutNode { Dimensions = dimensions, Position = new Position(0, 0) };

        // Reset the layout stack
        _layoutStack.Clear();
        _layoutStack.Push(_root);

        // Reset per-frame focus registration. _focusedPath persists across
        // frames and is reconciled at EndLayout once registrations are known.
        _focusScopeStack.Clear();
        _focusableWidgets.Clear();

        // Load this frame's keypresses into the dispatcher before widgets run.
        // This also clears per-key consumed state from the previous frame.
        _keyboard.BeginFrame(input.Keys);

        // Reset state usage tracking. Any state not used during this frame
        // will be pruned in EndLayout.
        _stateIdsUsedThisFrame.Clear();
    }

    // Retrieves the State<T> associated with `id`, creating it from `createInitial`
    // on first use. The same instance is returned for the same id across frames,
    // so widgets can mutate State<T>.Value and observe the change on the next
    // frame.
    //
    // State whose id is not requested during a frame is pruned at EndLayout —
    // a widget that disappears (for example because it lives inside an `if`
    // branch that is no longer taken) loses its state, which is the
    // conventional immediate-mode behavior.
    //
    // Throws InvalidOperationException if `id` was previously associated with a
    // different value type (programmer error: id collision).
    public State<T> UseState<T>(string id, Func<T> createInitial)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(createInitial);

        _stateIdsUsedThisFrame.Add(id);

        if (_stateById.TryGetValue(id, out object? existing))
        {
            if (existing is State<T> typed)
                return typed;

            throw new InvalidOperationException(
                $"State id '{id}' was previously associated with a different type."
            );
        }

        State<T> created = new State<T>(createInitial());
        _stateById[id] = created;
        return created;
    }

    // Convenience overload that takes an initial value directly. The value is
    // evaluated by the caller every frame even when the state already exists,
    // so prefer the factory overload when constructing the initial value is
    // non-trivial.
    public State<T> UseState<T>(string id, T initialValue) => UseState(id, () => initialValue);

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
        EnsureFocusedTargetExists();
        ConsumeDefaultFocusNavigation();
        PruneUnusedState();

        return _root;
    }

    private void PruneUnusedState()
    {
        if (_stateById.Count == _stateIdsUsedThisFrame.Count)
            return;

        List<string>? toRemove = null;
        foreach (string id in _stateById.Keys)
        {
            if (_stateIdsUsedThisFrame.Contains(id))
                continue;

            toRemove ??= [];
            toRemove.Add(id);
        }

        if (toRemove is null)
            return;

        foreach (string id in toRemove)
            _stateById.Remove(id);
    }

    /// <summary>
    /// Pushes a focus scope so later focusable widgets register inside it.
    /// </summary>
    /// <param name="id">Stable id for this scope within its parent scope.</param>
    public void PushFocusScope(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _focusScopeStack.Push(id);
    }

    /// <summary>
    /// Leaves the innermost focus scope so later focusable widgets register with its parent scope.
    /// </summary>
    public void CloseFocusScope()
    {
        if (_focusScopeStack.Count == 0)
            throw new InvalidOperationException("Cannot pop root focus scope.");

        _focusScopeStack.Pop();
    }

    /// <summary>
    /// Registers a widget as a focus target in the current focus scope.
    /// </summary>
    /// <param name="id">Stable id for the focusable widget.</param>
    /// <returns>The widget's focus and keyboard input state for this frame.</returns>
    internal WidgetInputState RegisterFocusable(string id)
    {
        string[] path = BuildCurrentPath(id);
        _focusableWidgets.Add(path);

        // Default focus: the very first widget registered while no focus
        // exists becomes focused for this frame. Mirrors the previous
        // FocusedChildId ??= id behavior so the first frame already returns
        // focused=true to the first widget.
        if (_focusedPath.Length == 0)
            _focusedPath = path;

        return new WidgetInputState(PathsEqual(path, _focusedPath), _keyboard);
    }

    /// <summary>
    /// Uses remaining Tab/Shift+Tab keys as the built-in focus navigation fallback.
    /// Widgets and application handlers get first chance to consume those keys.
    /// </summary>
    private void ConsumeDefaultFocusNavigation()
    {
        _keyboard.ConsumeKeys(key =>
        {
            bool isShiftTab =
                key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                && key.Key is ConsoleKey.Tab or ConsoleKey.F2;

            if (key.Key != ConsoleKey.Tab && !isShiftTab)
                return false;

            int offset = isShiftTab ? -1 : 1;
            if (MoveFocusBy(offset))
                RequestRedrawIn(TimeSpan.Zero);

            return true;
        });
    }

    private bool MoveFocusBy(int offset)
    {
        // A frame can have zero focusable widgets if the UI hides every
        // interactive element. Clear the stale focus target so debug output
        // and later navigation do not point at a widget that no longer exists.
        if (_focusableWidgets.Count == 0)
        {
            bool changed = _focusedPath.Length > 0;
            _focusedPath = [];
            return changed;
        }

        int index = FindFocusableIndex(_focusedPath);

        // If focus was missing or pointed at a disappeared widget, start just
        // before the first item when moving forward or just after the last
        // item when moving backward.
        if (index < 0)
            index = offset > 0 ? -1 : 0;

        int nextIndex = (index + offset + _focusableWidgets.Count) % _focusableWidgets.Count;
        string[] next = _focusableWidgets[nextIndex];
        bool moved = !PathsEqual(next, _focusedPath);

        _focusedPath = next;
        return moved;
    }

    /// <summary>
    /// Reconciles _focusedPath against the focusables registered this frame.
    /// If the focused widget vanished, picks the first focusable that shares
    /// the longest scope prefix so focus stays near where the user left it.
    /// </summary>
    private void EnsureFocusedTargetExists()
    {
        if (_focusableWidgets.Count == 0)
        {
            _focusedPath = [];
            return;
        }

        if (FindFocusableIndex(_focusedPath) >= 0)
            return;

        // Try progressively shorter prefixes of the stale path to keep focus
        // inside the deepest still-existing scope.
        for (int prefixLength = _focusedPath.Length - 1; prefixLength > 0; prefixLength--)
        {
            for (int i = 0; i < _focusableWidgets.Count; i++)
            {
                if (PathStartsWith(_focusableWidgets[i], _focusedPath, prefixLength))
                {
                    _focusedPath = _focusableWidgets[i];
                    return;
                }
            }
        }

        _focusedPath = _focusableWidgets[0];
    }

    private string[] BuildCurrentPath(string widgetId)
    {
        int scopeCount = _focusScopeStack.Count;
        string[] path = new string[scopeCount + 1];

        // Stack enumerates top-to-bottom (innermost-to-outermost); fill from
        // the end so the resulting array is ordered root-to-leaf.
        int index = scopeCount - 1;
        foreach (string scopeId in _focusScopeStack)
            path[index--] = scopeId;

        path[scopeCount] = widgetId;
        return path;
    }

    private int FindFocusableIndex(string[] path)
    {
        if (path.Length == 0)
            return -1;

        for (int i = 0; i < _focusableWidgets.Count; i++)
        {
            if (PathsEqual(_focusableWidgets[i], path))
                return i;
        }

        return -1;
    }

    private static bool PathsEqual(string[] a, string[] b)
    {
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    private static bool PathStartsWith(string[] path, string[] prefix, int prefixLength)
    {
        if (path.Length < prefixLength)
            return false;

        for (int i = 0; i < prefixLength; i++)
            if (path[i] != prefix[i])
                return false;

        return true;
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
