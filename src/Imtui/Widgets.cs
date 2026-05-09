// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed class ColorBlock() : IWidget
{
    public void Render(Rect bounds, ICanvas canvas) =>
        canvas.Fill(bounds, TerminalColorRgb.Random());
}

public sealed class BorderPanel() : IWidget
{
    private static readonly TerminalColorRgb s_foregroundColor = new(255, 255, 255);
    private static readonly TerminalColorRgb s_backgroundColor = new(0, 0, 0);

    public void Render(Rect bounds, ICanvas canvas)
    {
        int width = bounds.Dimensions.Width;
        int height = bounds.Dimensions.Height;

        if (width <= 0 || height <= 0)
            return;

        int left = bounds.Position.X;
        int top = bounds.Position.Y;
        int right = left + width - 1;
        int bottom = top + height - 1;

        DrawHorizontalBorder(canvas, left, right, top, '┌', '┐');

        for (int y = top + 1; y < bottom; y++)
        {
            Draw(canvas, left, y, '│');
            if (width > 1)
                Draw(canvas, right, y, '│');
        }

        if (height > 1)
            DrawHorizontalBorder(canvas, left, right, bottom, '└', '┘');
    }

    private static void DrawHorizontalBorder(
        ICanvas canvas,
        int left,
        int right,
        int y,
        char leftCorner,
        char rightCorner
    )
    {
        Draw(canvas, left, y, leftCorner);

        for (int x = left + 1; x < right; x++)
            Draw(canvas, x, y, '─');

        if (right > left)
            Draw(canvas, right, y, rightCorner);
    }

    private static void Draw(ICanvas canvas, int x, int y, char character) =>
        canvas.Draw(new Position(x, y), character, s_foregroundColor, s_backgroundColor);
}

public static class ColorBlockWidgetExtensions
{
    public static void ColorBlock(this ImtuiContext context) => context.AddWidget(new ColorBlock());
}

public static class BorderPanelWidgetExtensions
{
    public static void BorderPanel(this ImtuiContext context) =>
        context.AddWidget(new BorderPanel());
}
