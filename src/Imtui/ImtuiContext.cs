// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Collections.Concurrent;
using System.Diagnostics;

public sealed class ImtuiContext
{
    private readonly FocusNode _rootFocusNode = new("", "", isScope: true);
    private readonly List<FocusNode> _activeFocusPath = [];
    private readonly Dictionary<string, FocusNode> _focusScopes = [];
    private readonly Stack<FocusNode> _focusScopeStack = [];
    private readonly Stack<LayoutNode> _layoutStack = [];
    private readonly Queue<ConsoleKeyInfo> _unconsumedKeys = new Queue<ConsoleKeyInfo>();
    private readonly Dictionary<string, object> _stateById = [];
    private readonly HashSet<string> _stateIdsUsedThisFrame = [];
    private readonly ConcurrentDictionary<Task, byte> _wakeTasks =
        new ConcurrentDictionary<Task, byte>();

    private Action? _wakeHandler;
    private bool _activateFocusedWidget;
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

    /// <summary>
    /// Identifier of the focused widget, or null when no widget is focused.
    /// </summary>
    public string? FocusedWidgetId => GetFocusedWidgetId();

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

        // Reset the focus stack
        _focusScopeStack.Clear();
        _focusScopeStack.Push(_rootFocusNode);

        ProcessInput(input.Keys);

        ClearFocusTargets();

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
        EnsureFocusedWidgetExists();
        UpdateActiveFocusPath();
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
    /// Opens a focus scope and registers it as a focus target in its parent scope.
    /// </summary>
    /// <param name="id">Stable id for this scope within its parent scope.</param>
    /// <param name="navigation">Keys that move to the previous and next target in this scope.</param>
    public void OpenFocusScope(string id, FocusNavigation navigation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        FocusNode parent = _focusScopeStack.Peek();
        string key = parent.Key.Length == 0 ? id : $"{parent.Key}\u001F{id}";

        if (!_focusScopes.TryGetValue(key, out FocusNode? scope))
        {
            scope = new FocusNode(id, key, isScope: true);
            _focusScopes.Add(key, scope);
        }

        scope.Parent = parent;
        scope.Navigation = navigation;
        parent.Children.Add(scope);
        parent.FocusedChildId ??= id;
        _focusScopeStack.Push(scope);
    }

    /// <summary>
    /// Leaves the innermost focus scope so later focusable widgets register with its parent scope.
    /// </summary>
    public void CloseFocusScope()
    {
        if (_focusScopeStack.Peek() == _rootFocusNode)
            throw new InvalidOperationException("Cannot pop root focus scope.");

        _focusScopeStack.Pop();
    }

    /// <summary>
    /// Registers a widget as a focus target in the current focus scope.
    /// </summary>
    /// <param name="id">Stable id for the focusable widget.</param>
    /// <returns>The widget's focus and activation state for this frame.</returns>
    internal WidgetInputState RegisterFocusable(string id)
    {
        FocusNode scope = _focusScopeStack.Peek();
        scope.Children.Add(new FocusNode(id, id, isScope: false));

        // Focus the first widget that's registered.
        scope.FocusedChildId ??= id;

        bool focused = IsScopeFocused(scope) && scope.FocusedChildId == id;
        return new WidgetInputState(focused, focused && _activateFocusedWidget);
    }

    /// <summary>
    /// Applies frame input to focus state and queues keys not handled by focus navigation.
    /// </summary>
    /// <param name="keys">Keys received since the previous frame.</param>
    private void ProcessInput(IReadOnlyList<ConsoleKeyInfo>? keys)
    {
        _activateFocusedWidget = false;
        _unconsumedKeys.Clear();

        if (keys is null)
            return;

        // Process keys in the order they were pressed. Tab navigation and
        // Enter activation are handled here eagerly so that, for example, a
        // Tab followed by Enter activates the post-Tab focus target. Any
        // other key is first offered to the active focus scopes, then queued
        // for widgets to consume via TryConsumeKey if no scope handles it.
        foreach (ConsoleKeyInfo key in keys)
        {
            if (IsTab(key))
            {
                int direction = IsShiftTab(key) ? -1 : 1;
                MoveFocus(_rootFocusNode, direction);
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                _activateFocusedWidget = true;
            }
            else if (!MoveFocus(key))
            {
                _unconsumedKeys.Enqueue(key);
            }
        }
    }

    /// <summary>
    /// Returns whether a key should move focus through the root scope.
    /// </summary>
    /// <param name="input">Key to inspect.</param>
    /// <returns>True when the key is Tab or Shift+Tab.</returns>
    private static bool IsTab(ConsoleKeyInfo input) =>
        input.Key == ConsoleKey.Tab || IsShiftTab(input);

    /// <summary>
    /// Returns whether a key should move focus backward through the root scope.
    /// </summary>
    /// <param name="input">Key to inspect.</param>
    /// <returns>True when the key is Shift+Tab.</returns>
    private static bool IsShiftTab(ConsoleKeyInfo input) =>
        input.Modifiers.HasFlag(ConsoleModifiers.Shift)
        && input.Key is ConsoleKey.Tab or ConsoleKey.F2;

    /// <summary>
    /// Tries to dispatch a navigation key to the active focus scopes, innermost first.
    /// </summary>
    /// <param name="input">Key to dispatch.</param>
    /// <returns>True when a focus scope handled the key.</returns>
    private bool MoveFocus(ConsoleKeyInfo input)
    {
        for (int index = _activeFocusPath.Count - 1; index >= 0; index--)
        {
            FocusNode scope = _activeFocusPath[index];

            if (!scope.Navigation.TryGetDirection(input, out int direction))
                continue;

            MoveFocus(scope, direction);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves focus within one scope by the requested direction.
    /// </summary>
    /// <param name="scope">Scope whose focused target should move.</param>
    /// <param name="direction">Positive moves next; negative moves previous.</param>
    private void MoveFocus(FocusNode scope, int direction)
    {
        if (scope.Children.Count == 0)
        {
            scope.FocusedChildId = null;
            UpdateActiveFocusPath();
            return;
        }

        int index = scope.FocusedChildId is null
            ? -1
            : scope.Children.FindIndex(child => child.Id == scope.FocusedChildId);

        if (index < 0)
            index = direction > 0 ? -1 : 0;

        int nextIndex = (index + direction + scope.Children.Count) % scope.Children.Count;
        scope.FocusedChildId = scope.Children[nextIndex].Id;
        UpdateActiveFocusPath();
    }

    /// <summary>
    /// Reconciles focused targets after the current frame has registered its focus tree.
    /// </summary>
    private void EnsureFocusedWidgetExists()
    {
        EnsureFocusedTargetExists(_rootFocusNode);
    }

    /// <summary>
    /// Reconciles one scope and its child scopes so each focused target still exists.
    /// </summary>
    /// <param name="scope">Scope to reconcile.</param>
    private static void EnsureFocusedTargetExists(FocusNode scope)
    {
        if (scope.Children.Count == 0)
        {
            scope.FocusedChildId = null;
            return;
        }

        if (
            scope.FocusedChildId is null
            || !scope.Children.Exists(child => child.Id == scope.FocusedChildId)
        )
            scope.FocusedChildId = scope.Children[0].Id;

        foreach (FocusNode child in scope.Children)
        {
            if (child.IsScope)
                EnsureFocusedTargetExists(child);
        }
    }

    /// <summary>
    /// Clears targets registered during the previous frame while preserving focused ids.
    /// </summary>
    private void ClearFocusTargets()
    {
        _rootFocusNode.Children.Clear();
        foreach (FocusNode scope in _focusScopes.Values)
            scope.Children.Clear();
    }

    /// <summary>
    /// Returns whether a scope is on the active focus path.
    /// </summary>
    /// <param name="scope">Scope to inspect.</param>
    /// <returns>True when every ancestor is focused on this scope path.</returns>
    private bool IsScopeFocused(FocusNode scope)
    {
        if (scope == _rootFocusNode)
            return true;

        return scope.Parent is not null
            && IsScopeFocused(scope.Parent)
            && scope.Parent.FocusedChildId == scope.Id;
    }

    /// <summary>
    /// Caches the active focus scope path for the next input dispatch.
    /// </summary>
    private void UpdateActiveFocusPath()
    {
        _activeFocusPath.Clear();
        FocusNode scope = _rootFocusNode;
        _activeFocusPath.Add(scope);

        while (scope.FocusedChildId is not null)
        {
            FocusNode? child = scope.Children.Find(child => child.Id == scope.FocusedChildId);

            if (child is null || !child.IsScope)
                return;

            scope = child;
            _activeFocusPath.Add(scope);
        }
    }

    /// <summary>
    /// Finds the focused leaf widget id by following focused targets through scopes.
    /// </summary>
    /// <returns>The focused widget id, or null when focus does not end at a widget.</returns>
    private string? GetFocusedWidgetId()
    {
        FocusNode scope = _rootFocusNode;

        while (scope.FocusedChildId is not null)
        {
            FocusNode? child = scope.Children.Find(child => child.Id == scope.FocusedChildId);

            if (child is null)
                return null;

            if (!child.IsScope)
                return child.Id;

            scope = child;
        }

        return null;
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
