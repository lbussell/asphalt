// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

public sealed class TerminalCanvas(Dimensions dimensions) : ICanvas
{
    private readonly TerminalCell[,] _cells = new TerminalCell[dimensions.Height, dimensions.Width];
    public Dimensions Dimensions { get; } = dimensions;
    public int Width => Dimensions.Width;
    public int Height => Dimensions.Height;

    // Resets every cell to its default state. Called by the run loop at the
    // start of each frame so that widgets which shrink between frames don't
    // leave stale characters from the previous frame visible. Without this,
    // a Text that drew "loading..." in one frame would leave "ing..." behind
    // when a single-glyph Spinner replaces it the next frame.
    public void Clear() => Array.Clear(_cells);

    public void Draw(
        Position position,
        char character,
        TerminalColor foregroundColor = default,
        TerminalColor backgroundColor = default
    )
    {
        if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
            return;

        TerminalCell cell = _cells[position.Y, position.X];

        if (IsDefault(backgroundColor))
            backgroundColor = cell.BackgroundColor;

        _cells[position.Y, position.X] = new TerminalCell(
            character,
            foregroundColor,
            backgroundColor
        );
    }

    internal TerminalCell GetCell(int x, int y) => _cells[y, x];

    private static bool IsDefault(TerminalColor color) => color.Kind == TerminalColorKind.Default;
}
