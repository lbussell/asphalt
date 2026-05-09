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
    public static ImtuiContext.WidgetScope BorderPanel(
        this ImtuiContext context,
        BorderStyle? borderStyle = null,
        LayoutStyle? style = null,
        Direction direction = Direction.Vertical
    )
    {
        return context.PushNode(
            direction,
            new BorderPanel(borderStyle ?? BorderStyle.Round),
            AddBorderPadding(style ?? LayoutStyle.Default)
        );
    }

    private static LayoutStyle AddBorderPadding(LayoutStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);

        Padding padding = style.Padding;
        return style with
        {
            Padding = new Padding(
                padding.Left + 1,
                padding.Top + 1,
                padding.Right + 1,
                padding.Bottom + 1
            ),
        };
    }
}

public sealed class Text(string value) : IWidget, IMeasurableWidget
{
    private readonly string _value = value ?? throw new ArgumentNullException(nameof(value));

    public Dimensions Measure()
    {
        int width = 0;
        int height = 1;
        int lineWidth = 0;

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == '\n')
            {
                width = Math.Max(width, lineWidth);
                lineWidth = 0;
                height++;
                continue;
            }

            lineWidth++;
        }

        return new Dimensions(Math.Max(width, lineWidth), height);
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        int x = 0;
        int y = 0;

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == '\n')
            {
                x = 0;
                y++;
                if (y >= bounds.Dimensions.Height)
                    return;

                continue;
            }

            if (x < bounds.Dimensions.Width)
                canvas.Draw(new Position(bounds.Position.X + x, bounds.Position.Y + y), character);

            x++;
        }
    }
}

public static class TextWidgetExtensions
{
    private static LayoutStyle DefaultTextStyle { get; } =
        new() { Width = LayoutLength.Fit(), Height = LayoutLength.Fit() };

    public static void Text(this ImtuiContext context, string value, LayoutStyle? style = null)
    {
        context.AddWidget(new Text(value), style ?? DefaultTextStyle);
    }
}
