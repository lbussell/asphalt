// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;

public static class ButtonExtensions
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

            context.OpenElement(new ButtonWidget(text, inputState.Focused), style);
            context.CloseElement();

            bool pressedThisFrame = inputState.ConsumeKeys(static key =>
                key.Key == ConsoleKey.Enter
            );

            return pressedThisFrame;
        }
    }
}
