// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

using System.Runtime.CompilerServices;
using Asphalt.Rendering;

public static class InputTextWidget
{
    extension(AsphaltContext context)
    {
        // Single-line text input. The current value lives in the caller's
        // variable and is mutated in place when the user types.
        //
        // Cursor position is persisted across frames via UseState, keyed off
        // the same id used to register focus, so multiple InputText call sites
        // each get their own independent cursor.
        public void InputText(
            ref string value,
            string? placeholder = null,
            Layout? style = null,
            string uniqueKey = "",
            [CallerArgumentExpression(nameof(value))] string? valueExpression = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(value);

            string id = $"{filePath}:{lineNumber}:{valueExpression}:{uniqueKey}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            int initialCursor = value.Length;
            State<int> cursor = context.UseState(id + ":cursor", () => initialCursor);

            // Clamp cursor in case the caller mutated the string between frames.
            if (cursor.Value > value.Length)
                cursor.Value = value.Length;
            else if (cursor.Value < 0)
                cursor.Value = 0;

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
                new Implementation(
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
        }
    }

    private static bool IsPrintable(char character) =>
        character != '\0' && !char.IsControl(character);

    internal sealed class Implementation(
        string value,
        int cursor,
        bool focused,
        string? placeholder = null,
        TerminalColor backgroundColor = default,
        TerminalColor focusedBackgroundColor = default,
        TerminalColor placeholderColor = default
    ) : IWidget
    {
        private const int DefaultPreferredWidth = 10;

        public string Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
        public int Cursor { get; } = cursor;
        public bool Focused { get; } = focused;
        public string? Placeholder { get; } = placeholder;
        public TerminalColor BackgroundColor { get; } = backgroundColor;
        public TerminalColor FocusedBackgroundColor { get; } = focusedBackgroundColor;
        public TerminalColor PlaceholderColor { get; } = placeholderColor;

        private TerminalColor CurrentBackgroundColor =>
            Focused ? FocusedBackgroundColor : BackgroundColor;

        public WidgetLayout Measure()
        {
            // Reserve one extra column so the cursor can sit just after the last
            // character without overflowing the widget. Use the placeholder width
            // as a hint when the value is empty so empty inputs do not collapse to
            // a single cell.
            int preferredWidth = Math.Max(
                DefaultPreferredWidth,
                Math.Max(Value.Length + 1, Placeholder?.Length ?? 0)
            );
            return new WidgetLayout(new Dimensions(1, 1), new Dimensions(preferredWidth, 1));
        }

        public void Render(Rect bounds, ICanvas canvas)
        {
            if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
                return;

            FillBackground(bounds, canvas);

            bool showPlaceholder = Value.Length == 0 && !Focused && Placeholder is { Length: > 0 };
            string displayText = showPlaceholder ? Placeholder! : Value;
            TerminalColor foregroundColor = showPlaceholder ? PlaceholderColor : default;

            int viewOffset = ComputeViewOffset(bounds.Dimensions.Width);

            int width = bounds.Dimensions.Width;
            for (int column = 0; column < width; column++)
            {
                int textIndex = column + viewOffset;
                char character = textIndex < displayText.Length ? displayText[textIndex] : ' ';
                canvas.Draw(
                    new Position(bounds.Position.X + column, bounds.Position.Y),
                    character,
                    foregroundColor,
                    CurrentBackgroundColor
                );
            }

            if (Focused)
                DrawCursor(bounds, viewOffset, canvas);
        }

        private int ComputeViewOffset(int width)
        {
            // When the text fits, no scrolling is needed. Otherwise, scroll so the
            // cursor is visible: keep the cursor inside [viewOffset, viewOffset+width-1].
            // Reserve the last column for the cursor itself so it remains visible
            // when positioned past the final character.
            if (Value.Length < width)
                return 0;

            int maxVisibleCursor = width - 1;
            return Cursor <= maxVisibleCursor ? 0 : Cursor - maxVisibleCursor;
        }

        private void DrawCursor(Rect bounds, int viewOffset, ICanvas canvas)
        {
            int cursorColumn = Cursor - viewOffset;
            if (cursorColumn < 0 || cursorColumn >= bounds.Dimensions.Width)
                return;

            char characterUnderCursor = Cursor < Value.Length ? Value[Cursor] : ' ';

            // Render the cursor as a reverse-video cell: background becomes the
            // text color and vice versa, which works on any terminal without
            // requiring a dedicated cursor-style capability on the canvas.
            canvas.Draw(
                new Position(bounds.Position.X + cursorColumn, bounds.Position.Y),
                characterUnderCursor,
                foregroundColor: CurrentBackgroundColor,
                backgroundColor: TerminalColor.White
            );
        }

        private void FillBackground(Rect bounds, ICanvas canvas)
        {
            for (int y = 0; y < bounds.Dimensions.Height; y++)
            {
                for (int x = 0; x < bounds.Dimensions.Width; x++)
                {
                    canvas.Draw(
                        new Position(bounds.Position.X + x, bounds.Position.Y + y),
                        ' ',
                        backgroundColor: CurrentBackgroundColor
                    );
                }
            }
        }
    }
}
