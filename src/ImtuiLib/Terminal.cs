// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace ImtuiLib;

public readonly record struct Cell(Rune Glyph, CellStyle Style = default);

public readonly record struct CellPosition(int X, int Y);

public readonly record struct CellStyle(AnsiColor Foreground, AnsiColor Background);

public readonly record struct Screen(int Width, int Height, Cell[] Cells)
{
    public Screen(int width, int height)
        : this(width, height, new Cell[width * height]) { }
}

public readonly record struct Size(int Width, int Height);
