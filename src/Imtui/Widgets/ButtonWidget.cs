// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;
using Imtui.Rendering;

public static class ButtonWidget
{
    extension(ImtuiContext context)
    {
        public bool Button(
            string text,
            LayoutStyle? style = null,
            [CallerArgumentExpression(nameof(text))] string? textExpression = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(text);

            string id = $"{filePath}:{lineNumber}:{memberName}:{textExpression}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            context.OpenElement(new ButtonWidgetImplementation(text, inputState.Focused), style);
            context.CloseElement();

            bool pressedThisFrame = inputState.ConsumeKeys(static key =>
                key.Key == ConsoleKey.Enter
            );

            return pressedThisFrame;
        }
    }

    private sealed record ButtonWidgetImplementation(string Text, bool Focused) : IWidget
    {
        public WidgetLayout Measure()
        {
            // Width+2 for the brackets around the text
            Dimensions dimensions = new(Width: Text.Length + 2, Height: 1);
            return new WidgetLayout(Minimum: dimensions, Preferred: dimensions);
        }

        public void Render(Rect bounds, ICanvas canvas)
        {
            if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            {
                return;
            }

            TextStyle style = Focused ? TextStyle.Reverse : TextStyle.None;
            int width = Math.Min(bounds.Dimensions.Width, Text.Length + 2);

            for (int x = 0; x < width; x += 1)
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
}
