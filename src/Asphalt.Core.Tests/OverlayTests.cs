// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class OverlayTests
{
    private static readonly Dimensions s_screen = new Dimensions(40, 20);

    // A minimal fixed-size widget for layout assertions. Use the same
    // dimensions for Minimum and Preferred so the overlay's fit-sizing is
    // deterministic.
    private sealed record SizedStub(Dimensions Size) : IWidget
    {
        public WidgetLayout Measure() => new WidgetLayout(Minimum: Size, Preferred: Size);

        public void Layout(Dimensions available) { }

        public void Render(Rect contentRect, Asphalt.Rendering.ICanvas canvas) { }
    }

    [TestMethod]
    public void OverlaySubtree_DoesNotAppearInPrimaryTree()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);

        context.OpenOverlay(Anchor.Center);
        context.OpenElement(new SizedStub(new Dimensions(4, 2)));
        context.CloseElement();
        context.CloseElement();

        LayoutNode root = context.EndLayout();

        Assert.AreEqual(0, root.Children.Count, "overlay must not be a child of the root");
        Assert.AreEqual(1, context.Overlays.Count);
        Assert.AreEqual(1, context.Overlays[0].Children.Count);
    }

    [TestMethod]
    public void OverlayRoot_FitsToContent()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);

        context.OpenOverlay(Anchor.Top | Anchor.Left);
        context.OpenElement(new SizedStub(new Dimensions(6, 3)));
        context.CloseElement();
        context.CloseElement();

        context.EndLayout();

        LayoutNode overlay = context.Overlays[0];
        Assert.AreEqual(new Dimensions(6, 3), overlay.Dimensions);
    }

    [TestMethod]
    public void Anchor_Center_PositionsAtCentreOfScreen()
    {
        Position pos = LayoutOverlayAt(Anchor.Center, contentSize: new Dimensions(10, 4));
        // (40-10)/2 = 15, (20-4)/2 = 8
        Assert.AreEqual(new Position(15, 8), pos);
    }

    [TestMethod]
    public void Anchor_TopLeft_PositionsAtOrigin()
    {
        Position pos = LayoutOverlayAt(
            Anchor.Top | Anchor.Left,
            contentSize: new Dimensions(10, 4)
        );
        Assert.AreEqual(new Position(0, 0), pos);
    }

    [TestMethod]
    public void Anchor_BottomRight_PositionsAtFarCorner()
    {
        Position pos = LayoutOverlayAt(
            Anchor.Bottom | Anchor.Right,
            contentSize: new Dimensions(10, 4)
        );
        // 40-10=30, 20-4=16
        Assert.AreEqual(new Position(30, 16), pos);
    }

    [TestMethod]
    public void Anchor_TopOnly_CentresHorizontally()
    {
        Position pos = LayoutOverlayAt(Anchor.Top, contentSize: new Dimensions(10, 4));
        Assert.AreEqual(new Position(15, 0), pos);
    }

    [TestMethod]
    public void Anchor_RightOnly_CentresVertically()
    {
        Position pos = LayoutOverlayAt(Anchor.Right, contentSize: new Dimensions(10, 4));
        Assert.AreEqual(new Position(30, 8), pos);
    }

    [TestMethod]
    public void Anchor_TopAndBottom_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);

        Assert.ThrowsExactly<ArgumentException>(() =>
            context.OpenOverlay(Anchor.Top | Anchor.Bottom)
        );
    }

    [TestMethod]
    public void Anchor_LeftAndRight_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);

        Assert.ThrowsExactly<ArgumentException>(() =>
            context.OpenOverlay(Anchor.Left | Anchor.Right)
        );
    }

    [TestMethod]
    public void MultipleOverlays_PreserveRegistrationOrder()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);

        context.OpenOverlay(Anchor.Top | Anchor.Left);
        context.OpenElement(new SizedStub(new Dimensions(2, 1)));
        context.CloseElement();
        context.CloseElement();

        context.OpenOverlay(Anchor.Bottom | Anchor.Right);
        context.OpenElement(new SizedStub(new Dimensions(4, 2)));
        context.CloseElement();
        context.CloseElement();

        context.EndLayout();

        Assert.AreEqual(2, context.Overlays.Count);
        Assert.AreEqual(new Position(0, 0), context.Overlays[0].Position);
        Assert.AreEqual(new Position(36, 18), context.Overlays[1].Position);
    }

    [TestMethod]
    public void Overlays_ClearedEachFrame()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(s_screen);
        context.OpenOverlay(Anchor.Center);
        context.OpenElement(new SizedStub(new Dimensions(2, 1)));
        context.CloseElement();
        context.CloseElement();
        context.EndLayout();
        Assert.AreEqual(1, context.Overlays.Count);

        context.BeginLayout(s_screen);
        context.EndLayout();
        Assert.AreEqual(0, context.Overlays.Count);
    }

    [TestMethod]
    public void UnclosedOverlay_ThrowsAtEndLayout()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);
        context.OpenOverlay(Anchor.Center);
        // Forget to close.

        Assert.ThrowsExactly<InvalidOperationException>(() => context.EndLayout());
    }

    [TestMethod]
    public void OverlayWithGrowStyle_FillsScreenOnGrowAxis()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);

        // Grow width, fit height: should be screen-wide but only as tall as content.
        context.OpenOverlay(
            Anchor.Top,
            style: new Layout
            {
                Direction = Direction.Vertical,
                Width = LayoutLength.Grow(),
                Height = LayoutLength.Fit(),
                ChildGap = 0,
                Padding = Padding.Zero,
            }
        );
        context.OpenElement(new SizedStub(new Dimensions(5, 2)));
        context.CloseElement();
        context.CloseElement();

        context.EndLayout();

        LayoutNode overlay = context.Overlays[0];
        Assert.AreEqual(s_screen.Width, overlay.Dimensions.Width);
        Assert.AreEqual(2, overlay.Dimensions.Height);
        // Anchor.Top with full width: x = 0 (centred of zero slack), y = 0.
        Assert.AreEqual(new Position(0, 0), overlay.Position);
    }

    // Lays out a single overlay containing a single fixed-size child and
    // returns the overlay root's computed Position.
    private static Position LayoutOverlayAt(Anchor anchor, Dimensions contentSize)
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_screen);

        context.OpenOverlay(anchor);
        context.OpenElement(new SizedStub(contentSize));
        context.CloseElement();
        context.CloseElement();

        context.EndLayout();
        return context.Overlays[0].Position;
    }
}
