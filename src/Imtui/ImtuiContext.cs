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

    public void OpenElement(IWidget? widget = null, LayoutStyle? style = null)
    {
        LayoutNode node = new LayoutNode
        {
            Direction = style?.Direction ?? Direction.Vertical,
            Widget = widget,
        };
        _layoutStack.Peek().Children.Add(node);
        _layoutStack.Push(node);
    }

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

    public LayoutNode Build(Dimensions dimensions)
    {
        if (_layoutStack.Count != 1)
            throw new InvalidOperationException("Unclosed node scope.");

        return _root;
    }
}
