// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

using Asphalt.Rendering;
using Asphalt.Widgets;

[TestClass]
public class OverlayRenderingTests
{
    private static readonly Dimensions s_dimensions = new Dimensions(20, 5);

    [TestMethod]
    public void Overlay_RendersOnTopOfPrimaryTree()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_dimensions);

        context.BeginLayout(s_dimensions);
        // Primary: fill the screen with 'A's via a Text widget.
        context.Text(new string('A', s_dimensions.Width));
        // Overlay anchored top-left containing 'BB'. Should paint on top.
        using (context.Overlay(Anchor.Top | Anchor.Left))
        {
            context.Text("BB");
        }
        LayoutNode root = context.EndLayout();

        LayoutRenderer.Render(root, canvas);
        foreach (LayoutNode overlay in context.Overlays)
            LayoutRenderer.Render(overlay, canvas);

        Assert.AreEqual('B', canvas.GetCell(0, 0).CharacterOrSpace);
        Assert.AreEqual('B', canvas.GetCell(1, 0).CharacterOrSpace);
        // Beyond the overlay's width, the primary 'A's still show through.
        Assert.AreEqual('A', canvas.GetCell(2, 0).CharacterOrSpace);
    }

    [TestMethod]
    public void Overlay_AnchorBottomRight_PaintsInBottomRightCorner()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_dimensions);

        context.BeginLayout(s_dimensions);
        using (context.Overlay(Anchor.Bottom | Anchor.Right))
        {
            context.Text("XY");
        }
        LayoutNode root = context.EndLayout();

        LayoutRenderer.Render(root, canvas);
        foreach (LayoutNode overlay in context.Overlays)
            LayoutRenderer.Render(overlay, canvas);

        int lastRow = s_dimensions.Height - 1;
        int lastCol = s_dimensions.Width - 1;
        Assert.AreEqual('X', canvas.GetCell(lastCol - 1, lastRow).CharacterOrSpace);
        Assert.AreEqual('Y', canvas.GetCell(lastCol, lastRow).CharacterOrSpace);
    }

    [TestMethod]
    public void Overlay_LaterOverlayPaintsOverEarlierOverlay()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_dimensions);

        context.BeginLayout(s_dimensions);
        using (context.Overlay(Anchor.Top | Anchor.Left))
        {
            context.Text("AA");
        }
        // Same anchor: second overlay paints over the first.
        using (context.Overlay(Anchor.Top | Anchor.Left))
        {
            context.Text("BB");
        }
        LayoutNode root = context.EndLayout();

        LayoutRenderer.Render(root, canvas);
        foreach (LayoutNode overlay in context.Overlays)
            LayoutRenderer.Render(overlay, canvas);

        Assert.AreEqual('B', canvas.GetCell(0, 0).CharacterOrSpace);
        Assert.AreEqual('B', canvas.GetCell(1, 0).CharacterOrSpace);
    }
}
