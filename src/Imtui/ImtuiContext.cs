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
    private readonly FocusTree _focusState = new();

    private Screen _previousScreen;
    private Screen _currentScreen;
    private int _widgetCursorY;

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
    public WidgetID? FocusedWidgetId => _focusState.FocusedId;

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
        _focusState.BeginFrame(input);
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

    public bool IsFocused(WidgetID id) => FocusedWidgetId == id;

    public bool IsActivated(WidgetID id) =>
        IsFocused(id)
        && (ThisFrameInput.HasKey(ImtuiKey.Enter) || ThisFrameInput.HasKey(ImtuiKey.Space));

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
            _focusState.Register(widget.ID);
    }

    private static Size CurrentTerminalSize => new(Console.WindowWidth, Console.WindowHeight);
}
