// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

public sealed class AsphaltContext
{
    // Sentinel id for the implicit root focus scope. The empty string is
    // reserved and cannot be used by callers (ClaimFocusId rejects it).
    private const string RootScopeId = "";

    // Persistent per-scope focus state. Each scope (including the root) owns
    // a list of children registered this frame and a remembered FocusedChild
    // that survives across frames. Children may be either widget ids (leaves)
    // or nested scope ids; a child is a scope iff it is itself a key in this
    // dictionary. Scope and widget ids must be globally unique, enforced via
    // _focusIdsSeenThisFrame.
    private readonly Dictionary<string, FocusScope> _focusScopes = new()
    {
        [RootScopeId] = new FocusScope { SeenThisFrame = true },
    };

    // Open scope ids during widget construction, root at the bottom. Cleared
    // and re-seeded with the root every BeginLayout.
    private readonly Stack<string> _openFocusScopes = new();

    // All scope and widget ids registered this frame. Used to detect id
    // collisions (since scope-vs-leaf is implicit in _focusScopes membership).
    private readonly HashSet<string> _focusIdsSeenThisFrame = [];

    // Stack of currently-open WidgetScopes. Each entry is the focus state of
    // the widget that opened the scope. KeyDown checks the top of this stack:
    // when non-empty and the top is not focused, key checks short-circuit and
    // do not consume. Cleared every BeginLayout.
    private readonly Stack<bool> _widgetInputScopes = new();

    // Shortcut hints registered during the current frame via
    // AddShortcutHint. Cleared every BeginLayout.
    private readonly List<ShortcutHint> _shortcutHints = new();

    /// <summary>
    /// Shortcut hints registered for the current frame via
    /// AddShortcutHint. Reflects registration order and is cleared at the
    /// start of every frame.
    /// </summary>
    public IReadOnlyList<ShortcutHint> ShortcutHints => _shortcutHints;

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
    // available for another handler. Any remaining arrow keys move focus at
    // EndLayout: Up/Down within the focused widget's scope, Left/Right between
    // sibling scopes.
    public bool ConsumeKeys(Func<ConsoleKeyInfo, bool> handleKey) =>
        _keyboard.ConsumeKeys(handleKey);

    // Consumes every keypress not consumed by a focused widget.
    public bool ConsumeKeys(Action<ConsoleKeyInfo> handleKey) => _keyboard.ConsumeKeys(handleKey);

    /// <summary>
    /// Consumes a matching unhandled keypress for this frame and returns whether one was found.
    /// </summary>
    /// <remarks>
    /// When called inside a <see cref="WidgetScope"/> (e.g. the body of
    /// <c>using (context.SelectableList(...))</c>), the check is gated on that
    /// widget being focused — unfocused widgets never see keys via this method.
    /// When called outside any widget scope, the check is global, useful for
    /// application-level hotkeys.
    /// </remarks>
    public bool KeyDown(ConsoleKey key)
    {
        if (!IsCurrentWidgetInputScopeFocused())
            return false;
        return _keyboard.ConsumeKeys(k => k.Key == key);
    }

    /// <summary>
    /// for focus-gating rules.
    /// </summary>
    public bool KeyDown(ConsoleKey key, ConsoleModifiers modifiers)
    {
        if (!IsCurrentWidgetInputScopeFocused())
            return false;
        return _keyboard.ConsumeKeys(k => k.Key == key && k.Modifiers == modifiers);
    }

    /// <summary>
    /// Registers a shortcut hint for the current frame. When called inside a
    /// widget input scope the hint is only recorded when that widget is
    /// focused; outside any widget scope the hint is always recorded (useful
    /// for application-level hotkeys like Q: Quit). Read the accumulated
    /// hints back via ShortcutHints when rendering a shortcut bar.
    /// </summary>
    public void AddShortcutHint(string label, string value)
    {
        if (!IsCurrentWidgetInputScopeFocused())
            return;
        _shortcutHints.Add(new ShortcutHint(label, value));
    }

    // Returns true if there is no widget input scope active, or if the
    // innermost active scope corresponds to a focused widget. Shared by
    // KeyDown and AddShortcutHint so the two stay in lock-step.
    private bool IsCurrentWidgetInputScopeFocused() =>
        _widgetInputScopes.Count == 0 || _widgetInputScopes.Peek();

    /// <summary>
    /// Pushes a widget input scope, gating subsequent <see cref="KeyDown(ConsoleKey)"/>
    /// calls on <paramref name="focused"/>. Always paired with
    /// <see cref="PopWidgetInputScope"/> via a <see cref="WidgetScope"/>.
    /// </summary>
    internal void PushWidgetInputScope(bool focused) => _widgetInputScopes.Push(focused);

    /// <summary>Pops the innermost widget input scope.</summary>
    internal void PopWidgetInputScope()
    {
        if (_widgetInputScopes.Count == 0)
            throw new InvalidOperationException("No widget input scope to pop.");
        _widgetInputScopes.Pop();
    }

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
    public string? FocusedWidgetId
    {
        get
        {
            string current = RootScopeId;
            while (_focusScopes.TryGetValue(current, out FocusScope? scope))
            {
                if (scope.FocusedChild is null)
                    return null;
                if (!_focusScopes.ContainsKey(scope.FocusedChild))
                    return scope.FocusedChild;
                current = scope.FocusedChild;
            }
            return null;
        }
    }

    /// <summary>
    /// True if <paramref name="id"/> is the focused widget or an ancestor
    /// focus scope of it. Equivalent to "is this id on the focused path
    /// from the root scope down to the focused leaf".
    /// </summary>
    public bool IsFocused(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        string current = RootScopeId;
        while (_focusScopes.TryGetValue(current, out FocusScope? scope))
        {
            if (scope.FocusedChild is null)
                return false;
            if (scope.FocusedChild == id)
                return true;
            current = scope.FocusedChild;
        }
        return false;
    }

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

        // Reset per-frame focus registration. Scopes (and their remembered
        // FocusedChild) persist across frames and are reconciled at EndLayout.
        foreach (FocusScope scope in _focusScopes.Values)
        {
            scope.Children.Clear();
            scope.SeenThisFrame = false;
        }
        _focusScopes[RootScopeId].SeenThisFrame = true;
        _openFocusScopes.Clear();
        _openFocusScopes.Push(RootScopeId);
        _focusIdsSeenThisFrame.Clear();
        _widgetInputScopes.Clear();
        _shortcutHints.Clear();

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
        // A consumed key is assumed to have mutated app state, so schedule an
        // immediate follow-up frame. This keeps input-driven updates visible
        // without requiring callers to nudge the loop manually.
        if (_keyboard.EndFrame())
            RequestRedrawIn(TimeSpan.Zero);

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
        ReconcileFocusScopes();
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
    /// <param name="id">Globally-unique stable id for this scope.</param>
    internal void PushFocusScope(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ClaimFocusId(id);

        if (!_focusScopes.TryGetValue(id, out FocusScope? scope))
        {
            scope = new FocusScope();
            _focusScopes[id] = scope;
        }
        scope.SeenThisFrame = true;

        AddChildToCurrentScope(id);
        _openFocusScopes.Push(id);
    }

    /// <summary>
    /// Leaves the innermost focus scope so later focusable widgets register with its parent scope.
    /// </summary>
    internal void CloseFocusScope()
    {
        if (_openFocusScopes.Count <= 1)
            throw new InvalidOperationException("Cannot pop root focus scope.");

        _openFocusScopes.Pop();
    }

    /// <summary>
    /// Registers a widget as a focus target in the current focus scope.
    /// </summary>
    /// <param name="id">Globally-unique stable id for the focusable widget.</param>
    /// <returns>The widget's focus and keyboard input state for this frame.</returns>
    internal WidgetInputState RegisterFocusable(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ClaimFocusId(id);
        AddChildToCurrentScope(id);
        return new WidgetInputState(FocusedWidgetId == id, _keyboard);
    }

    private void ClaimFocusId(string id)
    {
        if (!_focusIdsSeenThisFrame.Add(id))
            throw new InvalidOperationException($"Duplicate focus id '{id}' this frame.");
    }

    private void AddChildToCurrentScope(string childId)
    {
        FocusScope parent = _focusScopes[_openFocusScopes.Peek()];
        parent.Children.Add(childId);
        parent.FocusedChild ??= childId;
    }

    /// <summary>
    /// Arrow keys as the built-in focus navigation fallback. All four keys
    /// share one rule: walk the focused scope chain from a starting depth
    /// outward, and move FocusedChild in the first scope where movement
    /// succeeds. Up/Down start at the innermost scope (depth 0). Left/Right
    /// start at its parent (depth 1) so they always cross a scope boundary
    /// — which is what makes "Down past the end of a scope" behave like
    /// "Right" (both bubble out and move at the next ancestor).
    /// </summary>
    private void ConsumeDefaultFocusNavigation()
    {
        _keyboard.ConsumeKeys(key =>
            key.Key switch
            {
                ConsoleKey.UpArrow => MoveFocus(startDepth: 0, direction: -1),
                ConsoleKey.DownArrow => MoveFocus(startDepth: 0, direction: +1),
                ConsoleKey.LeftArrow => MoveFocus(startDepth: 1, direction: -1),
                ConsoleKey.RightArrow => MoveFocus(startDepth: 1, direction: +1),
                ConsoleKey.H => MoveFocus(startDepth: 1, direction: -1),
                ConsoleKey.J => MoveFocus(startDepth: 0, direction: +1),
                ConsoleKey.K => MoveFocus(startDepth: 0, direction: -1),
                ConsoleKey.L => MoveFocus(startDepth: 1, direction: +1),
                _ => false,
            }
        );
    }

    // Walks the focused scope chain starting `startDepth` scopes out from
    // the innermost (0 = innermost itself, 1 = its parent, ...). Moves
    // FocusedChild by `direction` (±1) in the first scope where movement
    // is possible. Always returns true so arrow keys are consumed even
    // when nothing moves; the consumption itself causes EndFrame to
    // schedule a follow-up redraw.
    private bool MoveFocus(int startDepth, int direction)
    {
        foreach (FocusScope scope in FocusedScopeChain().Skip(startDepth))
        {
            if (MoveFocusedChild(scope, direction))
                break;
        }
        return true;
    }

    // Bumps the scope's FocusedChild by `offset` positions in its children
    // list. Returns false (no-op) if there is no focused child, if the
    // focused child is stale, or if the target index is out of range — the
    // caller relies on the false return to keep bubbling outward.
    private static bool MoveFocusedChild(FocusScope scope, int offset)
    {
        if (scope.FocusedChild is null)
            return false;

        int currentIndex = scope.Children.IndexOf(scope.FocusedChild);
        if (currentIndex < 0)
            return false;

        int nextIndex = currentIndex + offset;
        if (nextIndex < 0 || nextIndex >= scope.Children.Count)
            return false;

        scope.FocusedChild = scope.Children[nextIndex];
        return true;
    }

    // Walks from the root scope through FocusedChild links and returns the
    // scopes visited, innermost first. Iteration order matches how arrow
    // keys consume scopes: innermost gets first chance, then we bubble
    // outward.
    private IEnumerable<FocusScope> FocusedScopeChain()
    {
        List<FocusScope> chain = [];
        string current = RootScopeId;
        while (_focusScopes.TryGetValue(current, out FocusScope? scope))
        {
            chain.Add(scope);
            if (scope.FocusedChild is null || !_focusScopes.ContainsKey(scope.FocusedChild))
                break;
            current = scope.FocusedChild;
        }

        for (int i = chain.Count - 1; i >= 0; i--)
            yield return chain[i];
    }

    // Removes scopes not visited this frame and clamps each remaining scope's
    // FocusedChild to its current Children list. Per-scope local fix; no path
    // arithmetic.
    private void ReconcileFocusScopes()
    {
        List<string>? toRemove = null;
        foreach ((string id, FocusScope scope) in _focusScopes)
        {
            if (id != RootScopeId && !scope.SeenThisFrame)
            {
                toRemove ??= [];
                toRemove.Add(id);
                continue;
            }

            if (scope.FocusedChild is not null && !scope.Children.Contains(scope.FocusedChild))
                scope.FocusedChild = scope.Children.Count > 0 ? scope.Children[0] : null;
        }

        if (toRemove is null)
            return;

        foreach (string id in toRemove)
            _focusScopes.Remove(id);
    }

    private sealed class FocusScope
    {
        public List<string> Children { get; } = [];
        public string? FocusedChild { get; set; }
        public bool SeenThisFrame { get; set; }
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
