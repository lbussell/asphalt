// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

// Renders the formatted display string for a scalar value inside a single-row
// textbox cell. Intentionally non-generic and read-only: the widget knows
// nothing about the underlying numeric type and never mutates anything.
// Mutation happens in ScalarInputExtensions before this widget is constructed.
public sealed class ScalarInputWidget(
    string displayText,
    bool focused,
    int? preferredWidth = null,
    TerminalColor backgroundColor = default,
    TerminalColor focusedBackgroundColor = default
) : IWidget
{
    private const int HorizontalPadding = 1;

    private static readonly TerminalColor s_backgroundColor = TerminalColor.Rgb(0x3F, 0x3F, 0x48);
    private static readonly TerminalColor s_focusedBackgroundColor = TerminalColor.Rgb(
        0x29,
        0x4A,
        0x7A
    );

    public string DisplayText { get; } =
        displayText ?? throw new ArgumentNullException(nameof(displayText));
    public bool Focused { get; } = focused;
    public TerminalColor BackgroundColor { get; } =
        backgroundColor == default ? s_backgroundColor : backgroundColor;
    public TerminalColor FocusedBackgroundColor { get; } =
        focusedBackgroundColor == default ? s_focusedBackgroundColor : focusedBackgroundColor;

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
