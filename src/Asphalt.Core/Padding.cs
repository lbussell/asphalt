// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

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

    /// <summary>
    /// Shorthand for <see cref="Padding(int)"/> so callers can write
    /// <c>Padding = 2</c> instead of <c>Padding = new Padding(2)</c>.
    /// </summary>
    public static implicit operator Padding(int all) => new(all);

    private static int ThrowIfNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Padding cannot be negative.");

        return value;
    }
}

public static class PaddingExtensions
{
    extension(Padding padding)
    {
        public int TotalHorizontal => padding.Left + padding.Right;
        public int TotalVertical => padding.Top + padding.Bottom;
    }
}
