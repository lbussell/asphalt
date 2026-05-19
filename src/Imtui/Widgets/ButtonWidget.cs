// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class ButtonWidget(string text, bool focused) : IWidget
{
    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));
    public bool Focused { get; } = focused;

    public WidgetLayout Measure()
    {
        Dimensions dimensions = new(Text.Length + 2, 1);
        return new WidgetLayout(dimensions, dimensions);
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        TextStyle style = Focused ? TextStyle.Reverse : TextStyle.None;
        int width = Math.Min(bounds.Dimensions.Width, Text.Length + 2);

        for (int x = 0; x < width; x++)
        {
            char character = x switch
            {
                0 => '[',
                _ when x == Text.Length + 1 => ']',
                _ => Text[x - 1],
            };
            canvas.Draw(
                new Position(bounds.Position.X + x, bounds.Position.Y),
                character,
                style: style
            );
        }
    }
}
