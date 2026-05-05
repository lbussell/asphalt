// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using Imtui.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class ImtuiContextRenderingTests
{
    [TestMethod]
    public void WriteText_WritesGlyphsAtPosition()
    {
        ImtuiContext context = CreateContext();
        CellStyle style = new(Color.Ansi(AnsiColor.Yellow), Color.Default);

        context.WriteText(new CellPosition(1, 0), "Hi", style);

        Assert.AreEqual(
            new Cell(new Rune('H'), style),
            context.CurrentScreen[new CellPosition(1, 0)]
        );
        Assert.AreEqual(
            new Cell(new Rune('i'), style),
            context.CurrentScreen[new CellPosition(2, 0)]
        );
    }

    [TestMethod]
    public void WriteText_ClipsOutOfBoundsGlyphs()
    {
        ImtuiContext context = CreateContext();
        CellStyle style = new(Color.Default, Color.Ansi(AnsiColor.Blue));

        context.WriteText(new CellPosition(-1, 1), "ABC", style);
        context.WriteText(new CellPosition(3, 0), "XYZ", style);

        Assert.AreEqual(
            new Cell(new Rune('B'), style),
            context.CurrentScreen[new CellPosition(0, 1)]
        );
        Assert.AreEqual(
            new Cell(new Rune('C'), style),
            context.CurrentScreen[new CellPosition(1, 1)]
        );
        Assert.AreEqual(
            new Cell(new Rune('X'), style),
            context.CurrentScreen[new CellPosition(3, 0)]
        );
    }

    private static ImtuiContext CreateContext()
    {
        ImtuiContext context = new();
        context.NewFrame(new Size(4, 2));
        return context;
    }
}
