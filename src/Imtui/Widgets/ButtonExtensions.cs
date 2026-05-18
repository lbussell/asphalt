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

            Theme theme = context.Theme;

            context.OpenElement(
                new ButtonWidget(text, inputState.Focused, theme.Surface, theme.SurfaceFocused),
                style
            );
            context.CloseElement();

            return inputState.Activated;
        }
    }
}
