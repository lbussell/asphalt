// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public static class CheckboxWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public void Checkbox(string label, ref bool value)
        {
            ArgumentNullException.ThrowIfNull(label);
            CheckboxWidget widget = new(label, value);
            CheckboxResult result = context.Submit(widget);
            value = result.Value;
        }
    }
}

internal readonly record struct CheckboxResult(bool Value, bool Changed);

internal readonly record struct CheckboxWidget(string Label, bool Value) : IWidget<CheckboxResult>
{
    public CheckboxResult Execute(ImtuiContext context)
    {
        WidgetID id = context.GetId(Label);
        bool focused = context.RegisterFocusable(id);
        bool value = Value;
        bool changed = false;

        if (context.IsActivated(id))
        {
            value = !value;
            changed = true;
        }

        char marker = value ? 'x' : ' ';
        context.WriteText(
            context.AllocateWidgetPosition(),
            $"[{marker}] {Label}",
            WidgetStyles.ForFocus(focused)
        );

        return new CheckboxResult(value, changed);
    }
}
