// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

using Asphalt.Widgets;

[TestClass]
public class LayoutHarnessTests
{
    [TestMethod]
    public void EmptyFrame_RootMatchesTerminalDimensions()
    {
        LayoutNode root = AsphaltTestHarness.RunFrame(_ => { }, width: 80, height: 24);

        Assert.AreEqual(new Dimensions(80, 24), root.Dimensions);
        Assert.AreEqual(new Position(0, 0), root.Position);
        Assert.AreEqual(0, root.Children.Count);
    }

    [TestMethod]
    public void GrowChild_FillsAvailableSpace()
    {
        LayoutNode root = AsphaltTestHarness.RunFrame(
            context =>
            {
                context.OpenElement(
                    style: new Layout { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
                );
                context.CloseElement();
            },
            width: 40,
            height: 10
        );

        LayoutNode child = root.Children.Single();
        Assert.AreEqual(new Dimensions(40, 10), child.Dimensions);
        Assert.AreEqual(new Position(0, 0), child.Position);
    }

    [TestMethod]
    public void HStack_PlacesChildrenSideBySide()
    {
        LayoutNode root = AsphaltTestHarness.RunFrame(
            context =>
            {
                using (context.HStack(gap: 1))
                {
                    context.Text("hello");
                    context.Text("world");
                }
            },
            width: 80,
            height: 24
        );

        LayoutNode stack = root.Children.Single();
        Assert.AreEqual(Direction.Horizontal, stack.Direction);
        Assert.AreEqual(2, stack.Children.Count);

        LayoutNode first = stack.Children[0];
        LayoutNode second = stack.Children[1];

        Assert.AreEqual(0, first.Position.X);
        Assert.AreEqual(first.Position.Y, second.Position.Y);
        Assert.AreEqual(first.Position.X + first.Dimensions.Width + 1, second.Position.X);
    }

    [TestMethod]
    public void VStack_PlacesChildrenStackedVertically()
    {
        LayoutNode root = AsphaltTestHarness.RunFrame(
            context =>
            {
                using (context.VStack())
                {
                    context.Text("top");
                    context.Text("bottom");
                }
            },
            width: 80,
            height: 24
        );

        LayoutNode stack = root.Children.Single();
        Assert.AreEqual(Direction.Vertical, stack.Direction);
        Assert.AreEqual(2, stack.Children.Count);

        LayoutNode first = stack.Children[0];
        LayoutNode second = stack.Children[1];

        Assert.AreEqual(first.Position.X, second.Position.X);
        Assert.AreEqual(first.Position.Y + first.Dimensions.Height, second.Position.Y);
    }

    [TestMethod]
    public void Panel_UniqueKey_AllowsRepeatedCallSite()
    {
        LayoutNode root = AsphaltTestHarness.RunFrame(
            context =>
            {
                for (int index = 0; index < 2; index++)
                {
                    using (context.Panel(uniqueKey: index.ToString()))
                    {
                        context.Text(index.ToString());
                    }
                }
            },
            width: 80,
            height: 24
        );

        Assert.AreEqual(2, root.Children.Count);
    }

    [TestMethod]
    public void Walk_VisitsEntireTreeInPreOrder()
    {
        LayoutNode root = AsphaltTestHarness.RunFrame(
            context =>
            {
                using (context.VStack())
                {
                    context.Text("a");
                    using (context.HStack())
                    {
                        context.Text("b");
                        context.Text("c");
                    }
                }
            },
            width: 80,
            height: 24
        );

        int totalNodes = root.Walk().Count();
        int textNodes = root.NodesWithWidget<TextWidget.Implementation>().Count();

        // root + outer VStack + inner HStack + 3 text widgets
        Assert.AreEqual(6, totalNodes);
        Assert.AreEqual(3, textNodes);
    }
}
