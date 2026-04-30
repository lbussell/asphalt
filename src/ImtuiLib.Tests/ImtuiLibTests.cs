// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImtuiLib.Tests;

[TestClass]
public class ImtuiLibTests
{
    [TestMethod]
    public void ImtuiLib_CanBeInstantiated()
    {
        Assert.IsNotNull(typeof(Imtui));
    }

    [TestMethod]
    public void Render_FirstFrameReportsEveryCellAsChanged()
    {
        ImtuiContext context = new();

        context.NewFrame(new FrameRequest(new ViewportSize(3, 2)));
        RenderedFrame frame = context.Render();

        Assert.AreEqual(3, frame.Width);
        Assert.AreEqual(2, frame.Height);
        Assert.AreEqual(6, frame.Changes.Count);
        Assert.IsTrue(frame.Changes.All(change => change.Cell == Cell.Empty));
    }

    [TestMethod]
    public void Render_UnchangedFrameReportsNoChanges()
    {
        ImtuiContext context = new();
        FrameRequest request = new(new ViewportSize(5, 1));

        context.NewFrame(request);
        context.Text("Hello");
        _ = context.Render();

        context.NewFrame(request);
        context.Text("Hello");
        RenderedFrame frame = context.Render();

        Assert.AreEqual(0, frame.Changes.Count);
    }

    [TestMethod]
    public void Render_ChangedFrameReportsOnlyChangedCells()
    {
        ImtuiContext context = new();
        FrameRequest request = new(new ViewportSize(5, 1));

        context.NewFrame(request);
        context.Text("Hello");
        _ = context.Render();

        context.NewFrame(request);
        context.Text("Hxllo");
        RenderedFrame frame = context.Render();

        Assert.AreEqual(1, frame.Changes.Count);
        Assert.AreEqual(new CellPosition(1, 0), frame.Changes[0].Position);
        Assert.AreEqual(new Rune('x'), frame.Changes[0].Cell.Glyph);
    }

    [TestMethod]
    public void TextAt_ClipsTextToViewport()
    {
        ImtuiContext context = new();

        context.NewFrame(new FrameRequest(new ViewportSize(3, 1)));
        context.TextAt(new CellPosition(1, 0), "Hello");
        RenderedFrame frame = context.Render();

        Assert.AreEqual(Cell.Empty, frame[0, 0]);
        Assert.AreEqual(new Rune('H'), frame[1, 0].Glyph);
        Assert.AreEqual(new Rune('e'), frame[2, 0].Glyph);
    }

    [TestMethod]
    public void Render_StyleChangesAreCellChanges()
    {
        ImtuiContext context = new();
        FrameRequest request = new(new ViewportSize(1, 1));

        context.NewFrame(request);
        context.Text("A");
        _ = context.Render();

        CellStyle style = new(
            TerminalColor.FromAnsi(AnsiColor.BrightGreen),
            TerminalColor.Default,
            TextAttributes.Bold | TextAttributes.Reverse
        );

        context.NewFrame(request);
        context.Text("A", style);
        RenderedFrame frame = context.Render();

        Assert.AreEqual(1, frame.Changes.Count);
        Assert.AreEqual(style, frame[0, 0].Style);
    }

    [TestMethod]
    public void Render_SizeChangeWithSameCellCountReportsEveryCellAsChanged()
    {
        ImtuiContext context = new();

        context.NewFrame(new FrameRequest(new ViewportSize(2, 3)));
        _ = context.Render();

        context.NewFrame(new FrameRequest(new ViewportSize(3, 2)));
        RenderedFrame frame = context.Render();

        Assert.AreEqual(6, frame.Changes.Count);
    }
}
