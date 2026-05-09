// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Text;

public interface ICanvas
{
    void Fill(Rect bounds, TerminalColorRgb color);
}

public sealed class TerminalCanvas(Dimensions dimensions) : ICanvas
{
    private readonly TerminalColorRgb[,] _pixels = new TerminalColorRgb[
        dimensions.Height,
        dimensions.Width
    ];
    private bool _firstPresent = true;
    public Dimensions Dimensions { get; } = dimensions;
    public int Width => Dimensions.Width;
    public int Height => Dimensions.Height;

    public void Fill(Rect bounds, TerminalColorRgb color)
    {
        int x0 = Math.Max(0, bounds.Position.X);
        int y0 = Math.Max(0, bounds.Position.Y);
        int x1 = Math.Min(Width, bounds.Position.X + bounds.Dimensions.Width);
        int y1 = Math.Min(Height, bounds.Position.Y + bounds.Dimensions.Height);

        for (int y = y0; y < y1; y++)
        for (int x = x0; x < x1; x++)
            _pixels[y, x] = color;
    }

    public void Present()
    {
        StringBuilder sb = new StringBuilder(Width * Height * 24);

        if (_firstPresent)
        {
            // Reserve vertical space by emitting blank lines, then come back up.
            // This ensures we don't get pushed off-screen when starting near the bottom.
            for (int i = 0; i < Height; i++)
                sb.Append('\n');

            sb.Append("\x1b[").Append(Height).Append('A'); // move cursor up Height lines
            sb.Append("\x1b[s"); // save cursor position
            sb.Append("\x1b[?25l"); // hide cursor
            _firstPresent = false;
        }
        else
        {
            sb.Append("\x1b[u"); // restore to saved position
        }

        for (int y = 0; y < Height; y++)
        {
            sb.Append("\x1b[G"); // move to column 1 of current line
            for (int x = 0; x < Width; x++)
            {
                TerminalColorRgb c = _pixels[y, x];
                sb.Append("\x1b[48;2;")
                    .Append(c.R)
                    .Append(';')
                    .Append(c.G)
                    .Append(';')
                    .Append(c.B)
                    .Append('m');
                sb.Append(' ');
            }
            sb.Append("\x1b[0m");
            if (y < Height - 1)
                sb.Append('\n');
        }

        Console.Write(sb.ToString());
        Console.Out.Flush();
    }
}
