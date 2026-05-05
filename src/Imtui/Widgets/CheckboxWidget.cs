// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui.Widgets;

public static class CheckboxWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public void Checkbox(string label, ref bool value)
        {
            ArgumentNullException.ThrowIfNull(label);

            WidgetID id = context.GetId(label);
            CheckboxWidget widget = new(id, label, value);
            CheckboxResult result = context.Submit(widget);

            value = result.Value;
        }
    }
}

internal readonly record struct CheckboxResult(bool Value, bool Changed);

internal readonly record struct CheckboxWidget(WidgetID ID, string Label, bool Value)
    : IStatefulWidget<CheckboxResult>
{
    public bool IsFocusable => true;

    public CheckboxResult Execute(ImtuiContext context)
    {
        bool focused = context.IsFocused(ID);
        bool value = Value;
        bool changed = false;

        if (context.IsActivated(ID))
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

        CheckboxResult result = new(value, changed);
        return result;
    }
}
