// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using Imtui.Rendering;

namespace Imtui;

public static class PrimitiveDrawingExtensions
{
    extension(ImtuiContext context)
    {
        public void WriteText(CellPosition position, string text, CellStyle style = default)
        {
            int x = position.X;
            foreach (Rune glyph in text.EnumerateRunes())
            {
                context.WriteCell(new CellPosition(x, position.Y), new Cell(glyph, style));
                x++;
            }
        }

        public void FillRect(Rect rect, CellStyle style = default)
        {
            Cell cell = new(new Rune(' '), style);
            for (int y = rect.Y; y < rect.Bottom; y++)
            {
                for (int x = rect.X; x < rect.Right; x++)
                    context.WriteCell(new CellPosition(x, y), cell);
            }
        }

        public void DrawBox(Rect rect, BorderStyle borderStyle, CellStyle style = default)
        {
            if (rect.Width < 2 || rect.Height < 2)
                return;

            BorderStyle box = borderStyle;
            int right = rect.Right - 1;
            int bottom = rect.Bottom - 1;

            // Corners
            context.WriteCell(new CellPosition(rect.X, rect.Y), new Cell(box.TopLeft, style));
            context.WriteCell(new CellPosition(right, rect.Y), new Cell(box.TopRight, style));
            context.WriteCell(new CellPosition(rect.X, bottom), new Cell(box.BottomLeft, style));
            context.WriteCell(new CellPosition(right, bottom), new Cell(box.BottomRight, style));

            // Top and bottom edges
            Cell horizontalCell = new(box.Horizontal, style);
            for (int x = rect.X + 1; x < right; x++)
            {
                context.WriteCell(new CellPosition(x, rect.Y), horizontalCell);
                context.WriteCell(new CellPosition(x, bottom), horizontalCell);
            }

            // Left and right edges
            Cell verticalCell = new(box.Vertical, style);
            for (int y = rect.Y + 1; y < bottom; y++)
            {
                context.WriteCell(new CellPosition(rect.X, y), verticalCell);
                context.WriteCell(new CellPosition(right, y), verticalCell);
            }
        }

        public void DrawHorizontalLine(CellPosition start, int length, CellStyle style = default)
        {
            Cell cell = new(BorderStyle.Square.Horizontal, style);
            for (int x = start.X; x < start.X + length; x++)
                context.WriteCell(new CellPosition(x, start.Y), cell);
        }

        public void DrawVerticalLine(CellPosition start, int length, CellStyle style = default)
        {
            Cell cell = new(BorderStyle.Square.Vertical, style);
            for (int y = start.Y; y < start.Y + length; y++)
                context.WriteCell(new CellPosition(start.X, y), cell);
        }
    }
}
