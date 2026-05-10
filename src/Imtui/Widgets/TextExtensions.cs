// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public static class TextExtensions
{
    extension(ImtuiContext context)
    {
        public void Text(
            string text,
            LayoutStyle? style = null,
            TextWrappingMode wrappingMode = TextWrappingMode.Wrap,
            TerminalColor foregroundColor = default,
            TerminalColor backgroundColor = default
        )
        {
            context.OpenElement(
                new TextWidget(text, wrappingMode, foregroundColor, backgroundColor),
                style
            );
            context.CloseElement();
        }
    }
}
