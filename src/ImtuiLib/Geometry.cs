// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace ImtuiLib;

/// <summary>
/// Describes the size of a rendered terminal viewport.
/// </summary>
public readonly record struct ViewportSize
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportSize"/> struct.
    /// </summary>
    /// <param name="width">The viewport width, in terminal cells.</param>
    /// <param name="height">The viewport height, in terminal cells.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="width"/> or <paramref name="height"/> is negative.
    /// </exception>
    public ViewportSize(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the viewport width, in terminal cells.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the viewport height, in terminal cells.
    /// </summary>
    public int Height { get; }
}

/// <summary>
/// Describes a position in a terminal cell grid.
/// </summary>
public readonly record struct CellPosition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CellPosition"/> struct.
    /// </summary>
    /// <param name="x">The horizontal cell coordinate.</param>
    /// <param name="y">The vertical cell coordinate.</param>
    public CellPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the horizontal cell coordinate.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the vertical cell coordinate.
    /// </summary>
    public int Y { get; }
}
