// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui;

/// <summary>
/// Specifies inset amounts for the four sides of a layout region.
/// </summary>
public readonly record struct Padding(int Left, int Right, int Top, int Bottom)
{
    /// <summary>
    /// Creates padding with the same value applied to all four sides.
    /// </summary>
    public static Padding Uniform(int amount) => new(amount, amount, amount, amount);

    /// <summary>
    /// Creates padding with one value applied to the left and right sides and
    /// another applied to the top and bottom.
    /// </summary>
    public static Padding Symmetric(int horizontal, int vertical) =>
        new(horizontal, horizontal, vertical, vertical);
}
