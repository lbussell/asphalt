// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

public static class LayoutDebug
{
    private static readonly int[] s_colors =
    [
        24,
        25,
        26,
        27,
        28,
        29,
        30,
        31,
        32,
        33,
        34,
        35,
        36,
        37,
        38,
        39,
        40,
        41,
        42,
        43,
        44,
        45,
    ];

    public static void PrintLayout(LayoutNode root, TextWriter output)
    {
        int width = Math.Max(0, root.Dimensions.Width);
        int height = Math.Max(0, root.Dimensions.Height);
        if (width == 0 || height == 0)
            return;

        int[] cells = new int[width * height];
        Array.Fill(cells, -1);

        int nextColor = 0;
        Paint(root);

        for (int y = 0; y < height; y++)
        {
            int currentColor = -1;
            for (int x = 0; x < width; x++)
            {
                int color = cells[(y * width) + x];
                if (color != currentColor)
                {
                    WriteBackground(output, color);
                    currentColor = color;
                }

                output.Write(' ');
            }

            output.Write("\x1b[0m");
            output.WriteLine();
        }

        output.Flush();

        void Paint(LayoutNode node)
        {
            int color = s_colors[nextColor % s_colors.Length];
            nextColor++;

            int left = Math.Max(0, node.Position.X - root.Position.X);
            int top = Math.Max(0, node.Position.Y - root.Position.Y);
            int right = Math.Min(width, left + Math.Max(0, node.Dimensions.Width));
            int bottom = Math.Min(height, top + Math.Max(0, node.Dimensions.Height));

            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                    cells[(y * width) + x] = color;
            }

            foreach (LayoutNode child in node.Children)
                Paint(child);
        }
    }

    private static void WriteBackground(TextWriter output, int color)
    {
        if (color < 0)
            output.Write("\x1b[0m");
        else
            output.Write($"\x1b[48;5;{color}m");
    }
}
