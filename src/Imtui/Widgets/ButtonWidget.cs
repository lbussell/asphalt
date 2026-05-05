// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui.Widgets;

public static class ButtonWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public bool Button(string label)
        {
            ArgumentNullException.ThrowIfNull(label);
            return context.Submit(new ButtonWidget(label));
        }
    }
}

public readonly record struct ButtonWidget(string Label) : IWidget<bool>
{
    public bool Execute(ImtuiContext context)
    {
        WidgetID id = context.GetId(Label);
        bool focused = context.RegisterFocusable(id);

        context.WriteText(
            context.AllocateWidgetPosition(),
            $"[{Label}]",
            WidgetStyles.ForFocus(focused)
        );

        return context.IsActivated(id);
    }
}
