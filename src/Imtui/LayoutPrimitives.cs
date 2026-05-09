// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public enum Direction
{
    Horizontal,
    Vertical,
}

public readonly record struct Position(int X, int Y);

public readonly record struct Dimensions(int Width, int Height);

public readonly record struct Rect(Position Position, Dimensions Dimensions)
{
    public Rect(int x, int y, int width, int height)
        : this(new Position(x, y), new Dimensions(width, height)) { }
}

public sealed class Node
{
    public Direction Direction { get; init; }
    public List<Node> Children { get; } = [];
    public IWidget? Widget { get; init; } // null for pure containers
}

public interface IWidget
{
    void Render(Rect bounds, ICanvas canvas);
}

public sealed record LayoutNode(Rect Bounds, IWidget? Widget, IReadOnlyList<LayoutNode> Children);
