// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

using System.Numerics;
using System.Runtime.CompilerServices;
using Asphalt.Rendering;

public static class SliderWidget
{
    extension(AsphaltContext context)
    {
        // Horizontal slider for any numeric type that implements INumber<T>
        // (int, long, float, double, decimal, ...). The current value lives in
        // the caller's variable; the widget mutates it in response to arrow
        // keys when focused.
        //
        //   Left/Down arrow  -> value -= step (clamped at min)
        //   Right/Up arrow   -> value += step (clamped at max)
        //   Home / End       -> jump to min / max
        //
        // `step` defaults to T.One. For floating-point sliders with small
        // ranges (e.g. 0..1) supply a smaller step explicitly.
        public void Slider<T>(
            ref T value,
            T min,
            T max,
            T step = default,
            Layout? style = null,
            string uniqueKey = "",
            [CallerArgumentExpression(nameof(value))] string? valueExpression = null,
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

            string id = $"{filePath}:{lineNumber}:{valueExpression}:{uniqueKey}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            T clamped = Clamp(value, min, max);
            T newValue = clamped;

            if (inputState.Focused)
                newValue = ApplyKeys(inputState, newValue, min, max, effectiveStep);

            value = newValue;

            double normalized = ComputeNormalized(newValue, min, max);

            Theme theme = context.Theme;

            context.OpenElement(
                new Implementation(
                    normalized,
                    inputState.Focused,
                    theme.Border.Unfocused,
                    theme.PlaceholderText,
                    theme.InteractableSurface.Focused.Background
                ),
                style
            );
            context.CloseElement();
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

    private static double ComputeNormalized<T>(T value, T min, T max)
        where T : struct, INumber<T>
    {
        if (max == min)
            return 0.0;

        double numerator = double.CreateChecked(value - min);
        double denominator = double.CreateChecked(max - min);
        return numerator / denominator;
    }

    // Renders a horizontal slider bar with a single-cell handle positioned by a
    // normalized fraction in [0, 1]. The widget is intentionally non-generic —
    // it knows nothing about the underlying numeric type.
    internal sealed class Implementation(
        double normalizedPosition,
        bool focused,
        TerminalColor barColor = default,
        TerminalColor handleColor = default,
        TerminalColor focusedHandleColor = default
    ) : IWidget
    {
        private const char BarCharacter = '─';
        private const char HandleCharacter = '█';
        private const int DefaultPreferredWidth = 20;

        // Clamp to [0, 1] up front so renderers do not have to worry about
        // out-of-range input (NaN is treated as 0).
        public double NormalizedPosition { get; } =
            double.IsNaN(normalizedPosition) ? 0.0
            : normalizedPosition < 0.0 ? 0.0
            : normalizedPosition > 1.0 ? 1.0
            : normalizedPosition;
        public bool Focused { get; } = focused;
        public TerminalColor BarColor { get; } = barColor;
        public TerminalColor HandleColor { get; } = handleColor;
        public TerminalColor FocusedHandleColor { get; } = focusedHandleColor;

        private TerminalColor CurrentHandleColor => Focused ? FocusedHandleColor : HandleColor;

        public WidgetLayout Measure() =>
            new(new Dimensions(2, 1), new Dimensions(DefaultPreferredWidth, 1));

        public void Render(Rect bounds, ICanvas canvas)
        {
            if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
                return;

            int width = bounds.Dimensions.Width;
            int handleColumn = ComputeHandleColumn(width);

            for (int column = 0; column < width; column++)
            {
                canvas.Draw(
                    new Position(bounds.Position.X + column, bounds.Position.Y),
                    BarCharacter,
                    BarColor
                );
            }

            canvas.Draw(
                new Position(bounds.Position.X + handleColumn, bounds.Position.Y),
                HandleCharacter,
                CurrentHandleColor
            );
        }

        private int ComputeHandleColumn(int width)
        {
            if (width <= 1)
                return 0;

            double scaled = NormalizedPosition * (width - 1);
            int column = (int)Math.Round(scaled);
            return column < 0 ? 0
                : column >= width ? width - 1
                : column;
        }
    }
}
