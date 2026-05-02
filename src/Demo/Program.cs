// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Imtui;

Screen screen = new(new Size(32, 5));

WriteText(
    screen,
    new CellPosition(2, 1),
    "dotnet-imtui",
    new CellStyle(Color.Ansi(AnsiColor.BrightCyan), Color.Default)
);
WriteText(
    screen,
    new CellPosition(2, 2),
    "minimal terminal output",
    new CellStyle(Color.Ansi(AnsiColor.White), Color.Ansi(AnsiColor.Blue))
);

Console.Write("\x1b[2J\x1b[H");
Console.Write(AnsiFormatter.Format(DifferentialRendering.Render(new Screen(screen.Size), screen)));
Console.Out.Flush();

static void WriteText(Screen screen, CellPosition position, string text, CellStyle style)
{
    int x = position.X;

    foreach (Rune glyph in text.EnumerateRunes())
    {
        screen.Cells[position.Y * screen.Size.Width + x] = new Cell(glyph, style);
        x++;
    }
}
