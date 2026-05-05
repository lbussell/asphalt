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

            WidgetID id = context.GetId(label);
            ButtonWidget widget = new(id, label);

            bool pressed = context.Submit(widget);
            return pressed;
        }
    }
}

public readonly record struct ButtonWidget(WidgetID ID, string Label) : IStatefulWidget<bool>
{
    public bool IsFocusable => true;

    public bool Execute(ImtuiContext context)
    {
        bool focused = context.IsFocused(ID);

        context.WriteText(
            context.AllocateWidgetPosition(),
            $"[{Label}]",
            WidgetStyles.ForFocus(focused)
        );

        bool activated = context.IsActivated(ID);
        return activated;
    }
}
