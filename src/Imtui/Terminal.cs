// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace Imtui;

public readonly record struct Cell(Rune Glyph, CellStyle Style = default);

public readonly record struct CellPosition(int X, int Y);

public readonly record struct CellStyle(Color Foreground, Color Background);

public readonly record struct Screen(Size Size, Cell[] Cells)
{
    public Screen(Size size)
        : this(size, CreateCells(size)) { }

    public Cell this[CellPosition position]
    {
        get => Cells[position.Y * Size.Width + position.X];
        set => Cells[position.Y * Size.Width + position.X] = value;
    }

    public void WriteText(CellPosition position, string text, CellStyle style)
    {
        int x = position.X;

        foreach (Rune glyph in text.EnumerateRunes())
        {
            this[new CellPosition(x, position.Y)] = new Cell(glyph, style);
            x++;
        }
    }

    public bool Equals(Screen other) =>
        Size == other.Size && Cells.AsSpan().SequenceEqual(other.Cells);

    public override int GetHashCode()
    {
        HashCode hashCode = new HashCode();
        hashCode.Add(Size);

        foreach (Cell cell in Cells.AsSpan())
        {
            hashCode.Add(cell);
        }

        return hashCode.ToHashCode();
    }

    private static Cell[] CreateCells(Size size)
    {
        Cell[] cells = new Cell[size.Width * size.Height];
        Array.Fill(cells, Cell.Empty);
        return cells;
    }
}

public readonly record struct Size(int Width, int Height);
