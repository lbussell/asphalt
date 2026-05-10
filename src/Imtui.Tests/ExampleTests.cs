// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class ExampleTests
{
    [TestMethod]
    public void ExampleTest()
    {
        Assert.IsNotNull("Hello world");
    }

    [TestMethod]
    public void TerminalCanvasDrawCanUseDefaultColors()
    {
        TerminalCanvas canvas = new(new Dimensions(1, 1));

        canvas.Draw(new Position(0, 0), 'X');
        string rendered = Render(canvas);

        StringAssert.Contains(rendered, "X");
        Assert.IsFalse(rendered.Contains("\x1b[3"));
        Assert.IsFalse(rendered.Contains("\x1b[4"));
    }

    [TestMethod]
    public void TerminalColorUsesFourBytes()
    {
        Assert.AreEqual(4, Marshal.SizeOf<TerminalColor>());
    }

    [TestMethod]
    public void TerminalCanvasDrawCanOverrideForegroundOnly()
    {
        TerminalCanvas canvas = new(new Dimensions(1, 1));

        canvas.Draw(new Position(0, 0), 'X', foregroundColor: TerminalColor.Red);
        string rendered = Render(canvas);

        StringAssert.Contains(rendered, "\x1b[31m");
        Assert.IsFalse(rendered.Contains("\x1b[41m"));
        Assert.IsFalse(rendered.Contains("\x1b[48;"));
    }

    [TestMethod]
    public void TerminalCanvasDrawCanOverrideBackgroundOnly()
    {
        TerminalCanvas canvas = new(new Dimensions(1, 1));

        canvas.Draw(new Position(0, 0), 'X', backgroundColor: TerminalColor.Palette(123));
        string rendered = Render(canvas);

        StringAssert.Contains(rendered, "\x1b[48;5;123m");
        Assert.IsFalse(rendered.Contains("\x1b[38;"));
        Assert.IsFalse(rendered.Contains("\x1b[31m"));
    }

    [TestMethod]
    public void TerminalCanvasDrawResetsDefaultColorsAfterColoredCells()
    {
        TerminalCanvas canvas = new(new Dimensions(2, 1));

        canvas.Draw(new Position(0, 0), 'A', backgroundColor: TerminalColor.Red);
        canvas.Draw(new Position(1, 0), 'B');
        string rendered = Render(canvas);

        StringAssert.Contains(rendered, "\x1b[41mA\x1b[0mB");
    }

    [TestMethod]
    public void TerminalCanvasDrawRendersRgbColors()
    {
        TerminalCanvas canvas = new(new Dimensions(1, 1));

        canvas.Draw(
            new Position(0, 0),
            'X',
            TerminalColor.Rgb(1, 2, 3),
            TerminalColor.Rgb(4, 5, 6)
        );
        string rendered = Render(canvas);

        StringAssert.Contains(rendered, "\x1b[38;2;1;2;3m");
        StringAssert.Contains(rendered, "\x1b[48;2;4;5;6m");
        StringAssert.Contains(rendered, "X");
    }

    [TestMethod]
    public void TerminalCanvasDrawRendersPaletteColors()
    {
        TerminalCanvas canvas = new(new Dimensions(1, 1));

        canvas.Draw(new Position(0, 0), 'X', TerminalColor.Palette(123), TerminalColor.Palette(45));
        string rendered = Render(canvas);

        StringAssert.Contains(rendered, "\x1b[38;5;123m");
        StringAssert.Contains(rendered, "\x1b[48;5;45m");
    }

    [TestMethod]
    public void TerminalCanvasDrawRendersAnsi16Colors()
    {
        (TerminalColor Color, int ForegroundCode, int BackgroundCode)[] colors =
        [
            (TerminalColor.Black, 30, 40),
            (TerminalColor.Red, 31, 41),
            (TerminalColor.Green, 32, 42),
            (TerminalColor.Yellow, 33, 43),
            (TerminalColor.Blue, 34, 44),
            (TerminalColor.Magenta, 35, 45),
            (TerminalColor.Cyan, 36, 46),
            (TerminalColor.White, 37, 47),
            (TerminalColor.BrightBlack, 90, 100),
            (TerminalColor.BrightRed, 91, 101),
            (TerminalColor.BrightGreen, 92, 102),
            (TerminalColor.BrightYellow, 93, 103),
            (TerminalColor.BrightBlue, 94, 104),
            (TerminalColor.BrightMagenta, 95, 105),
            (TerminalColor.BrightCyan, 96, 106),
            (TerminalColor.BrightWhite, 97, 107),
        ];

        foreach ((TerminalColor color, int foregroundCode, int backgroundCode) in colors)
        {
            TerminalCanvas canvas = new(new Dimensions(1, 1));

            canvas.Draw(new Position(0, 0), 'X', color, color);
            string rendered = Render(canvas);

            StringAssert.Contains(rendered, $"\x1b[{foregroundCode}m");
            StringAssert.Contains(rendered, $"\x1b[{backgroundCode}m");
        }
    }

    [TestMethod]
    public void BorderPanelRendersBoxDrawingCharacters()
    {
        TerminalCanvas canvas = new(new Dimensions(4, 3));
        BorderPanel borderPanel = new(BorderStyle.Square);

        borderPanel.Render(new Rect(0, 0, 4, 3), canvas);
        string rendered = StripAnsi(Render(canvas));

        StringAssert.Contains(rendered, "┌──┐\n│  │\n└──┘");
    }

    [TestMethod]
    public void BorderPanelRendersCustomBorderStyle()
    {
        TerminalCanvas canvas = new(new Dimensions(4, 3));
        BorderPanel borderPanel = new(BorderStyle.Ascii);

        borderPanel.Render(new Rect(0, 0, 4, 3), canvas);
        string rendered = StripAnsi(Render(canvas));

        StringAssert.Contains(rendered, "+--+\n|  |\n+--+");
    }

    [TestMethod]
    public void BorderPanelRendersBestEffortForSmallBounds()
    {
        TerminalCanvas canvas = new(new Dimensions(1, 3));
        BorderPanel borderPanel = new();

        borderPanel.Render(new Rect(0, 0, 1, 3), canvas);
        string rendered = StripAnsi(Render(canvas));

        StringAssert.Contains(rendered, "╭\n│\n╰");
    }

    [TestMethod]
    public void TextRendersInsideBounds()
    {
        TerminalCanvas canvas = new(new Dimensions(5, 2));
        Text text = new("Hello\nWorld");

        text.Render(new Rect(0, 0, 3, 2), canvas);
        string rendered = StripAnsi(Render(canvas));

        StringAssert.Contains(rendered, "Hel  \nWor");
    }

    [TestMethod]
    public void TextWrapsAtSpacesByDefault()
    {
        TerminalCanvas canvas = new(new Dimensions(5, 2));
        Text text = new("hello world");

        text.Render(new Rect(0, 0, 5, 2), canvas);
        string rendered = StripAnsi(Render(canvas));

        StringAssert.Contains(rendered, "hello\nworld");
    }

    [TestMethod]
    public void TextCanForceWrapInsideWords()
    {
        TerminalCanvas canvas = new(new Dimensions(3, 2));
        Text text = new("abcdef", TextWrapMode.Force);

        text.Render(new Rect(0, 0, 3, 2), canvas);
        string rendered = StripAnsi(Render(canvas));

        StringAssert.Contains(rendered, "abc\ndef");
    }

    [TestMethod]
    public void TextCanClipWithoutWrapping()
    {
        TerminalCanvas canvas = new(new Dimensions(3, 2));
        Text text = new("abcdef", TextWrapMode.Clip);

        text.Render(new Rect(0, 0, 3, 2), canvas);
        string rendered = StripAnsi(Render(canvas));

        StringAssert.Contains(rendered, "abc\n   ");
        Assert.IsFalse(rendered.Contains("def"));
    }

    [TestMethod]
    public void TextMeasuresSpaceWrapMinimumWidthFromLongestWord()
    {
        Text text = new("aa bbbb cc");

        Dimensions measured = text.Measure();

        Assert.AreEqual(4, measured.Width);
    }

    private static string Render(TerminalCanvas canvas)
    {
        StringWriter output = new();
        canvas.Present(output);
        return output.ToString();
    }

    private static string StripAnsi(string text) =>
        Regex.Replace(text, @"\x1B\[[0-9;]*[A-Za-z]", "");
}
