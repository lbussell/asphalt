// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using Imtui.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class DrawingPrimitiveTests
{
    private static readonly CellStyle TestStyle = new(
        Color.Ansi(AnsiColor.White),
        Color.Ansi(AnsiColor.Blue)
    );

    // --- FillRect tests ---

    [TestMethod]
    public void FillRect_FillsEntireArea()
    {
        ImtuiContext context = CreateContext(4, 3);

        context.FillRect(new Rect(1, 0, 2, 2), TestStyle);

        Cell expected = new(new Rune(' '), TestStyle);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(1, 0)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(2, 0)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(1, 1)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(2, 1)]);

        // Outside the rect should be untouched
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(0, 0)]);
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(3, 0)]);
    }

    [TestMethod]
    public void FillRect_ClipsToScreenBounds()
    {
        ImtuiContext context = CreateContext(4, 3);

        // Rect extends beyond right and bottom edges
        context.FillRect(new Rect(3, 2, 5, 5), TestStyle);

        Cell expected = new(new Rune(' '), TestStyle);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(3, 2)]);
        // No crash — out-of-bounds cells are silently ignored
    }

    [TestMethod]
    public void FillRect_ZeroDimensions_IsNoOp()
    {
        ImtuiContext context = CreateContext(4, 3);

        context.FillRect(new Rect(0, 0, 0, 0), TestStyle);
        context.FillRect(new Rect(0, 0, -1, -1), TestStyle);

        // All cells unchanged
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(0, 0)]);
    }

    // --- DrawBox tests ---

    [TestMethod]
    public void DrawBox_DrawsBorderCharacters()
    {
        ImtuiContext context = CreateContext(5, 4);

        context.DrawBox(new Rect(0, 0, 5, 4), TestStyle);

        BoxChars box = BoxChars.Light;
        Assert.AreEqual(
            new Cell(box.TopLeft, TestStyle),
            context.CurrentScreen[new CellPosition(0, 0)]
        );
        Assert.AreEqual(
            new Cell(box.TopRight, TestStyle),
            context.CurrentScreen[new CellPosition(4, 0)]
        );
        Assert.AreEqual(
            new Cell(box.BottomLeft, TestStyle),
            context.CurrentScreen[new CellPosition(0, 3)]
        );
        Assert.AreEqual(
            new Cell(box.BottomRight, TestStyle),
            context.CurrentScreen[new CellPosition(4, 3)]
        );

        // Horizontal edges
        Assert.AreEqual(
            new Cell(box.Horizontal, TestStyle),
            context.CurrentScreen[new CellPosition(1, 0)]
        );
        Assert.AreEqual(
            new Cell(box.Horizontal, TestStyle),
            context.CurrentScreen[new CellPosition(2, 3)]
        );

        // Vertical edges
        Assert.AreEqual(
            new Cell(box.Vertical, TestStyle),
            context.CurrentScreen[new CellPosition(0, 1)]
        );
        Assert.AreEqual(
            new Cell(box.Vertical, TestStyle),
            context.CurrentScreen[new CellPosition(4, 2)]
        );

        // Interior is not filled
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(2, 1)]);
    }

    [TestMethod]
    public void DrawBox_TooSmall_IsNoOp()
    {
        ImtuiContext context = CreateContext(4, 3);

        // Width < 2 or Height < 2 should be a no-op
        context.DrawBox(new Rect(0, 0, 1, 3), TestStyle);
        context.DrawBox(new Rect(0, 0, 3, 1), TestStyle);

        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(0, 0)]);
    }

    [TestMethod]
    public void DrawBox_ClipsToScreenBounds()
    {
        ImtuiContext context = CreateContext(4, 3);

        // Box extends beyond screen — should not crash, clips gracefully
        context.DrawBox(new Rect(2, 1, 10, 10), TestStyle);

        BoxChars box = BoxChars.Light;
        Assert.AreEqual(
            new Cell(box.TopLeft, TestStyle),
            context.CurrentScreen[new CellPosition(2, 1)]
        );
        Assert.AreEqual(
            new Cell(box.Horizontal, TestStyle),
            context.CurrentScreen[new CellPosition(3, 1)]
        );
        Assert.AreEqual(
            new Cell(box.Vertical, TestStyle),
            context.CurrentScreen[new CellPosition(2, 2)]
        );
    }

    // --- DrawHorizontalLine tests ---

    [TestMethod]
    public void DrawHorizontalLine_DrawsLineCharacters()
    {
        ImtuiContext context = CreateContext(5, 2);

        context.DrawHorizontalLine(new CellPosition(1, 0), 3, TestStyle);

        Cell expected = new(BoxChars.Light.Horizontal, TestStyle);
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(0, 0)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(1, 0)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(2, 0)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(3, 0)]);
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(4, 0)]);
    }

    [TestMethod]
    public void DrawHorizontalLine_ClipsToScreenBounds()
    {
        ImtuiContext context = CreateContext(4, 2);

        // Starts at x=2, length=10 — should clip at screen edge
        context.DrawHorizontalLine(new CellPosition(2, 0), 10, TestStyle);

        Cell expected = new(BoxChars.Light.Horizontal, TestStyle);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(2, 0)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(3, 0)]);
        // No crash
    }

    [TestMethod]
    public void DrawHorizontalLine_ZeroLength_IsNoOp()
    {
        ImtuiContext context = CreateContext(4, 2);

        context.DrawHorizontalLine(new CellPosition(0, 0), 0, TestStyle);

        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(0, 0)]);
    }

    // --- DrawVerticalLine tests ---

    [TestMethod]
    public void DrawVerticalLine_DrawsLineCharacters()
    {
        ImtuiContext context = CreateContext(3, 5);

        context.DrawVerticalLine(new CellPosition(1, 1), 3, TestStyle);

        Cell expected = new(BoxChars.Light.Vertical, TestStyle);
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(1, 0)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(1, 1)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(1, 2)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(1, 3)]);
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(1, 4)]);
    }

    [TestMethod]
    public void DrawVerticalLine_ClipsToScreenBounds()
    {
        ImtuiContext context = CreateContext(3, 4);

        context.DrawVerticalLine(new CellPosition(0, 2), 10, TestStyle);

        Cell expected = new(BoxChars.Light.Vertical, TestStyle);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(0, 2)]);
        Assert.AreEqual(expected, context.CurrentScreen[new CellPosition(0, 3)]);
        // No crash
    }

    [TestMethod]
    public void DrawVerticalLine_ZeroLength_IsNoOp()
    {
        ImtuiContext context = CreateContext(3, 4);

        context.DrawVerticalLine(new CellPosition(0, 0), 0, TestStyle);

        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(0, 0)]);
    }

    private static ImtuiContext CreateContext(int width, int height)
    {
        ImtuiContext context = new();
        context.NewFrame(new Size(width, height));
        return context;
    }
}
