// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace Asphalt;

public enum LayoutLengthKind
{
    Fit,
    Fixed,
    Grow,
}

public readonly record struct LayoutLength
{
    private LayoutLength(LayoutLengthKind kind, int value, int minimum, int maximum)
    {
        Debug.Assert(value >= 0, "Layout length cannot be negative.");
        Debug.Assert(minimum >= 0, "Layout minimum cannot be negative.");
        Debug.Assert(maximum >= minimum, "Layout maximum cannot be less than the minimum.");

        Value = value;
        Minimum = minimum;
        Maximum = maximum;
        Kind = kind;
    }

    public int Value { get; }
    public int Minimum { get; }
    public int Maximum { get; }
    public LayoutLengthKind Kind { get; }

    public static LayoutLength Fit(int minimum = 0, int maximum = int.MaxValue) =>
        new(LayoutLengthKind.Fit, 0, minimum, maximum);

    public static LayoutLength Fixed(int value) => new(LayoutLengthKind.Fixed, value, value, value);

    public static LayoutLength Grow(int minimum = 0, int maximum = int.MaxValue) =>
        new(LayoutLengthKind.Grow, 0, minimum, maximum);
}
