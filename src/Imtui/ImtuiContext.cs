// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Diagnostics;
using Imtui.Rendering;
using Imtui.Widgets;

namespace Imtui;

public class ImtuiContext
{
    private readonly Stack<WidgetID> _idStack = new();
    private readonly WidgetStateStorage _stateStorage = new();
    private readonly List<WidgetID> _focusOrder = [];
    private readonly Stack<LayoutFrame> _layoutStack = new();

    private Screen _previousScreen;
    private Screen _currentScreen;
    private WidgetID? _focusedWidgetId;

    /// <summary>
    /// Creates a new Imtui context.
    /// </summary>
    public ImtuiContext()
    {
        Size size = CurrentTerminalSize;
        _previousScreen = new Screen(size);
        _currentScreen = new Screen(size);
        _idStack.Push(WidgetID.Root);
        _layoutStack.Push(CreateRootFrame());
    }

    public Screen CurrentScreen => _currentScreen;
    public ImtuiInput ThisFrameInput { get; private set; }
    public FocusState FocusState { get; private set; }
    public WidgetID? FocusedWidgetId => _focusedWidgetId;

    public void NewFrame(Size? size = null, ImtuiInput input = default)
    {
        Debug.Assert(
            _idStack.Count == 1,
            $"Unbalanced PushId/PopId: expected stack depth 1, got {_idStack.Count}"
        );

        Size nextFrameSize = size.GetValueOrDefault(CurrentTerminalSize);
        _previousScreen = _currentScreen;
        _currentScreen = new Screen(nextFrameSize);

        // Reset ID stack
        _idStack.Clear();
        _idStack.Push(WidgetID.Root);

        // Reset layout frame stack to a single root frame
        _layoutStack.Clear();
        _layoutStack.Push(CreateRootFrame());

        ThisFrameInput = input;
        FocusAction focusAction = GetFocusAction(input);
        ApplyFocusAction(focusAction);

        WidgetID focusedWidgetId = _focusedWidgetId.GetValueOrDefault();
        WidgetID activeWidgetId =
            _focusedWidgetId is not null && ShouldActivateFocusedWidget(input)
                ? focusedWidgetId
                : default;
        FocusState = new FocusState(focusedWidgetId, activeWidgetId);
        _focusOrder.Clear();
    }

    public string RenderFrame() => Renderer.Render(_previousScreen, _currentScreen);

    public WidgetID GetId(string label) => WidgetID.Hash(label.AsSpan(), _idStack.Peek());

    public WidgetID GetId(int intId) => WidgetID.Hash(intId, _idStack.Peek());

    public void PushId(string label) => _idStack.Push(GetId(label));

    public void PushId(int intId) => _idStack.Push(GetId(intId));

    public void PopId()
    {
        if (_idStack.Count > 1)
            _idStack.Pop();
    }

    public T GetWidgetState<T>(WidgetID id)
        where T : class, new() => _stateStorage.GetOrCreate<T>(id);

    public bool IsFocused(WidgetID id) => FocusedWidgetId is not null && FocusState.Focused == id;

    public bool IsActivated(WidgetID id) => IsFocused(id) && FocusState.Active == id;

    public void Submit(IWidget widget)
    {
        RegisterWidget(widget);
        widget.Execute(this);
        AdvanceCursor(_layoutStack.Peek());
    }

    public TResult Submit<TResult>(IStatefulWidget<TResult> widget)
    {
        RegisterWidget(widget);
        TResult result = widget.Execute(this);
        AdvanceCursor(_layoutStack.Peek());
        return result;
    }

    public CellPosition AllocateWidgetPosition()
    {
        LayoutFrame frame = _layoutStack.Peek();
        return new CellPosition(frame.CursorX, frame.CursorY);
    }

    public void WriteCell(CellPosition position, Cell cell)
    {
        LayoutFrame frame = _layoutStack.Peek();
        Cell resolved = ApplyDefaultStyle(cell, frame.DefaultStyle);
        if (position.X + 1 > frame.MaxX)
            frame.MaxX = position.X + 1;
        if (position.Y + 1 > frame.MaxY)
            frame.MaxY = position.Y + 1;

        if (_currentScreen.IsInBounds(position))
            _currentScreen[position] = resolved;
    }

    /// <summary>
    /// Pushes a new layout frame onto the stack. Subsequent calls to
    /// <see cref="AllocateWidgetPosition"/>, <see cref="WriteCell"/>, and
    /// <see cref="Submit(IWidget)"/> will operate against the new frame until
    /// it is popped via <see cref="PopLayoutFrame"/>.
    /// </summary>
    public void PushLayoutFrame(
        int originX,
        int originY,
        LayoutDirection direction = LayoutDirection.Vertical,
        CellStyle defaultStyle = default
    )
    {
        LayoutFrame frame = new(originX, originY, direction, defaultStyle);
        _layoutStack.Push(frame);
    }

    /// <summary>
    /// Pops the topmost layout frame and returns the bounding box of cells
    /// that were written while it was on top of the stack.
    /// </summary>
    public LayoutMeasurement PopLayoutFrame()
    {
        Debug.Assert(_layoutStack.Count > 1, "Cannot pop the root layout frame.");
        LayoutFrame frame = _layoutStack.Pop();
        int width = Math.Max(0, frame.MaxX - frame.OriginX);
        int height = Math.Max(0, frame.MaxY - frame.OriginY);
        return new LayoutMeasurement(frame.OriginX, frame.OriginY, width, height);
    }

    private void RegisterWidget(IWidget widget)
    {
        if (widget.IsFocusable)
            RegisterFocusableWidget(widget.ID);
    }

    private void RegisterFocusableWidget(WidgetID id)
    {
        if (!_focusOrder.Contains(id))
            _focusOrder.Add(id);

        _focusedWidgetId ??= id;

        if (_focusedWidgetId == id)
        {
            WidgetID activeWidgetId = ShouldActivateFocusedWidget(ThisFrameInput) ? id : default;
            FocusState = new FocusState(id, activeWidgetId);
        }
    }

    private void ApplyFocusAction(FocusAction action)
    {
        if (action == FocusAction.None || _focusedWidgetId is null || _focusOrder.Count == 0)
            return;

        int currentIndex = _focusOrder.IndexOf(_focusedWidgetId.Value);
        if (currentIndex < 0)
            return;

        int step = action == FocusAction.FocusNext ? 1 : -1;
        int nextIndex = Utilities.Wrap(
            index: currentIndex + step,
            min: 0,
            max: _focusOrder.Count - 1
        );
        _focusedWidgetId = _focusOrder[nextIndex];
    }

    private static FocusAction GetFocusAction(ImtuiInput input) =>
        input.HasKey(ImtuiKey.Tab) ? FocusAction.FocusNext
        : input.HasKey(ImtuiKey.ShiftTab) ? FocusAction.FocusPrevious
        : FocusAction.None;

    private static bool ShouldActivateFocusedWidget(ImtuiInput input) =>
        input.HasKey(ImtuiKey.Enter) || input.HasKey(ImtuiKey.Space);

    private static Size CurrentTerminalSize => new(Console.WindowWidth, Console.WindowHeight);

    private static LayoutFrame CreateRootFrame() =>
        new(originX: 0, originY: 0, LayoutDirection.Vertical, defaultStyle: default);

    private static void AdvanceCursor(LayoutFrame frame)
    {
        switch (frame.Direction)
        {
            case LayoutDirection.Vertical:
                if (frame.MaxY > frame.CursorY)
                    frame.CursorY = frame.MaxY;
                break;
            case LayoutDirection.Horizontal:
                if (frame.MaxX > frame.CursorX)
                    frame.CursorX = frame.MaxX;
                break;
            case LayoutDirection.None:
                break;
        }
    }

    private static Cell ApplyDefaultStyle(Cell cell, CellStyle defaultStyle)
    {
        Color foreground =
            cell.Style.Foreground.Kind == ColorKind.Default
                ? defaultStyle.Foreground
                : cell.Style.Foreground;
        Color background =
            cell.Style.Background.Kind == ColorKind.Default
                ? defaultStyle.Background
                : cell.Style.Background;

        if (foreground == cell.Style.Foreground && background == cell.Style.Background)
            return cell;

        return new Cell(cell.Glyph, new CellStyle(foreground, background));
    }

    private enum FocusAction
    {
        None,
        FocusNext,
        FocusPrevious,
    }
}
