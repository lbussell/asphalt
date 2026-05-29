// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

/// <summary>
/// Describes how an element sizes itself, spaces its children, and arranges
/// them on its main axis. Syntax helpers for constructing and adjusting
/// instances live in <see cref="LayoutExtensions"/>.
/// </summary>
public readonly record struct Layout
{
    public LayoutLength Width { get; init; }
    public LayoutLength Height { get; init; }
    public Padding Padding { get; init; }
    public int ChildGap { get; init; }
    public Direction Direction { get; init; }
}

/// <summary>
/// Syntax helpers for <see cref="Layout"/>: static factories for common
/// presets (<c>Grow</c>, <c>Fit</c>, <c>Fixed</c>, <c>Sized</c>) and
/// <c>With*</c> modifiers for layering adjustments onto an existing style.
/// </summary>
/// <example>
/// <code>
/// // grow to fill, with 2-cell padding and a 1-cell gap between children
/// Layout.Grow.WithPadding(2).WithGap(1);
///
/// // fixed 80x24 box arranged horizontally
/// Layout.Fixed(80, 24).WithDirection(Direction.Horizontal);
/// </code>
/// </example>
public static class LayoutExtensions
{
    private static readonly Layout s_grow = new()
    {
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Grow(),
    };

    extension(Layout)
    {
        /// <summary>The default style: fit to content, no padding, no gap, vertical.</summary>
        public static Layout Default => default;

        /// <summary>A style that grows to fill the available space on both axes.</summary>
        public static Layout Grow => s_grow;

        /// <summary>A style that fits its content on both axes (same as <c>Default</c>).</summary>
        public static Layout Fit => default;

        /// <summary>A style with a fixed width and height.</summary>
        public static Layout Fixed(int width, int height) =>
            new() { Width = LayoutLength.Fixed(width), Height = LayoutLength.Fixed(height) };

        /// <summary>A style with the given width and height.</summary>
        public static Layout Sized(LayoutLength width, LayoutLength height) =>
            new() { Width = width, Height = height };
    }

    extension(Layout style)
    {
        /// <summary>Returns a copy with the given width.</summary>
        public Layout WithWidth(LayoutLength width) => style with { Width = width };

        /// <summary>Returns a copy with the given height.</summary>
        public Layout WithHeight(LayoutLength height) => style with { Height = height };

        /// <summary>Returns a copy with the given padding.</summary>
        public Layout WithPadding(Padding padding) => style with { Padding = padding };

        /// <summary>Returns a copy with the given inter-child gap (in cells).</summary>
        public Layout WithGap(int gap) => style with { ChildGap = gap };

        /// <summary>Returns a copy with the given child layout direction.</summary>
        public Layout WithDirection(Direction direction) => style with { Direction = direction };
    }
}
