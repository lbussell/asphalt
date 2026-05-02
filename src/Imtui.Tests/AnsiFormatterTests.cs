// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class AnsiFormatterTests
{
    [TestMethod]
    public void Format_MoveCursor_UsesOneBasedAnsiCoordinates()
    {
        string formatted = AnsiFormatter.Format([TermOp.MoveCursor(new CellPosition(1, 2))]);

        Assert.AreEqual("\x1b[3;2H", formatted);
    }

    [TestMethod]
    public void Format_WriteWithDefaultColors_EmitsExplicitDefaultsAndReset()
    {
        string formatted = AnsiFormatter.Format([TermOp.Write(new Cell(new Rune('A')))]);

        Assert.AreEqual("\x1b[39;49mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithAnsiColors_EmitsAnsiForegroundAndBackground()
    {
        Cell cell = new(
            new Rune('A'),
            new CellStyle(Color.Ansi(AnsiColor.BrightCyan), Color.Ansi(AnsiColor.Red))
        );

        string formatted = AnsiFormatter.Format([TermOp.Write(cell)]);

        Assert.AreEqual("\x1b[96;41mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithPaletteColors_EmitsPaletteForegroundAndBackground()
    {
        Cell cell = new(new Rune('A'), new CellStyle(Color.Palette256(123), Color.Palette256(45)));

        string formatted = AnsiFormatter.Format([TermOp.Write(cell)]);

        Assert.AreEqual("\x1b[38;5;123;48;5;45mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithRgbColors_EmitsRgbForegroundAndBackground()
    {
        Cell cell = new(new Rune('A'), new CellStyle(Color.Rgb(1, 2, 3), Color.Rgb(4, 5, 6)));

        string formatted = AnsiFormatter.Format([TermOp.Write(cell)]);

        Assert.AreEqual("\x1b[38;2;1;2;3;48;2;4;5;6mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithDefaultGlyph_EmitsVisibleDebugMarker()
    {
        string formatted = AnsiFormatter.Format([TermOp.Write(default)]);

        Assert.AreEqual("\x1b[37;41mX\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithNonNullControlGlyph_Throws()
    {
        TermOp[] operations = [TermOp.Write(new Cell(new Rune('\n')))];

        Assert.ThrowsExactly<ArgumentException>(() => AnsiFormatter.Format(operations));
    }

    [TestMethod]
    public void Format_MoveCursorWithNegativePosition_Throws()
    {
        TermOp[] operations = [TermOp.MoveCursor(new CellPosition(-1, 0))];

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => AnsiFormatter.Format(operations));
    }
}
