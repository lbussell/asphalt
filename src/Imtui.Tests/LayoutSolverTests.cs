// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class LayoutSolverTests
{
    [TestMethod]
    public void DefaultStylesSplitChildrenEvenly()
    {
        Node root = Container(Direction.Horizontal, Widget(), Widget(), Widget());

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(5, 3));

        AssertRect(layout.Children[0].Bounds, 0, 0, 2, 3);
        AssertRect(layout.Children[1].Bounds, 2, 0, 2, 3);
        AssertRect(layout.Children[2].Bounds, 4, 0, 1, 3);
    }

    [TestMethod]
    public void FixedAndGrowChildrenShareAvailableSpace()
    {
        LayoutStyle fixedWidth = new LayoutStyle { Width = LayoutLength.Fixed(10) };
        Node root = Container(Direction.Horizontal, Widget(fixedWidth), Widget());

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(30, 5));

        AssertRect(layout.Children[0].Bounds, 0, 0, 10, 5);
        AssertRect(layout.Children[1].Bounds, 10, 0, 20, 5);
    }

    [TestMethod]
    public void GrowRaisesSmallestChildrenFirst()
    {
        LayoutStyle minimumFive = new LayoutStyle { Width = LayoutLength.Grow(minimum: 5) };
        LayoutStyle minimumZero = new LayoutStyle { Width = LayoutLength.Grow(minimum: 0) };
        Node root = Container(Direction.Horizontal, Widget(minimumFive), Widget(minimumZero));

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(10, 2));

        AssertRect(layout.Children[0].Bounds, 0, 0, 5, 2);
        AssertRect(layout.Children[1].Bounds, 5, 0, 5, 2);
    }

    [TestMethod]
    public void GrowStopsAtMaximumAndRedistributesSpace()
    {
        LayoutStyle maximumTwo = new LayoutStyle { Width = LayoutLength.Grow(maximum: 2) };
        Node root = Container(Direction.Horizontal, Widget(maximumTwo), Widget(), Widget());

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(12, 2));

        AssertRect(layout.Children[0].Bounds, 0, 0, 2, 2);
        AssertRect(layout.Children[1].Bounds, 2, 0, 5, 2);
        AssertRect(layout.Children[2].Bounds, 7, 0, 5, 2);
    }

    [TestMethod]
    public void FitSizingUsesPaddingAndChildGaps()
    {
        LayoutStyle fitContainer = new LayoutStyle
        {
            Width = LayoutLength.Fit(),
            Height = LayoutLength.Fit(),
            Padding = new Padding(1),
            ChildGap = 1,
        };
        LayoutStyle fitWidget = new LayoutStyle
        {
            Width = LayoutLength.Fit(),
            Height = LayoutLength.Fit(),
        };
        Node root = Container(
            Direction.Horizontal,
            Container(
                Direction.Vertical,
                fitContainer,
                Widget(new Dimensions(3, 2), fitWidget),
                Widget(new Dimensions(5, 1), fitWidget)
            )
        );

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(20, 10));
        LayoutNode fitLayout = layout.Children[0];

        AssertRect(fitLayout.Bounds, 0, 0, 7, 6);
        AssertRect(fitLayout.Children[0].Bounds, 1, 1, 3, 2);
        AssertRect(fitLayout.Children[1].Bounds, 1, 4, 5, 1);
    }

    [TestMethod]
    public void ShrinkReducesLargestChildrenFirst()
    {
        LayoutStyle fitMinimumOne = new LayoutStyle { Width = LayoutLength.Fit(minimum: 1) };
        Node root = Container(
            Direction.Horizontal,
            Widget(new Dimensions(4, 1), fitMinimumOne),
            Widget(new Dimensions(4, 1), fitMinimumOne)
        );

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(5, 1));

        AssertRect(layout.Children[0].Bounds, 0, 0, 2, 1);
        AssertRect(layout.Children[1].Bounds, 2, 0, 3, 1);
    }

    [TestMethod]
    public void UnderflowDoesNotProduceNegativeChildSizes()
    {
        LayoutStyle fixedTen = new LayoutStyle { Width = LayoutLength.Fixed(10) };
        Node root = Container(Direction.Horizontal, Widget(fixedTen), Widget(fixedTen));

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(5, 1));

        Assert.IsTrue(layout.Children[0].Bounds.Dimensions.Width >= 0);
        Assert.IsTrue(layout.Children[1].Bounds.Dimensions.Width >= 0);
        Assert.AreEqual(10, layout.Children[0].Bounds.Dimensions.Width);
        Assert.AreEqual(10, layout.Children[1].Bounds.Dimensions.Width);
    }

    [TestMethod]
    public void CrossAxisGrowRespectsMinimumSize()
    {
        LayoutStyle minimumHeight = new LayoutStyle { Height = LayoutLength.Grow(minimum: 10) };
        Node root = Container(Direction.Horizontal, Widget(minimumHeight));

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(5, 5));

        AssertRect(layout.Children[0].Bounds, 0, 0, 5, 10);
    }

    [TestMethod]
    public void RenderOnlyWidgetsFitToOneCell()
    {
        LayoutStyle fitWidget = new LayoutStyle
        {
            Width = LayoutLength.Fit(),
            Height = LayoutLength.Fit(),
        };
        Node root = Container(
            Direction.Horizontal,
            new Node { Widget = new RenderOnlyWidget(), Style = fitWidget }
        );

        LayoutNode layout = LayoutSolver.Solve(root, new Dimensions(5, 5));

        AssertRect(layout.Children[0].Bounds, 0, 0, 1, 1);
    }

    [TestMethod]
    public void BorderPanelCanContainText()
    {
        ImtuiContext imtui = new();

        using (
            imtui.BorderPanel(
                BorderStyle.Square,
                new LayoutStyle { Width = LayoutLength.Fixed(8), Height = LayoutLength.Fixed(3) }
            )
        )
        {
            imtui.Text("Hello");
        }

        LayoutNode layout = imtui.Build(new Dimensions(10, 5));
        LayoutNode panel = layout.Children[0];
        LayoutNode text = panel.Children[0];

        Assert.IsInstanceOfType(panel.Widget, typeof(BorderPanel));
        Assert.IsInstanceOfType(text.Widget, typeof(Text));
        AssertRect(panel.Bounds, 0, 0, 8, 3);
        AssertRect(text.Bounds, 1, 1, 5, 1);
    }

    [TestMethod]
    public void WidgetAuthorsCanPushScopedNodes()
    {
        ImtuiContext imtui = new();
        LayoutStyle style = new LayoutStyle
        {
            Width = LayoutLength.Fixed(8),
            Height = LayoutLength.Fixed(4),
            Padding = new Padding(1),
        };

        using (imtui.PushNode(Direction.Vertical, new TestWidget(new Dimensions(1, 1)), style))
        {
            imtui.Text("Hi");
        }

        LayoutNode layout = imtui.Build(new Dimensions(10, 5));
        LayoutNode customContainer = layout.Children[0];
        LayoutNode text = customContainer.Children[0];

        Assert.IsInstanceOfType(customContainer.Widget, typeof(TestWidget));
        AssertRect(customContainer.Bounds, 0, 0, 8, 4);
        AssertRect(text.Bounds, 1, 1, 2, 1);
    }

    private static Node Container(Direction direction, params Node[] children) =>
        Container(direction, LayoutStyle.Default, children);

    private static Node Container(Direction direction, LayoutStyle style, params Node[] children)
    {
        Node node = new Node { Direction = direction, Style = style };

        foreach (Node child in children)
            node.Children.Add(child);

        return node;
    }

    private static Node Widget() => Widget(new Dimensions(1, 1), LayoutStyle.Default);

    private static Node Widget(LayoutStyle style) => Widget(new Dimensions(1, 1), style);

    private static Node Widget(Dimensions measuredSize, LayoutStyle style) =>
        new Node { Widget = new TestWidget(measuredSize), Style = style };

    private static void AssertRect(Rect rect, int x, int y, int width, int height) =>
        Assert.AreEqual(new Rect(x, y, width, height), rect);

    private sealed class TestWidget(Dimensions measuredSize) : IWidget, IMeasurableWidget
    {
        public Dimensions Measure() => measuredSize;

        public void Render(Rect bounds, ICanvas canvas) { }
    }

    private sealed class RenderOnlyWidget : IWidget
    {
        public void Render(Rect bounds, ICanvas canvas) { }
    }
}
