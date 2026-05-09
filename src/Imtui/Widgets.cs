// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed class BorderPanel(BorderStyle borderStyle) : IWidget, IMeasurableWidget
{
    private readonly BorderStyle _borderStyle = borderStyle;

    public BorderPanel()
        : this(BorderStyle.Round) { }

    public Dimensions Measure() => new(1, 1);

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

        DrawHorizontalBorder(
            canvas,
            left,
            right,
            top,
            _borderStyle.TopLeft,
            _borderStyle.TopRight,
            _borderStyle.Horizontal
        );

        for (int y = top + 1; y < bottom; y++)
        {
            Draw(canvas, left, y, _borderStyle.Vertical);
            if (width > 1)
                Draw(canvas, right, y, _borderStyle.Vertical);
        }

        if (height > 1)
            DrawHorizontalBorder(
                canvas,
                left,
                right,
                bottom,
                _borderStyle.BottomLeft,
                _borderStyle.BottomRight,
                _borderStyle.Horizontal
            );
    }

    private static void DrawHorizontalBorder(
        ICanvas canvas,
        int left,
        int right,
        int y,
        char leftCorner,
        char rightCorner,
        char horizontal
    )
    {
        Draw(canvas, left, y, leftCorner);

        for (int x = left + 1; x < right; x++)
            Draw(canvas, x, y, horizontal);

        if (right > left)
            Draw(canvas, right, y, rightCorner);
    }

    private static void Draw(ICanvas canvas, int x, int y, char character) =>
        canvas.Draw(new Position(x, y), character);
}

public static class BorderPanelWidgetExtensions
{
    public static void BorderPanel(this ImtuiContext context) =>
        context.AddWidget(new BorderPanel());

    public static void BorderPanel(this ImtuiContext context, LayoutStyle style) =>
        context.AddWidget(new BorderPanel(), style);

    public static void BorderPanel(this ImtuiContext context, BorderStyle borderStyle) =>
        context.AddWidget(new BorderPanel(borderStyle));

    public static void BorderPanel(
        this ImtuiContext context,
        BorderStyle borderStyle,
        LayoutStyle style
    ) => context.AddWidget(new BorderPanel(borderStyle), style);
}
