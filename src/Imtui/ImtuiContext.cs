// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Diagnostics;
using System.Text;
using Imtui.Rendering;
using Imtui.Widgets;

namespace Imtui;

public class ImtuiContext
{
    private readonly Stack<WidgetID> _idStack = new();
    private readonly WidgetStateStorage _stateStorage = new();
    private readonly List<WidgetID> _focusOrder = [];

    private Screen _previousScreen;
    private Screen _currentScreen;
    private int _widgetCursorY;
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

        ResetIdStack();
        ThisFrameInput = input;
        _widgetCursorY = 0;
        bool activateFocusedWidget = ShouldActivateFocusedWidget(input);
        _focusedWidgetId = GetFocusedWidgetIdForFrame(input);
        FocusState = CreateFocusState(_focusedWidgetId, activateFocusedWidget);
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
    }

    public TResult Submit<TResult>(IStatefulWidget<TResult> widget)
    {
        RegisterWidget(widget);
        TResult result = widget.Execute(this);
        return result;
    }

    public CellPosition AllocateWidgetPosition() => new(0, _widgetCursorY++);

    public void WriteCell(CellPosition position, Cell cell)
    {
        if (_currentScreen.IsInBounds(position))
            _currentScreen[position] = cell;
    }

    public void WriteText(CellPosition position, string text, CellStyle style)
    {
        int x = position.X;

        foreach (Rune glyph in text.EnumerateRunes())
        {
            WriteCell(new CellPosition(x, position.Y), new Cell(glyph, style));
            x++;
        }
    }

    private void ResetIdStack()
    {
        _idStack.Clear();
        _idStack.Push(WidgetID.Root);
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
            bool activateFocusedWidget = ShouldActivateFocusedWidget(ThisFrameInput);
            FocusState = CreateFocusState(id, activateFocusedWidget);
        }
    }

    private WidgetID? GetFocusedWidgetIdForFrame(ImtuiInput input)
    {
        int focusOffset = GetFocusOffset(input);

        if (focusOffset == 0)
            return _focusedWidgetId;

        WidgetID? focusedWidgetId = GetOffsetWidgetId(_focusOrder, _focusedWidgetId, focusOffset);
        return focusedWidgetId;
    }

    private static int GetFocusOffset(ImtuiInput input)
    {
        if (input.HasKey(ImtuiKey.ShiftTab))
            return -1;

        if (input.HasKey(ImtuiKey.Tab))
            return 1;

        return 0;
    }

    private static WidgetID? GetOffsetWidgetId(
        IReadOnlyList<WidgetID> order,
        WidgetID? current,
        int offset
    )
    {
        if (order.Count == 0)
            return current;

        int currentIndex = current is { } currentId ? IndexOf(order, currentId) : -1;
        int nextIndex = currentIndex < 0 ? 0 : (currentIndex + offset + order.Count) % order.Count;
        WidgetID offsetWidgetId = order[nextIndex];
        return offsetWidgetId;
    }

    private static int IndexOf(IReadOnlyList<WidgetID> order, WidgetID id)
    {
        for (int index = 0; index < order.Count; index++)
        {
            if (order[index] == id)
                return index;
        }

        return -1;
    }

    private static FocusState CreateFocusState(
        WidgetID? focusedWidgetId,
        bool activateFocusedWidget
    )
    {
        if (focusedWidgetId is not { } focused)
            return default;

        WidgetID active = activateFocusedWidget ? focused : default;
        FocusState focusState = new(focused, active);
        return focusState;
    }

    private static bool ShouldActivateFocusedWidget(ImtuiInput input) =>
        input.HasKey(ImtuiKey.Enter) || input.HasKey(ImtuiKey.Space);

    private static Size CurrentTerminalSize => new(Console.WindowWidth, Console.WindowHeight);
}
