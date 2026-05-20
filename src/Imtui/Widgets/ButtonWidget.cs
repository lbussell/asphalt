// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;
using Imtui.Rendering;

/// <summary>
/// A button. Pressing <see cref="ConsoleKey.Enter"/> while focused activates the button.
/// </summary>
public static class ButtonWidget
{
    extension(ImtuiContext context)
    {
        /// <summary>
        /// Declares a button for this frame and reports whether it was pressed.
        /// </summary>
        /// <param name="text">Label drawn between the brackets.</param>
        /// <param name="style">Optional layout overrides (width, height, margin, ...)</param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple buttons that share a call site. Only use
        /// this when rendering multiple buttons in a loop from the same call site. The value
        /// should be unique among all buttons sharing the same call site.
        /// </param>
        /// <param name="textExpression">Compiler-supplied; do not pass.</param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        /// <returns>
        /// <c>true</c> on the single frame in which Enter was pressed while the button was
        /// focused; otherwise <c>false</c>.
        /// </returns>
        public bool Button(
            string text,
            LayoutStyle? style = null,
            string uniqueKey = "",
            [CallerArgumentExpression(nameof(text))] string? textExpression = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            text ??= "NULL";

            string id = $"{filePath}:{lineNumber}:{textExpression}:{uniqueKey}";
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
            // Width+2 for the brackets around the text.
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
