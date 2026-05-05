// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public static class TextWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public bool Text(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            ButtonWidget widget = new(content);
            return context.Submit(widget);
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
