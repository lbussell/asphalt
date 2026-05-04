// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Imtui.Rendering;

namespace Imtui;

/// <summary>
/// The central context for an immediate-mode TUI application. Owns the screen
/// state and produces ANSI output each frame.
/// </summary>
public class ImtuiContext
{
    private Screen _previous;
    private Screen _current;

    /// <summary>
    /// Creates a new Imtui context.
    /// </summary>
    public ImtuiContext()
    {
        Size size = CurrentTerminalSize;
        _previous = new Screen(size);
        _current = new Screen(size);
    }

    /// <summary>
    /// Begins a new frame. The previous frame becomes the baseline for
    /// diffing.
    /// </summary>
    public void NewFrame(Size? size = null)
    {
        Size nextFrameSize = size.GetValueOrDefault(CurrentTerminalSize);
        _previous = _current;
        _current = new Screen(nextFrameSize);
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

    private static Size CurrentTerminalSize => new(Console.WindowWidth, Console.WindowHeight);
}
