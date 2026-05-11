// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;
using Imtui.Rendering;

public static class ButtonExtensions
{
    extension(ImtuiContext context)
    {
        public bool Button(
            string text,
            LayoutStyle? style = null,
            TerminalColor backgroundColor = default,
            TerminalColor focusedBackgroundColor = default,
            [CallerArgumentExpression(nameof(text))] string? textExpression = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(text);

            string id = $"{filePath}:{lineNumber}:{memberName}:{textExpression}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            context.OpenElement(
                new ButtonWidget(text, inputState.Focused, backgroundColor, focusedBackgroundColor),
                style
            );
            context.CloseElement();

            return inputState.Activated;
        }
    }
}
