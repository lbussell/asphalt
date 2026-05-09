// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public static class LayoutSolver
{
    private enum Axis
    {
        Horizontal,
        Vertical,
    }

    public static LayoutNode Solve(Node root, Dimensions dimensions)
    {
        SolverNode solverRoot = BuildSolverNode(root);

        ComputeFitSize(solverRoot);
        solverRoot.Position = new Position(0, 0);
        solverRoot.Size = new Dimensions(
            Math.Max(0, dimensions.Width),
            Math.Max(0, dimensions.Height)
        );

        ResolveSizes(solverRoot);
        PositionChildren(solverRoot);

        return BuildLayoutNode(solverRoot);
    }

    private static SolverNode BuildSolverNode(Node node)
    {
        SolverNode solverNode = new SolverNode(node);

        foreach (Node child in node.Children)
            solverNode.Children.Add(BuildSolverNode(child));

        return solverNode;
    }

    private static Dimensions ComputeFitSize(SolverNode node)
    {
        foreach (SolverNode child in node.Children)
            ComputeFitSize(child);

        Dimensions childFitSize = ComputeChildFitSize(node);
        Dimensions widgetFitSize = MeasureWidget(node.Node.Widget);

        int width = Math.Max(childFitSize.Width, widgetFitSize.Width);
        int height = Math.Max(childFitSize.Height, widgetFitSize.Height);

        node.FitSize = new Dimensions(
            ResolveFitLength(node.Node.Style.Width, width),
            ResolveFitLength(node.Node.Style.Height, height)
        );

        return node.FitSize;
    }

    private static Dimensions ComputeChildFitSize(SolverNode node)
    {
        LayoutStyle style = node.Node.Style;
        Padding padding = style.Padding;

        if (node.Children.Count == 0)
            return new Dimensions(padding.Horizontal, padding.Vertical);

        int gapSize = style.ChildGap * (node.Children.Count - 1);

        if (node.Node.Direction == Direction.Horizontal)
        {
            int width = padding.Horizontal + gapSize;
            int height = 0;

            foreach (SolverNode child in node.Children)
            {
                width += child.FitSize.Width;
                height = Math.Max(height, child.FitSize.Height);
            }

            return new Dimensions(width, height + padding.Vertical);
        }

        int verticalWidth = 0;
        int verticalHeight = padding.Vertical + gapSize;

        foreach (SolverNode child in node.Children)
        {
            verticalWidth = Math.Max(verticalWidth, child.FitSize.Width);
            verticalHeight += child.FitSize.Height;
        }

        return new Dimensions(verticalWidth + padding.Horizontal, verticalHeight);
    }

    private static Dimensions MeasureWidget(IWidget? widget)
    {
        if (widget is null)
            return new Dimensions(0, 0);

        if (widget is IMeasurableWidget measurableWidget)
        {
            Dimensions measuredSize = measurableWidget.Measure();
            return new Dimensions(
                Math.Max(0, measuredSize.Width),
                Math.Max(0, measuredSize.Height)
            );
        }

        return new Dimensions(1, 1);
    }

    private static void ResolveSizes(SolverNode node)
    {
        if (node.Children.Count == 0)
            return;

        Axis layoutAxis =
            node.Node.Direction == Direction.Horizontal ? Axis.Horizontal : Axis.Vertical;
        Axis crossAxis = layoutAxis == Axis.Horizontal ? Axis.Vertical : Axis.Horizontal;

        ResolveLayoutAxis(node, layoutAxis);
        ResolveCrossAxis(node, crossAxis);

        foreach (SolverNode child in node.Children)
            ResolveSizes(child);
    }

    private static void ResolveLayoutAxis(SolverNode parent, Axis axis)
    {
        List<AxisItem> items = BuildAxisItems(parent, axis);
        int availableSize = GetInnerSize(parent, axis);
        int totalSize = SumSizes(items);
        int remainingSize = availableSize - totalSize;

        if (remainingSize > 0)
            GrowSizes(items, remainingSize);
        else if (remainingSize < 0)
            ShrinkSizes(items, -remainingSize);

        foreach (AxisItem item in items)
            SetSize(item.Node, axis, item.Size);
    }

    private static void ResolveCrossAxis(SolverNode parent, Axis axis)
    {
        int availableSize = GetInnerSize(parent, axis);

        foreach (SolverNode child in parent.Children)
        {
            LayoutLength length = GetLength(child.Node.Style, axis);
            int fitSize = GetSize(child.FitSize, axis);
            int size = ResolveCrossSize(length, fitSize, availableSize);
            SetSize(child, axis, size);
        }
    }

    private static List<AxisItem> BuildAxisItems(SolverNode parent, Axis axis)
    {
        List<AxisItem> items = new List<AxisItem>(parent.Children.Count);

        foreach (SolverNode child in parent.Children)
        {
            LayoutLength length = GetLength(child.Node.Style, axis);
            int fitSize = GetSize(child.FitSize, axis);
            int preferredSize = ResolvePreferredSize(length, fitSize);
            items.Add(new AxisItem(child, length, preferredSize));
        }

        return items;
    }

    private static int GetInnerSize(SolverNode node, Axis axis)
    {
        Padding padding = node.Node.Style.Padding;
        int outerSize = GetSize(node.Size, axis);
        int paddingSize = axis == Axis.Horizontal ? padding.Horizontal : padding.Vertical;
        int gapSize =
            axis == GetLayoutAxis(node)
                ? node.Node.Style.ChildGap * Math.Max(0, node.Children.Count - 1)
                : 0;

        return Math.Max(0, outerSize - paddingSize - gapSize);
    }

    private static Axis GetLayoutAxis(SolverNode node) =>
        node.Node.Direction == Direction.Horizontal ? Axis.Horizontal : Axis.Vertical;

    private static void GrowSizes(List<AxisItem> items, int remainingSize)
    {
        while (remainingSize > 0)
        {
            AxisItem? target = FindSmallestGrowItem(items);

            if (target is null)
                return;

            target.Size++;
            remainingSize--;
        }
    }

    private static void ShrinkSizes(List<AxisItem> items, int deficit)
    {
        while (deficit > 0)
        {
            AxisItem? target = FindLargestShrinkItem(items);

            if (target is null)
                return;

            target.Size--;
            deficit--;
        }
    }

    private static AxisItem? FindSmallestGrowItem(List<AxisItem> items)
    {
        AxisItem? target = null;

        foreach (AxisItem item in items)
        {
            if (item.Length.Kind != LayoutLengthKind.Grow || item.Size >= item.Length.Maximum)
                continue;

            if (target is null || item.Size < target.Size)
                target = item;
        }

        return target;
    }

    private static AxisItem? FindLargestShrinkItem(List<AxisItem> items)
    {
        AxisItem? target = null;

        foreach (AxisItem item in items)
        {
            if (item.Length.Kind == LayoutLengthKind.Fixed)
                continue;

            if (item.Size <= item.Length.Minimum)
                continue;

            if (target is null || item.Size > target.Size)
                target = item;
        }

        return target;
    }

    private static int SumSizes(List<AxisItem> items)
    {
        int total = 0;

        foreach (AxisItem item in items)
            total += item.Size;

        return total;
    }

    private static int ResolveFitLength(LayoutLength length, int fitSize)
    {
        if (length.Kind == LayoutLengthKind.Fixed)
            return length.Value;

        return Clamp(fitSize, length.Minimum, length.Maximum);
    }

    private static int ResolvePreferredSize(LayoutLength length, int fitSize)
    {
        if (length.Kind == LayoutLengthKind.Fixed)
            return length.Value;

        return Clamp(fitSize, length.Minimum, length.Maximum);
    }

    private static int ResolveCrossSize(LayoutLength length, int fitSize, int availableSize)
    {
        if (length.Kind == LayoutLengthKind.Grow)
            return Clamp(availableSize, length.Minimum, length.Maximum);

        return ResolvePreferredSize(length, fitSize);
    }

    private static int Clamp(int value, int minimum, int maximum) =>
        Math.Min(Math.Max(value, minimum), maximum);

    private static LayoutLength GetLength(LayoutStyle style, Axis axis) =>
        axis == Axis.Horizontal ? style.Width : style.Height;

    private static int GetSize(Dimensions dimensions, Axis axis) =>
        axis == Axis.Horizontal ? dimensions.Width : dimensions.Height;

    private static void SetSize(SolverNode node, Axis axis, int size)
    {
        node.Size =
            axis == Axis.Horizontal
                ? new Dimensions(size, node.Size.Height)
                : new Dimensions(node.Size.Width, size);
    }

    private static void PositionChildren(SolverNode node)
    {
        if (node.Children.Count == 0)
            return;

        Padding padding = node.Node.Style.Padding;
        bool horizontal = node.Node.Direction == Direction.Horizontal;
        int currentX = node.Position.X + padding.Left;
        int currentY = node.Position.Y + padding.Top;

        foreach (SolverNode child in node.Children)
        {
            child.Position = new Position(currentX, currentY);
            PositionChildren(child);

            if (horizontal)
                currentX += child.Size.Width + node.Node.Style.ChildGap;
            else
                currentY += child.Size.Height + node.Node.Style.ChildGap;
        }
    }

    private static LayoutNode BuildLayoutNode(SolverNode node)
    {
        List<LayoutNode> children = new List<LayoutNode>(node.Children.Count);

        foreach (SolverNode child in node.Children)
            children.Add(BuildLayoutNode(child));

        return new LayoutNode(new Rect(node.Position, node.Size), node.Node.Widget, children);
    }

    private sealed class SolverNode(Node node)
    {
        public Node Node { get; } = node;
        public List<SolverNode> Children { get; } = [];
        public Dimensions FitSize { get; set; }
        public Dimensions Size { get; set; }
        public Position Position { get; set; }
    }

    private sealed class AxisItem(SolverNode node, LayoutLength length, int size)
    {
        public SolverNode Node { get; } = node;
        public LayoutLength Length { get; } = length;
        public int Size { get; set; } = size;
    }
}
