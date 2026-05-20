// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Imtui.Rendering;

public static class ScalarInputWidget
{
    extension(ImtuiContext context)
    {
        // Compact, read-only counterpart to Slider. Renders the current value
        // as formatted text inside a textbox-style cell and adjusts the value
        // using the same key bindings as Slider when focused. Returns true on
        // frames where the value changed.
        //
        //   Left/Down arrow  -> value -= step (clamped at min)
        //   Right/Up arrow   -> value += step (clamped at max)
        //   Home / End       -> jump to min / max
        //
        // Typing characters is intentionally not supported.
        //
        // `format` is an optional standard or custom numeric format string
        // (e.g. "0.00", "X4"). When null, T.ToString() is used.
        // `width` lets callers force a fixed cell width; when null, the widget
        // auto-sizes to fit the wider of the formatted min/max values plus one
        // cell of horizontal padding on each side.
        public bool ScalarInput<T>(
            ref T value,
            T min,
            T max,
            T step = default,
            string? format = null,
            int? width = null,
            LayoutStyle? style = null,
            [CallerArgumentExpression(nameof(value))] string? valueExpression = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
            where T : struct, INumber<T>
        {
            if (min > max)
                throw new ArgumentException(
                    $"ScalarInput minimum ({min}) must be less than or equal to maximum ({max}).",
                    nameof(min)
                );

            T effectiveStep = step == default ? T.One : step;
            if (effectiveStep <= T.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(step),
                    effectiveStep,
                    "ScalarInput step must be positive."
                );

            if (width is { } explicitWidth && explicitWidth <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    explicitWidth,
                    "ScalarInput width must be positive when specified."
                );

            string id = $"{filePath}:{lineNumber}:{memberName}:{valueExpression}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            T originalValue = Clamp(value, min, max);
            T newValue = originalValue;

            if (inputState.Focused)
                newValue = ApplyKeys(inputState, newValue, min, max, effectiveStep);

            value = newValue;

            string displayText = Format(newValue, format);
            int? preferredWidth = width ?? ComputeAutoWidth(min, max, format);

            Theme theme = context.Theme;

            context.OpenElement(
                new Implementation(
                    displayText,
                    inputState.Focused,
                    preferredWidth,
                    theme.Surface,
                    theme.SurfaceFocused
                ),
                style
            );
            context.CloseElement();

            return newValue != originalValue;
        }
    }

    private static T ApplyKeys<T>(WidgetInputState inputState, T value, T min, T max, T step)
        where T : struct, INumber<T>
    {
        inputState.ConsumeKeys(key =>
        {
            switch (key.KeyChar)
            {
                case '-':
                    value = value - step < min ? min : value - step;
                    return true;

                case '=':
                case '+':
                    value = value + step > max ? max : value + step;
                    return true;
            }

            switch (key.Key)
            {
                case ConsoleKey.Home:
                    value = min;
                    return true;

                case ConsoleKey.End:
                    value = max;
                    return true;

                default:
                    return false;
            }
        });

        return value;
    }

    private static T Clamp<T>(T value, T min, T max)
        where T : struct, INumber<T> =>
        value < min ? min
        : value > max ? max
        : value;

    private static string Format<T>(T value, string? format)
        where T : struct, INumber<T> =>
        format is null
            ? value.ToString() ?? string.Empty
            : value.ToString(format, CultureInfo.CurrentCulture);

    private static int ComputeAutoWidth<T>(T min, T max, string? format)
        where T : struct, INumber<T>
    {
        int minLength = Format(min, format).Length;
        int maxLength = Format(max, format).Length;
        int widest = minLength > maxLength ? minLength : maxLength;
        return widest + 2;
    }

    // Renders the formatted display string for a scalar value inside a
    // single-row textbox cell. Intentionally non-generic and read-only.
    internal sealed class Implementation(
        string displayText,
        bool focused,
        int? preferredWidth = null,
        TerminalColor backgroundColor = default,
        TerminalColor focusedBackgroundColor = default
    ) : IWidget
    {
        private const int HorizontalPadding = 1;

        public string DisplayText { get; } =
            displayText ?? throw new ArgumentNullException(nameof(displayText));
        public bool Focused { get; } = focused;
        public TerminalColor BackgroundColor { get; } = backgroundColor;
        public TerminalColor FocusedBackgroundColor { get; } = focusedBackgroundColor;

        private TerminalColor CurrentBackgroundColor =>
            Focused ? FocusedBackgroundColor : BackgroundColor;

        public WidgetLayout Measure()
        {
            int autoWidth = DisplayText.Length + (HorizontalPadding * 2);
            int width = preferredWidth ?? autoWidth;
            int minimumWidth = HorizontalPadding * 2 + 1;
            if (width < minimumWidth)
                width = minimumWidth;
            return new WidgetLayout(new Dimensions(minimumWidth, 1), new Dimensions(width, 1));
        }

        public void Render(Rect bounds, ICanvas canvas)
        {
            if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
                return;

            FillBackground(bounds, canvas);

            int width = bounds.Dimensions.Width;
            int textStartColumn = HorizontalPadding;
            int maxTextColumns = width - (HorizontalPadding * 2);
            if (maxTextColumns <= 0)
                return;

            int charactersToDraw = Math.Min(DisplayText.Length, maxTextColumns);
            for (int index = 0; index < charactersToDraw; index++)
            {
                canvas.Draw(
                    new Position(bounds.Position.X + textStartColumn + index, bounds.Position.Y),
                    DisplayText[index],
                    foregroundColor: default,
                    backgroundColor: CurrentBackgroundColor
                );
            }
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
