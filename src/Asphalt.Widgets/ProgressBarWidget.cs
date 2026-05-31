// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

using Asphalt.Rendering;

public static class ProgressBarWidget
{
    extension(AsphaltContext context)
    {
        /// <summary>
        /// Draws a horizontal progress bar that fills from left to right as
        /// <paramref name="progress"/> advances from 0 to 1. The value is
        /// computed by the caller; the widget only renders it.
        /// </summary>
        /// <param name="progress">
        /// Fraction filled, in the range [0, 1]. Values outside the range are
        /// clamped; NaN is treated as 0.
        /// </param>
        /// <param name="style">
        /// Layout for the bar. Defaults to growing horizontally with a height
        /// of one row. The bar follows normal layout rules and can be any size
        /// down to a minimum of 1x1.
        /// </param>
        /// <param name="fillColor">
        /// Color of the filled portion. Defaults to <see cref="Theme.ProgressBarFill"/>.
        /// </param>
        /// <param name="trackColor">
        /// Background color of the unfilled portion. Defaults to the theme's
        /// input surface background.
        /// </param>
        /// <remarks>
        /// The bar is not focusable. It uses the full block character and the
        /// left-eighth block characters (U+2588 through U+258F) so the leading
        /// edge can render in eighth-of-a-cell increments for smoother motion.
        /// </remarks>
        public void ProgressBar(
            float progress,
            Layout? style = null,
            TerminalColor fillColor = default,
            TerminalColor trackColor = default
        )
        {
            Theme theme = context.Theme;
            TerminalColor effectiveFill = fillColor == default ? theme.ProgressBarFill : fillColor;
            TerminalColor effectiveTrack =
                trackColor == default ? theme.InputSurface.Unfocused.Background : trackColor;

            context.OpenElement(
                new Implementation(progress, effectiveFill, effectiveTrack),
                style ?? new Layout { Width = LayoutLength.Grow(), Height = LayoutLength.Fixed(1) }
            );
            context.CloseElement();
        }
    }

    internal sealed class Implementation(
        float progress,
        TerminalColor fillColor = default,
        TerminalColor trackColor = default
    ) : IWidget
    {
        private const char FullBlock = '\u2588';
        private const int DefaultPreferredWidth = 20;

        // Clamp to [0, 1] up front (NaN is treated as 0) so the renderer never
        // has to reason about out-of-range input.
        public float Progress { get; } =
            float.IsNaN(progress) ? 0f
            : progress < 0f ? 0f
            : progress > 1f ? 1f
            : progress;

        public TerminalColor FillColor { get; } = fillColor;
        public TerminalColor TrackColor { get; } = trackColor;

        public WidgetLayout Measure() =>
            new(new Dimensions(1, 1), new Dimensions(DefaultPreferredWidth, 1));

        public void Render(Rect bounds, ICanvas canvas)
        {
            int width = bounds.Dimensions.Width;
            int height = bounds.Dimensions.Height;
            if (width <= 0 || height <= 0)
                return;

            // Split the filled length into whole cells plus a leading partial
            // cell rendered in eighths. Rounding a near-full partial up to a
            // whole cell keeps the leading edge from flickering.
            double filledCells = Progress * width;
            int fullCells = (int)Math.Floor(filledCells);
            int eighths = (int)Math.Round((filledCells - fullCells) * 8.0);
            if (eighths == 8)
            {
                fullCells++;
                eighths = 0;
            }

            for (int row = 0; row < height; row++)
            {
                int y = bounds.Position.Y + row;
                for (int column = 0; column < width; column++)
                {
                    int x = bounds.Position.X + column;

                    if (column < fullCells)
                    {
                        canvas.Draw(new Position(x, y), FullBlock, FillColor);
                    }
                    else if (column == fullCells && eighths > 0)
                    {
                        // Left-eighth blocks run U+2588 (8/8) .. U+258F (1/8),
                        // so the glyph for `eighths` eighths is U+2590 - eighths.
                        char partial = (char)('\u2590' - eighths);
                        canvas.Draw(new Position(x, y), partial, FillColor, TrackColor);
                    }
                    else
                    {
                        canvas.Draw(new Position(x, y), ' ', TrackColor, TrackColor);
                    }
                }
            }
        }
    }
}
