// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed class ImtuiContext
{
    private readonly Stack<LayoutNode> _layoutStack = new();
    private readonly LayoutNode _root;

    public ImtuiContext()
    {
        _root = new LayoutNode();
        _layoutStack.Push(_root);
    }

    // Push a new child element onto the layout stack, making it the current parent.
    public void OpenElement(IWidget? widget = null, LayoutStyle? style = null)
    {
        LayoutNode node = new LayoutNode
        {
            Direction = style?.Direction ?? Direction.Vertical,
            Widget = widget,
            WidthLayout = style?.Width ?? LayoutLength.Fit(),
            HeightLayout = style?.Height ?? LayoutLength.Fit(),
        };
        _layoutStack.Peek().Children.Add(node);
        _layoutStack.Push(node);
    }

    // Pop the current element off the layout stack and accumulate its size
    // (including padding and gap) into the parent's dimensions.
    public void CloseElement()
    {
        if (_layoutStack.Peek() == _root)
            throw new InvalidOperationException("Cannot pop root element");

        LayoutNode child = _layoutStack.Pop();
        LayoutNode parent = _layoutStack.Peek();

        int width = child.Dimensions.Width + child.Padding.Left + child.Padding.Right;
        int height = child.Dimensions.Height + child.Padding.Top + child.Padding.Bottom;

        int gap = (parent.Children.Count - 1) * parent.Gap;

        if (parent.Direction == Direction.Vertical)
        {
            height += gap;
            parent.Dimensions = parent.Dimensions with
            {
                Width = Math.Max(child.Dimensions.Width, parent.Dimensions.Width),
            };
        }
        else // horizontal
        {
            width += gap;
            parent.Dimensions = parent.Dimensions with
            {
                Height = Math.Max(child.Dimensions.Height, parent.Dimensions.Height),
            };
        }

        child.Dimensions = child.Dimensions with { Width = width, Height = height };
    }

    // Finalize the layout tree. Sets the root to the given dimensions, then
    // distributes remaining space to any growable children at each level.
    public LayoutNode Build(Dimensions dimensions)
    {
        if (_layoutStack.Count != 1)
            throw new InvalidOperationException("Unclosed node scope.");

        _root.Dimensions = dimensions;

        // Walk the tree top-down so parents are sized before their children.
        TraverseBreadthFirst(
            _root,
            node =>
            {
                LayoutNode[] growable =
                [
                    .. node.Children.Where(c =>
                        node.Direction == Direction.Horizontal
                            ? c.WidthLayout.Kind == LayoutLengthKind.Grow
                            : c.HeightLayout.Kind == LayoutLengthKind.Grow
                    ),
                ];

                if (growable.Length == 0)
                    return;

                GrowChildElements(node, growable);
            }
        );

        return _root;
    }

    // Distribute the parent's remaining space (along its layout direction)
    // evenly among growable children. Uses a leveling algorithm: each
    // iteration grows the smallest children up toward the next-smallest, so
    // all growable elements converge to equal size.
    private static void GrowChildElements(LayoutNode parent, LayoutNode[] growable)
    {
        bool horizontal = parent.Direction == Direction.Horizontal;

        // Start with the parent's content area (minus padding).
        int remainingSpace = horizontal
            ? parent.Dimensions.Width - parent.Padding.TotalHorizontal
            : parent.Dimensions.Height - parent.Padding.TotalVertical;

        // Subtract space already consumed by all children and gaps.
        foreach (LayoutNode child in parent.Children)
            remainingSpace -= horizontal ? child.Dimensions.Width : child.Dimensions.Height;

        remainingSpace -= (parent.Children.Count - 1) * parent.Gap;

        while (remainingSpace > 0)
        {
            // Find the smallest and second-smallest sizes among growable children.
            int smallest = GetSize(growable[0]);
            int secondSmallest = int.MaxValue;
            int sizeToAdd = remainingSpace;

            foreach (LayoutNode child in growable)
            {
                int size = GetSize(child);

                if (size < smallest)
                {
                    secondSmallest = smallest;
                    smallest = size;
                }

                if (size > smallest)
                {
                    secondSmallest = Math.Min(secondSmallest, size);
                    sizeToAdd = secondSmallest - smallest;
                }
            }

            // Cap growth so we don't overshoot the available space.
            sizeToAdd = Math.Min(sizeToAdd, remainingSpace / growable.Length);

            if (sizeToAdd <= 0)
                break;

            // Apply the growth to every child that is at the smallest size.
            foreach (LayoutNode child in growable)
            {
                if (GetSize(child) == smallest)
                {
                    child.Dimensions = horizontal
                        ? (child.Dimensions with { Width = child.Dimensions.Width + sizeToAdd })
                        : (child.Dimensions with { Height = child.Dimensions.Height + sizeToAdd });

                    remainingSpace -= sizeToAdd;
                }
            }
        }

        int GetSize(LayoutNode node) => horizontal ? node.Dimensions.Width : node.Dimensions.Height;
    }

    private static void TraverseBreadthFirst(LayoutNode node, Action<LayoutNode> action)
    {
        foreach (LayoutNode child in node.Children)
        {
            action(child);
            TraverseBreadthFirst(child, action);
        }
    }
}
