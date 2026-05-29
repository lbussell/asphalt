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
    // Stored as the distance below int.MaxValue rather than the absolute
    // maximum so that default(LayoutLength) — which has every field zeroed —
    // reports Maximum = int.MaxValue (i.e. "unbounded"). This makes
    // default(LayoutLength) equivalent to Fit() and lets enclosing types
    // (notably Layout) be cheap struct types whose `default` value is
    // already meaningful.
    private readonly int _maximumOffsetFromIntMax;

    private LayoutLength(LayoutLengthKind kind, int value, int minimum, int maximum)
    {
        Debug.Assert(value >= 0, "Layout length cannot be negative.");
        Debug.Assert(minimum >= 0, "Layout minimum cannot be negative.");
        Debug.Assert(maximum >= minimum, "Layout maximum cannot be less than the minimum.");

        Value = value;
        Minimum = minimum;
        _maximumOffsetFromIntMax = int.MaxValue - maximum;
        Kind = kind;
    }

    public int Value { get; }
    public int Minimum { get; }
    public int Maximum => int.MaxValue - _maximumOffsetFromIntMax;
    public LayoutLengthKind Kind { get; }

    public static LayoutLength Fit(int minimum = 0, int maximum = int.MaxValue) =>
        new(LayoutLengthKind.Fit, 0, minimum, maximum);

    public static LayoutLength Fixed(int value) => new(LayoutLengthKind.Fixed, value, value, value);

    public static LayoutLength Grow(int minimum = 0, int maximum = int.MaxValue) =>
        new(LayoutLengthKind.Grow, 0, minimum, maximum);

    /// <summary>
    /// Shorthand for <see cref="Fixed(int)"/> so callers can write
    /// <c>Width = 20</c> instead of <c>Width = LayoutLength.Fixed(20)</c>.
    /// </summary>
    public static implicit operator LayoutLength(int value) => Fixed(value);
}
