// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Diagnostics;

public sealed class ImtuiContext
{
    private readonly List<string> _focusableIds = [];
    private readonly Stack<LayoutNode> _layoutStack = [];
    private bool _activateFocusedWidget;
    private string? _focusedWidgetId;
    private LayoutNode _root = new LayoutNode();
    private Dimensions _dimensions;
    private TimeSpan _time;
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
        _root = new LayoutNode { Dimensions = dimensions, Position = new Position(0, 0) };
        _layoutStack.Clear();
        _layoutStack.Push(_root);
        ProcessInput(input.Key);
        _focusableIds.Clear();
    }

    // Marks the end of a frame's rendering work. Call this after presenting
    // the frame and before waiting for the next input so that LastFrameTime
    // measures only the work spent producing the frame.
    public void EndFrame()
    {
        _lastFrameTime = Stopwatch.GetElapsedTime(_frameStartTimestamp);
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

    private void ProcessInput(ConsoleKeyInfo? input)
    {
        _activateFocusedWidget = false;

        if (input is null)
            return;

        if (IsTab(input.Value))
        {
            int direction = IsShiftTab(input.Value) ? -1 : 1;
            MoveFocus(direction);
        }
        else if (input.Value.Key == ConsoleKey.Enter)
        {
            _activateFocusedWidget = true;
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
