// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text;
using Imtui.Rendering;
using Imtui.Widgets;

namespace Imtui;

/// <summary>
/// The central context for an immediate-mode TUI application. Owns the screen
/// state, widget ID stack, per-widget state storage, and produces ANSI output
/// each frame.
/// </summary>
public class ImtuiContext
{
    private Screen _previous;
    private Screen _current;
    private readonly Stack<WidgetID> _idStack = new();
    private readonly WidgetStateStorage _stateStorage = new();
    private readonly FocusState _focusState = new();
    private ImtuiInput _input;
    private int _widgetCursorY;

    /// <summary>
    /// Creates a new Imtui context.
    /// </summary>
    public ImtuiContext()
    {
        Size size = CurrentTerminalSize;
        _previous = new Screen(size);
        _current = new Screen(size);
        _idStack.Push(WidgetID.Root);
    }

    internal Screen CurrentScreen => _current;

    internal ImtuiInput CurrentInput => _input;

    internal WidgetID? FocusedWidgetId => _focusState.FocusedId;

    /// <summary>
    /// Begins a new frame. The previous frame becomes the baseline for
    /// diffing, and the accessed-ID set is cleared for the new frame.
    /// </summary>
    public void NewFrame(Size? size = null, ImtuiInput input = default)
    {
        Debug.Assert(
            _idStack.Count == 1,
            $"Unbalanced PushId/PopId: expected stack depth 1, got {_idStack.Count}"
        );

        Size nextFrameSize = size.GetValueOrDefault(CurrentTerminalSize);
        _previous = _current;
        _current = new Screen(nextFrameSize);

        ResetIdStack();
        _input = input;
        _widgetCursorY = 0;
        _focusState.BeginFrame(input);
    }

    /// <summary>
    /// Renders the current frame by diffing against the previous frame and
    /// returning the ANSI escape sequence output.
    /// </summary>
    public string RenderFrame()
    {
        return Renderer.Render(_previous, _current);
    }

    /// <summary>
    /// Generates a widget ID by hashing the label against the current ID
    /// stack seed.
    /// </summary>
    public WidgetID GetId(string label) => WidgetID.Hash(label.AsSpan(), _idStack.Peek());

    /// <summary>
    /// Generates a widget ID by hashing an integer against the current ID
    /// stack seed.
    /// </summary>
    public WidgetID GetId(int intId) => WidgetID.Hash(intId, _idStack.Peek());

    /// <summary>
    /// Pushes a string-based scope onto the ID stack so that widgets with
    /// the same label in different scopes produce different IDs.
    /// </summary>
    public void PushId(string label) => _idStack.Push(GetId(label));

    /// <summary>
    /// Pushes an integer-based scope onto the ID stack.
    /// </summary>
    public void PushId(int intId) => _idStack.Push(GetId(intId));

    /// <summary>
    /// Pops the top scope from the ID stack.
    /// </summary>
    public void PopId()
    {
        if (_idStack.Count <= 1)
        {
            throw new InvalidOperationException("Cannot pop the root ID from the stack.");
        }

        _idStack.Pop();
    }

    /// <summary>
    /// Gets or creates per-widget state of type <typeparamref name="T"/> for
    /// the given widget ID. The state persists across frames as long as the
    /// context is alive.
    /// </summary>
    public T GetWidgetState<T>(WidgetID id)
        where T : class, new() => _stateStorage.GetOrCreate<T>(id);

    internal bool RegisterFocusable(WidgetID id) => _focusState.Register(id);

    internal bool IsActivated(WidgetID id) =>
        FocusedWidgetId == id && (_input.HasKey(ImtuiKey.Enter) || _input.HasKey(ImtuiKey.Space));

    internal void Submit(IWidget widget) => widget.Execute(this);

    internal TResult Submit<TResult>(IWidget<TResult> widget) => widget.Execute(this);

    internal CellPosition AllocateWidgetPosition() => new(0, _widgetCursorY++);

    /// <summary>
    /// Writes a single cell to the current frame. Out-of-bounds writes are
    /// ignored.
    /// </summary>
    public void WriteCell(CellPosition position, Cell cell)
    {
        if (
            position.X >= 0
            && position.X < _current.Size.Width
            && position.Y >= 0
            && position.Y < _current.Size.Height
        )
        {
            _current[position] = cell;
        }
    }

    /// <summary>
    /// Writes text to the current frame. Out-of-bounds glyphs are ignored.
    /// </summary>
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

    private static Size CurrentTerminalSize => new(Console.WindowWidth, Console.WindowHeight);
}
