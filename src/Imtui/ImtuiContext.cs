// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed class ImtuiContext
{
    private readonly Stack<Node> _layoutStack = new();
    private readonly Node _root;

    public ImtuiContext()
    {
        // Implicit root container so users can call ui.Stack(...) at the top level.
        Node root = new Node { Direction = Direction.Vertical };
        _root = root;
        _layoutStack.Push(root);
    }

    public WidgetScope Container(Direction direction)
    {
        return Container(direction, LayoutStyle.Default);
    }

    public WidgetScope Container(Direction direction, LayoutStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);

        Node node = new Node { Direction = direction, Style = style };
        _layoutStack.Peek().Children.Add(node);
        _layoutStack.Push(node);
        return new WidgetScope(this);
    }

    // For widget authors / extension methods: attach a custom widget as a leaf.
    public void AddWidget(IWidget widget)
    {
        AddWidget(widget, LayoutStyle.Default);
    }

    public void AddWidget(IWidget widget, LayoutStyle style)
    {
        ArgumentNullException.ThrowIfNull(widget);
        ArgumentNullException.ThrowIfNull(style);

        Node node = new Node { Widget = widget, Style = style };
        _layoutStack.Peek().Children.Add(node);
    }

    public LayoutNode Build(Dimensions dimensions)
    {
        if (_layoutStack.Count != 1)
            throw new InvalidOperationException("Unclosed Stack scope.");

        return LayoutSolver.Solve(_root, dimensions);
    }

    internal void Pop() => _layoutStack.Pop();

    public readonly struct WidgetScope(ImtuiContext context) : IDisposable
    {
        public void Dispose() => context.Pop();
    }
}
