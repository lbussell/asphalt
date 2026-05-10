// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class PanelWidget(Padding padding = default, TerminalColor backgroundColor = default)
    : IWidget
{
    public Padding Padding { get; } = padding;
    public TerminalColor BackgroundColor { get; } = backgroundColor;

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (BackgroundColor == default)
            return;

        Rect panel = GetPanelBounds(bounds);

        for (int y = 0; y < panel.Dimensions.Height; y++)
        {
            for (int x = 0; x < panel.Dimensions.Width; x++)
            {
                canvas.Draw(
                    new Position(panel.Position.X + x, panel.Position.Y + y),
                    ' ',
                    backgroundColor: BackgroundColor
                );
            }
        }
    }

    private Rect GetPanelBounds(Rect contentBounds) =>
        new(
            contentBounds.Position.X - Padding.Left,
            contentBounds.Position.Y - Padding.Top,
            contentBounds.Dimensions.Width + Padding.TotalHorizontal,
            contentBounds.Dimensions.Height + Padding.TotalVertical
        );
}
