// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class ShadowBoxWidget(Padding padding = default, TerminalColor shadowColor = default)
    : IWidget
{
    private const char FullBlock = '█';
    private const char LowerHalfBlock = '▄';
    private const char UpperHalfBlock = '▀';
    private static readonly TerminalColor s_shadowColor = TerminalColor.Rgb(0, 0, 0);

    public Padding Padding { get; } = padding;
    public TerminalColor ShadowColor { get; } =
        shadowColor == default ? s_shadowColor : shadowColor;

    public void Render(Rect bounds, ICanvas canvas)
    {
        Rect box = GetBoxBounds(bounds);

        if (box.Dimensions.Width <= 0 || box.Dimensions.Height <= 0)
            return;

        int shadowX = box.Position.X + box.Dimensions.Width;
        int shadowY = box.Position.Y + box.Dimensions.Height;

        Draw(shadowX, box.Position.Y, LowerHalfBlock, canvas);

        for (int y = 1; y < box.Dimensions.Height; y++)
            Draw(shadowX, box.Position.Y + y, FullBlock, canvas);

        for (int x = 1; x <= box.Dimensions.Width; x++)
            Draw(box.Position.X + x, shadowY, UpperHalfBlock, canvas);
    }

    private Rect GetBoxBounds(Rect contentBounds) =>
        new(
            contentBounds.Position.X - Padding.Left,
            contentBounds.Position.Y - Padding.Top,
            contentBounds.Dimensions.Width + Padding.TotalHorizontal,
            contentBounds.Dimensions.Height + Padding.TotalVertical
        );

    private void Draw(int x, int y, char character, ICanvas canvas) =>
        canvas.Draw(new Position(x, y), character, ShadowColor);
}
