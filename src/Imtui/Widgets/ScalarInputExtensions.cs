// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Imtui.Rendering;

public static class ScalarInputExtensions
{
    extension(ImtuiContext context)
    {
        // Compact, read-only counterpart to Slider. Renders the current value
        // as formatted text inside a textbox-style cell and adjusts the value
        // using the same key bindings as Slider when focused. Returns true on
        // frames where the value changed.
        //
        //   Left/Down arrow  → value -= step (clamped at min)
        //   Right/Up arrow   → value += step (clamped at max)
        //   Home / End       → jump to min / max
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
            TerminalColor backgroundColor = default,
            TerminalColor focusedBackgroundColor = default,
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
                newValue = ApplyKeys(context, newValue, min, max, effectiveStep);

            value = newValue;

            string displayText = Format(newValue, format);
            int? preferredWidth = width ?? ComputeAutoWidth(min, max, format);

            context.OpenElement(
                new ScalarInputWidget(
                    displayText,
                    inputState.Focused,
                    preferredWidth,
                    backgroundColor,
                    focusedBackgroundColor
                ),
                style
            );
            context.CloseElement();

            return newValue != originalValue;
        }
    }

    private static T ApplyKeys<T>(ImtuiContext context, T value, T min, T max, T step)
        where T : struct, INumber<T>
    {
        while (context.TryConsumeKey(out ConsoleKeyInfo key))
        {
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.DownArrow:
                    value = value - step < min ? min : value - step;
                    break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.UpArrow:
                    value = value + step > max ? max : value + step;
                    break;

                case ConsoleKey.Home:
                    value = min;
                    break;

                case ConsoleKey.End:
                    value = max;
                    break;
            }
        }

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
}
