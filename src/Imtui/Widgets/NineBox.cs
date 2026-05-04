// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Imtui.Rendering;

namespace Imtui.Widgets;

/// <summary>
/// Provides the Box widget for <see cref="ImtuiContext"/>.
/// </summary>
public static partial class NineBoxWidget
{
    private static readonly char[] s_nineBox = ['┌', '─', '┐', '│', ' ', '│', '└', '─', '┘'];

    extension(ImtuiContext context)
    {
        /// <summary>
        /// Draws a solid-color rectangle filled with the given background color.
        /// Coordinates are inclusive.
        /// </summary>
        public void NineBox(int topLeftX, int topLeftY, int width, int height)
        {
            CellStyle style = new(Color.Default, Color.Default);

            CellPosition topLeft = new(topLeftX, topLeftY);
            CellPosition topRight = new(topLeftX + width, topLeftY);
            CellPosition bottomRight = new(topLeftX + width, topLeftY + height);
            CellPosition bottomLeft = new(topLeftX, topLeftY + height);

            for (int y = topLeftY; y <= topLeftY + height; y += 1)
            {
                for (int x = topLeftX; x <= topLeftX + width; x += 1)
                {
                    CellPosition position = new(x, y);

                    char c = position switch
                    {
                        _ when position == topLeft => s_nineBox[0],
                        _ when position == topRight => s_nineBox[2],
                        _ when position == bottomLeft => s_nineBox[6],
                        _ when position == bottomRight => s_nineBox[8],
                        _ when y == topLeftY => s_nineBox[1],
                        _ when y == topLeftY + height => s_nineBox[7],
                        _ when x == topLeftX => s_nineBox[3],
                        _ when x == topLeftX + width => s_nineBox[5],
                        _ => s_nineBox[4],
                    };

                    Cell cell = new(new Rune(c), style);
                    context.WriteCell(position, cell);
                }
            }
        }
    }
}
