// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public enum Direction
{
    Horizontal,
    Vertical,
}

public enum LayoutLengthKind
{
    Fit,
    Fixed,
    Grow,
}

public readonly record struct Position(int X, int Y);

public readonly record struct Dimensions(int Width, int Height);

public readonly record struct Rect(Position Position, Dimensions Dimensions)
{
    public Rect(int x, int y, int width, int height)
        : this(new Position(x, y), new Dimensions(width, height)) { }
}

public readonly record struct Padding(int Left, int Top, int Right, int Bottom)
{
    public static Padding Zero { get; } = new(0);

    public Padding(int all)
        : this(all, all, all, all) { }

    public Padding(int horizontal, int vertical)
        : this(horizontal, vertical, horizontal, vertical) { }

    public int Left { get; init; } = ThrowIfNegative(Left, nameof(Left));
    public int Top { get; init; } = ThrowIfNegative(Top, nameof(Top));
    public int Right { get; init; } = ThrowIfNegative(Right, nameof(Right));
    public int Bottom { get; init; } = ThrowIfNegative(Bottom, nameof(Bottom));

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;

    private static int ThrowIfNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Padding cannot be negative.");

        return value;
    }
}

public readonly record struct LayoutLength
{
    private LayoutLength(LayoutLengthKind kind, int value, int minimum, int maximum)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Layout length cannot be negative."
            );

        if (minimum < 0)
            throw new ArgumentOutOfRangeException(
                nameof(minimum),
                "Layout minimum cannot be negative."
            );

        if (maximum < minimum)
            throw new ArgumentOutOfRangeException(
                nameof(maximum),
                "Layout maximum cannot be less than the minimum."
            );

        Kind = kind;
        Value = value;
        Minimum = minimum;
        Maximum = maximum;
    }

    public LayoutLengthKind Kind { get; }
    public int Value { get; }
    public int Minimum { get; }
    public int Maximum { get; }

    public static LayoutLength Fit(int minimum = 0, int maximum = int.MaxValue) =>
        new(LayoutLengthKind.Fit, 0, minimum, maximum);

    public static LayoutLength Fixed(int value) => new(LayoutLengthKind.Fixed, value, value, value);

    public static LayoutLength Grow(int minimum = 0, int maximum = int.MaxValue) =>
        new(LayoutLengthKind.Grow, 0, minimum, maximum);
}

public sealed record LayoutStyle
{
    private int _childGap;

    public static LayoutStyle Default { get; } = new();

    public LayoutLength Width { get; init; } = LayoutLength.Grow();
    public LayoutLength Height { get; init; } = LayoutLength.Grow();
    public Padding Padding { get; init; } = Padding.Zero;

    public int ChildGap
    {
        get => _childGap;
        init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Child gap cannot be negative."
                );

            _childGap = value;
        }
    }
}

public sealed class Node
{
    public Direction Direction { get; init; }
    public LayoutStyle Style { get; init; } = LayoutStyle.Default;
    public List<Node> Children { get; } = [];
    public IWidget? Widget { get; init; } // null for pure containers
}

public interface IWidget
{
    void Render(Rect bounds, ICanvas canvas);
}

public interface IMeasurableWidget
{
    Dimensions Measure();
}

public sealed record LayoutNode(Rect Bounds, IWidget? Widget, IReadOnlyList<LayoutNode> Children);
