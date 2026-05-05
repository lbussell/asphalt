// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

/// <summary>
/// Immediate-mode widget APIs for <see cref="ImtuiContext"/>.
/// </summary>
public static class WidgetExtensions
{
    extension(ImtuiContext context)
    {
        /// <summary>
        /// Writes non-interactive text at the next widget position.
        /// </summary>
        public void Text(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            context.Submit(new TextWidget(text));
        }

        /// <summary>
        /// Draws a focusable button and returns <see langword="true"/> when it is activated.
        /// </summary>
        public bool Button(string label)
        {
            ArgumentNullException.ThrowIfNull(label);

            return context.Submit(new ButtonWidget(label));
        }

        /// <summary>
        /// Draws a focusable checkbox, mutates <paramref name="value"/>, and returns whether it changed.
        /// </summary>
        public bool Checkbox(string label, ref bool value)
        {
            ArgumentNullException.ThrowIfNull(label);

            CheckboxResult result = context.Submit(new CheckboxWidget(label, value));
            value = result.Value;
            return result.Changed;
        }

        /// <summary>
        /// Draws a focusable text field, mutates <paramref name="value"/>, and returns whether it changed.
        /// </summary>
        public bool TextField(string label, ref string value)
        {
            ArgumentNullException.ThrowIfNull(label);
            ArgumentNullException.ThrowIfNull(value);

            TextFieldResult result = context.Submit(new TextFieldWidget(label, value));
            value = result.Value;
            return result.Changed;
        }
    }
}
