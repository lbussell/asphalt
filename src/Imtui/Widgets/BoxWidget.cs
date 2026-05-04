// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Imtui.Rendering;

namespace Imtui.Widgets;

/// <summary>
/// Provides the Box widget for <see cref="ImtuiContext"/>.
/// </summary>
public static class BoxWidget
{
    extension(ImtuiContext context)
    {
        /// <summary>
        /// Draws a solid-color rectangle filled with the given background color.
        /// Coordinates are inclusive.
        /// </summary>
        public void Box(
            int topLeftX,
            int topLeftY,
            int bottomRightX,
            int bottomRightY,
            AnsiColor color
        )
        {
            CellStyle style = new(Color.Default, Color.Ansi(color));
            Cell cell = new(new Rune(' '), style);

            for (int y = topLeftY; y <= bottomRightY; y++)
            {
                for (int x = topLeftX; x <= bottomRightX; x++)
                {
                    context.WriteCell(new CellPosition(x, y), cell);
                }
            }
        }
    }
}
