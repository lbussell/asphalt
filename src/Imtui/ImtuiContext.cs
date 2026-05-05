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

        // Reset ID stack
        _idStack.Clear();
        _idStack.Push(WidgetID.Root);

        // Reset widget cursor
        _widgetCursorY = 0;

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

    private enum FocusAction
    {
        None,
        FocusNext,
        FocusPrevious,
    }
}
