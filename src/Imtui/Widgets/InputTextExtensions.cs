// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;

public static class InputTextExtensions
{
    extension(ImtuiContext context)
    {
        // Single-line text input. The current value lives in the caller's
        // variable and is mutated in place when the user types. Returns true on
        // frames where the value changed.
        //
        // Cursor position is persisted across frames via UseState, keyed off
        // the same id used to register focus, so multiple InputText call sites
        // each get their own independent cursor.
        public bool InputText(
            ref string value,
            string? placeholder = null,
            LayoutStyle? style = null,
            [CallerArgumentExpression(nameof(value))] string? valueExpression = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(value);

            string id = $"{filePath}:{lineNumber}:{memberName}:{valueExpression}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            int initialCursor = value.Length;
            State<int> cursor = context.UseState(id + ":cursor", () => initialCursor);

            // Clamp cursor in case the caller mutated the string between frames.
            if (cursor.Value > value.Length)
                cursor.Value = value.Length;
            else if (cursor.Value < 0)
                cursor.Value = 0;

            string originalValue = value;
            if (inputState.Focused)
            {
                value = ApplyKeys(context, value, cursor);
            }

            Theme theme = context.Theme;

            context.OpenElement(
                new InputTextWidget(
                    value,
                    cursor.Value,
                    inputState.Focused,
                    placeholder,
                    theme.Surface,
                    theme.SurfaceFocused,
                    theme.Placeholder
                ),
                style
            );
            context.CloseElement();

            return value != originalValue;
        }
    }

    // Drain unconsumed keys for this frame and apply standard single-line
    // editing semantics to (value, cursor). Returns the resulting string.
    private static string ApplyKeys(ImtuiContext context, string value, State<int> cursor)
    {
        while (context.TryConsumeKey(out ConsoleKeyInfo key))
        {
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (cursor.Value > 0)
                    {
                        value = value.Remove(cursor.Value - 1, 1);
                        cursor.Value -= 1;
                    }
                    break;

                case ConsoleKey.Delete:
                    if (cursor.Value < value.Length)
                        value = value.Remove(cursor.Value, 1);
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursor.Value > 0)
                        cursor.Value -= 1;
                    break;

                case ConsoleKey.RightArrow:
                    if (cursor.Value < value.Length)
                        cursor.Value += 1;
                    break;

                case ConsoleKey.Home:
                    cursor.Value = 0;
                    break;

                case ConsoleKey.End:
                    cursor.Value = value.Length;
                    break;

                default:
                    if (IsPrintable(key.KeyChar))
                    {
                        value = value.Insert(cursor.Value, key.KeyChar.ToString());
                        cursor.Value += 1;
                    }
                    break;
            }
        }

        return value;
    }

    private static bool IsPrintable(char character) =>
        character != '\0' && !char.IsControl(character);
}
