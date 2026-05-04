// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Imtui.Rendering;

namespace Imtui;

/// <summary>
/// The central context for an immediate-mode TUI application.
/// Owns the screen state and produces ANSI output each frame.
/// </summary>
public class ImtuiContext
{
    private readonly Size _size;
    private Screen _previous;
    private Screen _current;

    /// <summary>
    /// Creates a new context with the given terminal dimensions.
    /// </summary>
    public ImtuiContext(int width, int height)
    {
        _size = new Size(width, height);
        _previous = new Screen(_size);
        _current = new Screen(_size);
    }

    /// <summary>
    /// Begins a new frame. The previous frame becomes the baseline for diffing.
    /// </summary>
    public void NewFrame()
    {
        _previous = _current;
        _current = new Screen(_size);
    }

    /// <summary>
    /// Renders the current frame by diffing against the previous frame
    /// and returning the ANSI escape sequence output.
    /// </summary>
    public string Render()
    {
        return Renderer.Render(_previous, _current);
    }

    /// <summary>
    /// Writes a single cell to the current frame. Out-of-bounds writes are ignored.
    /// </summary>
    public void WriteCell(CellPosition position, Cell cell)
    {
        if (
            position.X >= 0
            && position.X < _size.Width
            && position.Y >= 0
            && position.Y < _size.Height
        )
        {
            _current[position] = cell;
        }
    }
}
