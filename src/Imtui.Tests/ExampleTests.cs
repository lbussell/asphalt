// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

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
    public void TerminalCanvasDrawRendersCharacterWithColors()
    {
        TerminalCanvas canvas = new(new Dimensions(1, 1));
        StringWriter output = new();

        canvas.Draw(
            new Position(0, 0),
            'X',
            new TerminalColorRgb(1, 2, 3),
            new TerminalColorRgb(4, 5, 6)
        );
        canvas.Present(output);

        string rendered = output.ToString();
        StringAssert.Contains(rendered, "\x1b[38;2;1;2;3m");
        StringAssert.Contains(rendered, "\x1b[48;2;4;5;6m");
        StringAssert.Contains(rendered, "X");
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

    private static string Render(TerminalCanvas canvas)
    {
        StringWriter output = new();
        canvas.Present(output);
        return output.ToString();
    }

    private static string StripAnsi(string text) =>
        Regex.Replace(text, @"\x1B\[[0-9;]*[A-Za-z]", "");
}
