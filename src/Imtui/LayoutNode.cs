// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed record LayoutNode
{
    public Dimensions Dimensions { get; set; }
    public Position Position { get; set; }
    public IWidget? Widget { get; init; } = null;
    public Direction Direction { get; init; } = Direction.Vertical;
    public Padding Padding { get; init; }
    public int Gap { get; init; }
    public LayoutLength WidthLayout { get; init; } = LayoutLength.Fit();
    public LayoutLength HeightLayout { get; init; } = LayoutLength.Fit();
    public List<LayoutNode> Children { get; } = [];
}
