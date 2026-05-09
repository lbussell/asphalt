// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public readonly record struct Rect(Position Position, Dimensions Dimensions)
{
    public Rect(int x, int y, int width, int height)
        : this(new Position(x, y), new Dimensions(width, height)) { }
}

public readonly record struct Position(int X, int Y);

public readonly record struct Dimensions(int Width, int Height);

public enum Direction
{
    Horizontal,
    Vertical,
}

public interface IWidget
{
    void Render(Rect bounds, ICanvas canvas);
}

public sealed class Node
{
    public Direction Direction { get; init; }
    public List<Node> Children { get; } = [];
    public IWidget? Widget { get; init; } // null for pure containers
}

public sealed record LayoutNode(Rect Bounds, IWidget? Widget, IReadOnlyList<LayoutNode> Children);

public static class Renderer
{
    public static void Render(LayoutNode node, ICanvas canvas)
    {
        node.Widget?.Render(node.Bounds, canvas);
        foreach (LayoutNode child in node.Children)
            Render(child, canvas);
    }
}

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
        Node node = new Node { Direction = direction };
        _layoutStack.Peek().Children.Add(node);
        _layoutStack.Push(node);
        return new WidgetScope(this);
    }

    // For widget authors / extension methods: attach a custom widget as a leaf.
    public void AddWidget(IWidget widget)
    {
        Node node = new Node { Widget = widget };
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

public sealed class ColorBlock() : IWidget
{
    public void Render(Rect bounds, ICanvas canvas) =>
        canvas.Fill(bounds, TerminalColorRgb.Random());
}

public static class WidgetExtensions
{
    public static void ColorBlock(this ImtuiContext context) => context.AddWidget(new ColorBlock());
}
