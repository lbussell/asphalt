// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class ButtonWidget(
    string text,
    bool focused,
    TerminalColor backgroundColor = default,
    TerminalColor focusedBackgroundColor = default
) : IWidget
{
    private static readonly TerminalColor s_backgroundColor = TerminalColor.Rgb(0x3F, 0x3F, 0x48);
    private static readonly TerminalColor s_focusedBackgroundColor = TerminalColor.Rgb(
        0x29,
        0x4A,
        0x7A
    );

    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));
    public bool Focused { get; } = focused;
    public TerminalColor BackgroundColor { get; } =
        backgroundColor == default ? s_backgroundColor : backgroundColor;
    public TerminalColor FocusedBackgroundColor { get; } =
        focusedBackgroundColor == default ? s_focusedBackgroundColor : focusedBackgroundColor;
    private TerminalColor CurrentBackgroundColor =>
        Focused ? FocusedBackgroundColor : BackgroundColor;

    public WidgetLayout Measure()
    {
        Dimensions dimensions = new(Text.Length + 2, 1);
        return new WidgetLayout(dimensions, dimensions);
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        FillBackground(bounds, canvas);
        DrawText(bounds, canvas);
    }

    private void FillBackground(Rect bounds, ICanvas canvas)
    {
        for (int y = 0; y < bounds.Dimensions.Height; y++)
        {
            for (int x = 0; x < bounds.Dimensions.Width; x++)
            {
                canvas.Draw(
                    new Position(bounds.Position.X + x, bounds.Position.Y + y),
                    ' ',
                    backgroundColor: CurrentBackgroundColor
                );
            }
        }
    }

    private void DrawText(Rect bounds, ICanvas canvas)
    {
        int width = Math.Min(bounds.Dimensions.Width, Text.Length + 2);

        for (int x = 0; x < width; x++)
        {
            char character = x == 0 || x == Text.Length + 1 ? ' ' : Text[x - 1];
            canvas.Draw(
                new Position(bounds.Position.X + x, bounds.Position.Y),
                character,
                backgroundColor: CurrentBackgroundColor
            );
        }
    }
}
