// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public static class TextWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public void Text(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            TextWidget widget = new(content);
            context.Submit(widget);
        }
    }
}

public readonly record struct TextWidget(string Text) : IWidget
{
    public void Execute(ImtuiContext context)
    {
        context.WriteText(context.AllocateWidgetPosition(), Text, WidgetStyles.Normal);
    }
}
