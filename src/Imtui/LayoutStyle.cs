// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed record LayoutStyle
{
    public static LayoutStyle Default { get; } = new();
    public LayoutLength Width { get; init; } = LayoutLength.Fit();
    public LayoutLength Height { get; init; } = LayoutLength.Fit();
    public Padding Padding { get; init; } = Padding.Zero;
    public int ChildGap { get; init; } = 0;
    public Direction Direction { get; init; } = Direction.Vertical;
}

public static class LayoutStyleExtensions
{
    private static readonly LayoutStyle s_grow = new()
    {
        Height = LayoutLength.Grow(),
        Width = LayoutLength.Grow(),
    };

    extension(LayoutStyle)
    {
        public static LayoutStyle Grow => s_grow;
    }
}
