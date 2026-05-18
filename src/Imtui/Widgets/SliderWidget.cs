// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

// Renders a horizontal slider bar with a single-cell handle positioned by a
// normalized fraction in [0, 1]. The widget is intentionally non-generic — it
// knows nothing about the underlying numeric type — so that the same renderer
// can be reused for int, long, float, double, or any future INumber<T>
// instantiation handled by the extension method.
public sealed class SliderWidget(
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

    private static readonly TerminalColor s_barColor = TerminalColor.Rgb(0x3F, 0x3F, 0x48);
    private static readonly TerminalColor s_handleColor = TerminalColor.Rgb(0x80, 0x80, 0x80);
    private static readonly TerminalColor s_focusedHandleColor = TerminalColor.Rgb(
        0x4A,
        0x90,
        0xE2
    );

    // Clamp to [0, 1] up front so renderers do not have to worry about
    // out-of-range input (NaN is treated as 0).
    public double NormalizedPosition { get; } =
        double.IsNaN(normalizedPosition) ? 0.0
        : normalizedPosition < 0.0 ? 0.0
        : normalizedPosition > 1.0 ? 1.0
        : normalizedPosition;
    public bool Focused { get; } = focused;
    public TerminalColor BarColor { get; } = barColor == default ? s_barColor : barColor;
    public TerminalColor HandleColor { get; } =
        handleColor == default ? s_handleColor : handleColor;
    public TerminalColor FocusedHandleColor { get; } =
        focusedHandleColor == default ? s_focusedHandleColor : focusedHandleColor;

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
