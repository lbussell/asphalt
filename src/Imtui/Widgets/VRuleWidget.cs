// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class VRuleWidget(TerminalColor color = default) : IWidget
{
    private const char RuleCharacter = '│';

    public TerminalColor Color { get; } = color;

    public WidgetLayout Measure() => new(new Dimensions(1, 1), new Dimensions(1, 1));

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        for (int y = 0; y < bounds.Dimensions.Height; y++)
        {
            canvas.Draw(
                new Position(bounds.Position.X, bounds.Position.Y + y),
                RuleCharacter,
                Color
            );
        }
    }
}
