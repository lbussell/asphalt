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
            string newValue = value;
            if (inputState.Focused)
            {
                inputState.ConsumeKeys(key =>
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.Backspace:
                            if (cursor.Value > 0)
                            {
                                newValue = newValue.Remove(cursor.Value - 1, 1);
                                cursor.Value -= 1;
                            }
                            return true;

                        case ConsoleKey.Delete:
                            if (cursor.Value < newValue.Length)
                                newValue = newValue.Remove(cursor.Value, 1);
                            return true;

                        case ConsoleKey.LeftArrow:
                            if (cursor.Value > 0)
                                cursor.Value -= 1;
                            return true;

                        case ConsoleKey.RightArrow:
                            if (cursor.Value < newValue.Length)
                                cursor.Value += 1;
                            return true;

                        case ConsoleKey.Home:
                            cursor.Value = 0;
                            return true;

                        case ConsoleKey.End:
                            cursor.Value = newValue.Length;
                            return true;

                        default:
                            if (IsPrintable(key.KeyChar))
                            {
                                newValue = newValue.Insert(cursor.Value, key.KeyChar.ToString());
                                cursor.Value += 1;
                                return true;
                            }
                            return false;
                    }
                });
            }
            value = newValue;

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

    private static bool IsPrintable(char character) =>
        character != '\0' && !char.IsControl(character);
}
