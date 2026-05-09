// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed class ImtuiContext
{
    private readonly Stack<Node> _layoutStack = new();
    private readonly Node _root;

    public ImtuiContext()
    {
        // Implicit root container so users can add top-level widgets.
        Node root = new Node { Direction = Direction.Vertical };
        _root = root;
        _layoutStack.Push(root);
    }

    public WidgetScope PushNode(
        Direction direction,
        IWidget? widget = null,
        LayoutStyle? style = null
    )
    {
        Node node = new Node
        {
            Direction = direction,
            Widget = widget,
            Style = style ?? LayoutStyle.Default,
        };
        _layoutStack.Peek().Children.Add(node);
        _layoutStack.Push(node);
        return new WidgetScope(this);
    }

    // For widget authors / extension methods: attach a custom widget as a leaf.
    public void AddWidget(IWidget widget, LayoutStyle? style = null)
    {
        ArgumentNullException.ThrowIfNull(widget);

        Node node = new Node { Widget = widget, Style = style ?? LayoutStyle.Default };
        _layoutStack.Peek().Children.Add(node);
    }

    public LayoutNode Build(Dimensions dimensions)
    {
        if (_layoutStack.Count != 1)
            throw new InvalidOperationException("Unclosed node scope.");

        return LayoutSolver.Solve(_root, dimensions);
    }

    internal void Pop() => _layoutStack.Pop();

    public readonly struct WidgetScope(ImtuiContext context) : IDisposable
    {
        public void Dispose() => context.Pop();
    }
}

public static class ContainerExtensions
{
    public static ImtuiContext.WidgetScope Container(
        this ImtuiContext context,
        Direction direction,
        LayoutStyle? style = null
    ) => context.PushNode(direction, style: style);
}
