// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Numerics;
using System.Runtime.CompilerServices;

public static class SliderExtensions
{
    extension(ImtuiContext context)
    {
        // Horizontal slider for any numeric type that implements INumber<T>
        // (int, long, float, double, decimal, …). The current value lives in
        // the caller's variable; the widget mutates it in response to arrow
        // keys when focused. Returns true on frames where the value changed.
        //
        //   Left/Down arrow  → value -= step (clamped at min)
        //   Right/Up arrow   → value += step (clamped at max)
        //   Home / End       → jump to min / max
        //
        // `step` defaults to T.One. For floating-point sliders with small
        // ranges (e.g. 0..1) supply a smaller step explicitly.
        public bool Slider<T>(
            ref T value,
            T min,
            T max,
            T step = default,
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
                    $"Slider minimum ({min}) must be less than or equal to maximum ({max}).",
                    nameof(min)
                );

            T effectiveStep = step == default ? T.One : step;
            if (effectiveStep <= T.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(step),
                    effectiveStep,
                    "Slider step must be positive."
                );

            string id = $"{filePath}:{lineNumber}:{memberName}:{valueExpression}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            T originalValue = Clamp(value, min, max);
            T newValue = originalValue;

            if (inputState.Focused)
                newValue = ApplyKeys(context, newValue, min, max, effectiveStep);

            value = newValue;

            double normalized = ComputeNormalized(newValue, min, max);
            context.OpenElement(new SliderWidget(normalized, inputState.Focused), style);
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

    private static double ComputeNormalized<T>(T value, T min, T max)
        where T : struct, INumber<T>
    {
        if (max == min)
            return 0.0;

        double numerator = double.CreateChecked(value - min);
        double denominator = double.CreateChecked(max - min);
        return numerator / denominator;
    }
}
