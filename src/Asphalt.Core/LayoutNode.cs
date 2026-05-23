// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

public sealed record LayoutNode
{
    public Dimensions Dimensions { get; set; }
    public Position Position { get; set; }
    public IWidget? Widget { get; init; } = null;
    public Direction Direction { get; init; } = Direction.Vertical;
    public Padding Padding { get; init; }
    public int Gap { get; init; }
    public LayoutLength WidthLayout { get; init; } = LayoutLength.Fit();
    public LayoutLength HeightLayout { get; init; } = LayoutLength.Fit();
    public List<LayoutNode> Children { get; } = [];
}

public static class LayoutNodeExtensions
{
    extension(Dimensions dimensions)
    {
        public int GetSize(Direction axis) =>
            axis switch
            {
                Direction.Horizontal => dimensions.Width,
                Direction.Vertical => dimensions.Height,
                _ => throw new InvalidOperationException($"Unknown direction: {axis}"),
            };
    }

    extension(LayoutNode node)
    {
        public Direction CrossAxisDirection =>
            node.Direction switch
            {
                Direction.Horizontal => Direction.Vertical,
                Direction.Vertical => Direction.Horizontal,
                _ => throw new InvalidOperationException($"Unknown direction: {node.Direction}"),
            };

        public int OnAxisPadding =>
            node.Direction switch
            {
                Direction.Horizontal => node.Padding.TotalHorizontal,
                Direction.Vertical => node.Padding.TotalVertical,
                _ => throw new InvalidOperationException($"Unknown direction: {node.Direction}"),
            };

        public int CrossAxisPadding =>
            node.Direction switch
            {
                Direction.Horizontal => node.Padding.TotalVertical,
                Direction.Vertical => node.Padding.TotalHorizontal,
                _ => throw new InvalidOperationException($"Unknown direction: {node.Direction}"),
            };

        public int OnAxisSize => node.GetSize(node.Direction);
        public int CrossAxisSize => node.GetSize(node.CrossAxisDirection);
        public int OnAxisContentSize => Math.Max(0, node.OnAxisSize - node.OnAxisPadding);
        public int CrossAxisContentSize => Math.Max(0, node.CrossAxisSize - node.CrossAxisPadding);
        public int ContentWidth =>
            Math.Max(0, node.Dimensions.Width - node.Padding.TotalHorizontal);
        public int ContentHeight =>
            Math.Max(0, node.Dimensions.Height - node.Padding.TotalVertical);
        public Dimensions ContentDimensions => new(node.ContentWidth, node.ContentHeight);
        public int GapSize => Math.Max(0, node.Children.Count - 1) * node.Gap;
        public Rect Bounds => new(node.Position, node.Dimensions);
        public Position ContentPosition =>
            new(node.Position.X + node.Padding.Left, node.Position.Y + node.Padding.Top);
        public Rect ContentRect => new(node.ContentPosition, node.ContentDimensions);
        public Position FirstChildPosition => node.ContentPosition;
        public WidgetLayout IntrinsicLayout => node.Widget?.Measure() ?? WidgetLayout.Zero;

        public int LayoutAxisChildrenSize
        {
            get
            {
                int size = node.GapSize;

                foreach (LayoutNode child in node.Children)
                    size += node.ChildOnAxisSize(child);

                return size;
            }
        }

        public int GetSize(Direction axis) =>
            axis switch
            {
                Direction.Horizontal => node.Dimensions.Width,
                Direction.Vertical => node.Dimensions.Height,
                _ => throw new InvalidOperationException($"Unknown direction: {axis}"),
            };

        public void SetSize(Direction axis, int size)
        {
            size = Math.Max(0, size);
            node.Dimensions = axis switch
            {
                Direction.Horizontal => node.Dimensions with { Width = size },
                Direction.Vertical => node.Dimensions with { Height = size },
                _ => throw new InvalidOperationException($"Unknown direction: {axis}"),
            };
        }

        public void AddSize(Direction axis, int amount) =>
            node.SetSize(axis, node.GetSize(axis) + amount);

        public LayoutLength GetLayout(Direction axis) =>
            axis switch
            {
                Direction.Horizontal => node.WidthLayout,
                Direction.Vertical => node.HeightLayout,
                _ => throw new InvalidOperationException($"Unknown direction: {axis}"),
            };

        public int GetPadding(Direction axis) =>
            axis switch
            {
                Direction.Horizontal => node.Padding.TotalHorizontal,
                Direction.Vertical => node.Padding.TotalVertical,
                _ => throw new InvalidOperationException($"Unknown direction: {axis}"),
            };

        public int ChildrenContentWidth =>
            node.Children.Count == 0 ? 0
            : node.Direction == Direction.Horizontal ? node.LayoutAxisChildrenSize
            : node.Children.Max(child => child.Dimensions.Width);

        public int ChildrenContentHeight =>
            node.Children.Count == 0 ? 0
            : node.Direction == Direction.Vertical ? node.LayoutAxisChildrenSize
            : node.Children.Max(child => child.Dimensions.Height);

        public int LaidOutContentHeight
        {
            get
            {
                int height = node.ChildrenContentHeight;

                if (node.Widget is not null)
                    height = Math.Max(height, node.ContentHeight);

                return height;
            }
        }

        public Dimensions MeasureContent()
        {
            int width = node.ChildrenContentWidth;
            int height = node.ChildrenContentHeight;
            WidgetLayout intrinsicLayout = node.IntrinsicLayout;

            width = Math.Max(width, intrinsicLayout.Preferred.Width);
            height = Math.Max(height, intrinsicLayout.Preferred.Height);

            return new Dimensions(width, height);
        }

        public void SetPreferredDimensions()
        {
            Dimensions content = node.MeasureContent();
            node.Dimensions = new Dimensions(
                ResolvePreferredSize(
                    node.WidthLayout,
                    content.Width + node.Padding.TotalHorizontal
                ),
                ResolvePreferredSize(node.HeightLayout, content.Height + node.Padding.TotalVertical)
            );
        }

        public void SetPreferredHeight()
        {
            node.Dimensions = node.Dimensions with
            {
                Height = ResolvePreferredSize(
                    node.HeightLayout,
                    node.LaidOutContentHeight + node.Padding.TotalVertical
                ),
            };
        }

        public void SetHeightFromContent(int contentHeight)
        {
            contentHeight = Math.Max(0, contentHeight);
            node.Dimensions = node.Dimensions with
            {
                Height = ResolvePreferredSize(
                    node.HeightLayout,
                    contentHeight + node.Padding.TotalVertical
                ),
            };
        }

        public void LayoutWidget()
        {
            if (node.Widget is null)
                return;

            Dimensions content = node.Widget.Layout(node.ContentDimensions);

            if (node.HeightLayout.Kind != LayoutLengthKind.Fixed)
                node.SetHeightFromContent(content.Height);
        }

        public int GetMinimumContentSize(Direction axis)
        {
            int size = 0;

            if (node.Children.Count > 0)
            {
                if (axis == node.Direction)
                {
                    size = node.GapSize;
                    foreach (LayoutNode child in node.Children)
                        size += child.GetMinimumOuterSize(axis);
                }
                else
                {
                    foreach (LayoutNode child in node.Children)
                        size = Math.Max(size, child.GetMinimumOuterSize(axis));
                }
            }

            WidgetLayout intrinsicLayout = node.IntrinsicLayout;
            int intrinsicSize = intrinsicLayout.Minimum.GetSize(axis);
            size = Math.Max(size, intrinsicSize);

            return size;
        }

        public int GetMinimumOuterSize(Direction axis)
        {
            LayoutLength layout = node.GetLayout(axis);

            if (layout.Kind == LayoutLengthKind.Fixed)
                return layout.Value;

            int contentSize = node.GetMinimumContentSize(axis);
            return Clamp(contentSize + node.GetPadding(axis), layout.Minimum, layout.Maximum);
        }

        public int GetMaximumOuterSize(Direction axis)
        {
            LayoutLength layout = node.GetLayout(axis);
            return layout.Kind == LayoutLengthKind.Fixed ? layout.Value : layout.Maximum;
        }

        public int ChildOnAxisSize(LayoutNode child) => child.GetSize(node.Direction);

        public int ChildCrossAxisSize(LayoutNode child) => child.GetSize(node.CrossAxisDirection);

        public LayoutLength ChildOnAxisLayout(LayoutNode child) => child.GetLayout(node.Direction);

        public LayoutLength ChildCrossAxisLayout(LayoutNode child) =>
            child.GetLayout(node.CrossAxisDirection);

        public int ChildMinimumOuterSizeOnAxis(LayoutNode child) =>
            child.GetMinimumOuterSize(node.Direction);

        public int ChildMinimumOuterSizeOnCrossAxis(LayoutNode child) =>
            child.GetMinimumOuterSize(node.CrossAxisDirection);

        public int ChildMaximumOuterSizeOnAxis(LayoutNode child) =>
            child.GetMaximumOuterSize(node.Direction);

        public int ChildMaximumOuterSizeOnCrossAxis(LayoutNode child) =>
            child.GetMaximumOuterSize(node.CrossAxisDirection);

        public void AddChildOnAxisSize(LayoutNode child, int amount) =>
            child.AddSize(node.Direction, amount);

        public void SetChildCrossAxisSize(LayoutNode child, int size) =>
            child.SetSize(node.CrossAxisDirection, size);

        public void SizeChildOnCrossAxis(LayoutNode child)
        {
            LayoutLength layout = node.ChildCrossAxisLayout(child);

            if (layout.Kind == LayoutLengthKind.Fixed)
                return;

            int minimum = node.ChildMinimumOuterSizeOnCrossAxis(child);
            int maximum = Math.Min(
                node.ChildMaximumOuterSizeOnCrossAxis(child),
                node.CrossAxisContentSize
            );
            int size = node.ChildCrossAxisSize(child);

            if (layout.Kind == LayoutLengthKind.Grow)
                size = maximum;

            node.SetChildCrossAxisSize(child, Math.Max(minimum, Math.Min(size, maximum)));
        }

        public void SizeChildrenAlongWidth()
        {
            if (node.Direction == Direction.Horizontal)
                node.SizeChildrenOnLayoutAxis();
            else
                node.SizeChildrenOnCrossAxis();
        }

        public void SizeChildrenAlongHeight()
        {
            if (node.Direction == Direction.Vertical)
                node.SizeChildrenOnLayoutAxis();
            else
                node.SizeChildrenOnCrossAxis();
        }

        public void SizeChildrenOnLayoutAxis()
        {
            if (node.Children.Count == 0)
                return;

            int remainingSpace = node.OnAxisContentSize - node.LayoutAxisChildrenSize;

            if (remainingSpace < 0)
            {
                LayoutNode[] shrinkable =
                [
                    .. node.Children.Where(child =>
                        node.ChildOnAxisLayout(child).Kind != LayoutLengthKind.Fixed
                    ),
                ];
                node.ShrinkChildrenOnAxis(shrinkable, -remainingSpace);
            }
            else if (remainingSpace > 0)
            {
                LayoutNode[] growable =
                [
                    .. node.Children.Where(child =>
                        node.ChildOnAxisLayout(child).Kind == LayoutLengthKind.Grow
                    ),
                ];
                node.GrowChildrenOnAxis(growable, remainingSpace);
            }
        }

        public void SizeChildrenOnCrossAxis()
        {
            foreach (LayoutNode child in node.Children)
                node.SizeChildOnCrossAxis(child);
        }

        public Position NextChildPosition(Position position, LayoutNode child) =>
            node.Direction switch
            {
                Direction.Horizontal => position with
                {
                    X = position.X + node.ChildOnAxisSize(child) + node.Gap,
                },
                Direction.Vertical => position with
                {
                    Y = position.Y + node.ChildOnAxisSize(child) + node.Gap,
                },
                _ => throw new InvalidOperationException($"Unknown direction: {node.Direction}"),
            };

        public void GrowChildrenOnAxis(LayoutNode[] children, int remainingSpace)
        {
            while (remainingSpace > 0)
            {
                LayoutNode[] growable =
                [
                    .. children.Where(child =>
                        node.ChildOnAxisSize(child) < node.ChildMaximumOuterSizeOnAxis(child)
                    ),
                ];

                if (growable.Length == 0)
                    break;

                int smallest = growable.Min(child => node.ChildOnAxisSize(child));
                LayoutNode[] smallestChildren =
                [
                    .. growable.Where(child => node.ChildOnAxisSize(child) == smallest),
                ];
                int nextSmallest = growable
                    .Where(child => node.ChildOnAxisSize(child) > smallest)
                    .Select(child => node.ChildOnAxisSize(child))
                    .DefaultIfEmpty(int.MaxValue)
                    .Min();
                int sizeToAdd = smallestChildren.Min(child =>
                    node.ChildMaximumOuterSizeOnAxis(child) - smallest
                );

                if (nextSmallest != int.MaxValue)
                    sizeToAdd = Math.Min(sizeToAdd, nextSmallest - smallest);

                sizeToAdd = Math.Min(sizeToAdd, remainingSpace / smallestChildren.Length);

                if (sizeToAdd == 0)
                {
                    foreach (LayoutNode child in smallestChildren)
                    {
                        if (remainingSpace == 0)
                            break;

                        node.AddChildOnAxisSize(child, 1);
                        remainingSpace--;
                    }
                }
                else
                {
                    foreach (LayoutNode child in smallestChildren)
                    {
                        node.AddChildOnAxisSize(child, sizeToAdd);
                        remainingSpace -= sizeToAdd;
                    }
                }
            }
        }

        public void ShrinkChildrenOnAxis(LayoutNode[] children, int overflow)
        {
            while (overflow > 0)
            {
                LayoutNode[] shrinkable =
                [
                    .. children.Where(child =>
                        node.ChildOnAxisSize(child) > node.ChildMinimumOuterSizeOnAxis(child)
                    ),
                ];

                if (shrinkable.Length == 0)
                    break;

                int largest = shrinkable.Max(child => node.ChildOnAxisSize(child));
                LayoutNode[] largestChildren =
                [
                    .. shrinkable.Where(child => node.ChildOnAxisSize(child) == largest),
                ];
                int nextLargest = shrinkable
                    .Where(child => node.ChildOnAxisSize(child) < largest)
                    .Select(child => node.ChildOnAxisSize(child))
                    .DefaultIfEmpty(0)
                    .Max();
                int sizeToRemove = largestChildren.Min(child =>
                    largest - node.ChildMinimumOuterSizeOnAxis(child)
                );

                if (nextLargest > 0)
                    sizeToRemove = Math.Min(sizeToRemove, largest - nextLargest);

                sizeToRemove = Math.Min(sizeToRemove, overflow / largestChildren.Length);

                if (sizeToRemove == 0)
                {
                    foreach (LayoutNode child in largestChildren)
                    {
                        if (overflow == 0)
                            break;

                        node.AddChildOnAxisSize(child, -1);
                        overflow--;
                    }
                }
                else
                {
                    foreach (LayoutNode child in largestChildren)
                    {
                        node.AddChildOnAxisSize(child, -sizeToRemove);
                        overflow -= sizeToRemove;
                    }
                }
            }
        }
    }

    private static int ResolvePreferredSize(LayoutLength layout, int contentSize) =>
        layout.Kind == LayoutLengthKind.Fixed
            ? layout.Value
            : Clamp(contentSize, layout.Minimum, layout.Maximum);

    private static int Clamp(int value, int minimum, int maximum) =>
        Math.Min(Math.Max(value, minimum), maximum);
}
