// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using CsCheck;
using Imtui.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Imtui.Rendering.Renderer;
using static Imtui.Tests.Generators;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class RendererTests
{
    [TestMethod]
    public void Property_Diff_Correctness()
    {
        GenTwoScreensSameSize.Sample((prev, next) => Apply(prev, Diff(prev, next)) == next);
    }

    [TestMethod]
    public void Property_Diff_Determinism()
    {
        GenTwoScreensSameSize.Sample(
            (prev, next) => Diff(prev, next).SequenceEqual(Diff(prev, next))
        );
    }

    [TestMethod]
    public void Property_Diff_Composition()
    {
        GenThreeScreensSameSize.Sample((a, b, c) => Apply(a, [.. Diff(a, b), .. Diff(b, c)]) == c);
    }

    [TestMethod]
    public void Format_MoveCursor_UsesOneBasedAnsiCoordinates()
    {
        string formatted = Format([TermOp.MoveCursor(new CellPosition(1, 2))]);

        Assert.AreEqual("\x1b[3;2H", formatted);
    }

    [TestMethod]
    public void Format_WriteWithDefaultColors_EmitsExplicitDefaultsAndReset()
    {
        string formatted = Format([TermOp.Write(new Cell(new Rune('A')))]);

        Assert.AreEqual("\x1b[39;49mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithAnsiColors_EmitsAnsiForegroundAndBackground()
    {
        Cell cell = new(
            new Rune('A'),
            new CellStyle(Color.Ansi(AnsiColor.BrightCyan), Color.Ansi(AnsiColor.Red))
        );

        string formatted = Format([TermOp.Write(cell)]);

        Assert.AreEqual("\x1b[96;41mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithPaletteColors_EmitsPaletteForegroundAndBackground()
    {
        Cell cell = new(new Rune('A'), new CellStyle(Color.Palette256(123), Color.Palette256(45)));

        string formatted = Format([TermOp.Write(cell)]);

        Assert.AreEqual("\x1b[38;5;123;48;5;45mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithRgbColors_EmitsRgbForegroundAndBackground()
    {
        Cell cell = new(new Rune('A'), new CellStyle(Color.Rgb(1, 2, 3), Color.Rgb(4, 5, 6)));

        string formatted = Format([TermOp.Write(cell)]);

        Assert.AreEqual("\x1b[38;2;1;2;3;48;2;4;5;6mA\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithDefaultGlyph_EmitsVisibleDebugMarker()
    {
        string formatted = Format([TermOp.Write(default)]);

        Assert.AreEqual("\x1b[37;41mX\x1b[0m", formatted);
    }

    [TestMethod]
    public void Format_WriteWithNonNullControlGlyph_Throws()
    {
        TermOp[] operations = [TermOp.Write(new Cell(new Rune('\n')))];

        Assert.ThrowsExactly<ArgumentException>(() => Format(operations));
    }

    [TestMethod]
    public void Format_MoveCursorWithNegativePosition_Throws()
    {
        TermOp[] operations = [TermOp.MoveCursor(new CellPosition(-1, 0))];

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Format(operations));
    }

    [TestMethod]
    public void Render_ProducesCorrectAnsiOutput()
    {
        Screen prev = new(new Size(2, 1));
        Screen next = new(
            new Size(2, 1),
            [
                new Cell(new Rune('A'), new CellStyle(Color.Default, Color.Default)),
                new Cell(new Rune('B'), new CellStyle(Color.Default, Color.Default)),
            ]
        );

        string result = Render(prev, next);

        Assert.AreEqual("\x1b[1;1H\x1b[39;49mA\x1b[1;2H\x1b[39;49mB\x1b[0m", result);
    }
}
