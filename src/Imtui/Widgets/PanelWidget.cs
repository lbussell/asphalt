// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class PanelWidget(
    BorderStyle borderStyle,
    string? title = "",
    Padding innerPadding = default,
    TerminalColor foregroundColor = default
) : IWidget
{
    private const int TitleOffset = 2;

    public BorderStyle BorderStyle { get; } = borderStyle;
    public string Title { get; } = title ?? string.Empty;
    public Padding InnerPadding { get; } = innerPadding;
    public TerminalColor ForegroundColor { get; } = foregroundColor;

    public void Render(Rect bounds, ICanvas canvas)
    {
        Rect border = GetBorderBounds(bounds);
        int width = border.Dimensions.Width;
        int height = border.Dimensions.Height;

        if (width <= 0 || height <= 0)
            return;

        DrawHorizontalBorder(border.Position.X, border.Position.Y, width, top: true, canvas);
        DrawTitle(border, canvas);

        if (height > 1)
            DrawHorizontalBorder(
                border.Position.X,
                border.Position.Y + height - 1,
                width,
                top: false,
                canvas
            );

        for (int y = 1; y < height - 1; y++)
        {
            Draw(border.Position.X, border.Position.Y + y, BorderStyle.Vertical, canvas);

            if (width > 1)
                Draw(
                    border.Position.X + width - 1,
                    border.Position.Y + y,
                    BorderStyle.Vertical,
                    canvas
                );
        }
    }

    private Rect GetBorderBounds(Rect contentBounds) =>
        new(
            contentBounds.Position.X - InnerPadding.Left - 1,
            contentBounds.Position.Y - InnerPadding.Top - 1,
            contentBounds.Dimensions.Width + InnerPadding.TotalHorizontal + 2,
            contentBounds.Dimensions.Height + InnerPadding.TotalVertical + 2
        );

    private void DrawTitle(Rect border, ICanvas canvas)
    {
        if (Title.Length == 0)
            return;

        int x = border.Position.X + TitleOffset;
        int width = Math.Min(Title.Length, Math.Max(0, border.Dimensions.Width - TitleOffset - 1));

        for (int offset = 0; offset < width; offset++)
            Draw(x + offset, border.Position.Y, Title[offset], canvas);
    }

    private void DrawHorizontalBorder(int x, int y, int width, bool top, ICanvas canvas)
    {
        if (width == 1)
        {
            Draw(x, y, top ? BorderStyle.TopLeft : BorderStyle.BottomLeft, canvas);
            return;
        }

        Draw(x, y, top ? BorderStyle.TopLeft : BorderStyle.BottomLeft, canvas);

        for (int offset = 1; offset < width - 1; offset++)
            Draw(x + offset, y, BorderStyle.Horizontal, canvas);

        Draw(x + width - 1, y, top ? BorderStyle.TopRight : BorderStyle.BottomRight, canvas);
    }

    private void Draw(int x, int y, char character, ICanvas canvas) =>
        canvas.Draw(new Position(x, y), character, ForegroundColor);
}
