// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Text;

public sealed class TerminalCanvas(Dimensions dimensions) : ICanvas
{
    private readonly TerminalCell[,] _cells = new TerminalCell[dimensions.Height, dimensions.Width];
    private bool _firstPresent = true;
    public Dimensions Dimensions { get; } = dimensions;
    public int Width => Dimensions.Width;
    public int Height => Dimensions.Height;

    public void Draw(
        Position position,
        char character,
        TerminalColor foregroundColor = default,
        TerminalColor backgroundColor = default
    )
    {
        if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
            return;

        _cells[position.Y, position.X] = new TerminalCell(
            character,
            foregroundColor,
            backgroundColor
        );
    }

    public void Present(TextWriter output)
    {
        StringBuilder sb = new StringBuilder(Width * Height * 24);

        if (_firstPresent)
        {
            // Reserve vertical space by emitting blank lines, then come back up.
            // This ensures we don't get pushed off-screen when starting near the bottom.
            for (int i = 0; i < Height; i++)
                sb.Append('\n');

            sb.Append("\x1b[").Append(Height).Append('A'); // move cursor up Height lines
            _firstPresent = false;
        }
        else
        {
            sb.Append("\x1b[u"); // restore to saved position
        }

        TerminalColor foregroundColor = default;
        TerminalColor backgroundColor = default;

        for (int y = 0; y < Height; y++)
        {
            sb.Append("\x1b[G"); // move to column 1 of current line
            for (int x = 0; x < Width; x++)
            {
                TerminalCell cell = _cells[y, x];
                AppendColorSequences(sb, cell, ref foregroundColor, ref backgroundColor);
                sb.Append(cell.CharacterOrSpace);
            }
            sb.Append("\x1b[0m");
            foregroundColor = default;
            backgroundColor = default;
            if (y < Height - 1)
                sb.Append('\n');
        }

        sb.Append('\n');
        output.Write(sb.ToString());
        output.Flush();
    }

    private static void AppendColorSequences(
        StringBuilder output,
        TerminalCell cell,
        ref TerminalColor foregroundColor,
        ref TerminalColor backgroundColor
    )
    {
        if (cell.ForegroundColor == foregroundColor && cell.BackgroundColor == backgroundColor)
            return;

        if (IsDefault(cell.ForegroundColor) || IsDefault(cell.BackgroundColor))
        {
            output.Append("\x1b[0m");
            foregroundColor = default;
            backgroundColor = default;
        }

        if (cell.BackgroundColor != backgroundColor)
        {
            AppendColorSequence(output, cell.BackgroundColor, foreground: false);
            backgroundColor = cell.BackgroundColor;
        }

        if (cell.ForegroundColor != foregroundColor)
        {
            AppendColorSequence(output, cell.ForegroundColor, foreground: true);
            foregroundColor = cell.ForegroundColor;
        }
    }

    private static bool IsDefault(TerminalColor color) => color.Kind == TerminalColorKind.Default;

    private static void AppendColorSequence(
        StringBuilder output,
        TerminalColor color,
        bool foreground
    )
    {
        if (IsDefault(color))
            return;

        output.Append("\x1b[");
        switch (color.Kind)
        {
            case TerminalColorKind.Ansi16:
                output.Append(GetAnsi16Code(color.AnsiIndex, foreground));
                break;
            case TerminalColorKind.Palette256:
                output.Append(foreground ? 38 : 48).Append(";5;").Append(color.PaletteIndex);
                break;
            case TerminalColorKind.Rgb:
                output
                    .Append(foreground ? 38 : 48)
                    .Append(";2;")
                    .Append(color.R)
                    .Append(';')
                    .Append(color.G)
                    .Append(';')
                    .Append(color.B);
                break;
        }
        output.Append('m');
    }

    private static int GetAnsi16Code(byte colorIndex, bool foreground)
    {
        if (colorIndex < 8)
            return (foreground ? 30 : 40) + colorIndex;

        return (foreground ? 90 : 100) + colorIndex - 8;
    }

    private readonly record struct TerminalCell(
        char Character,
        TerminalColor ForegroundColor,
        TerminalColor BackgroundColor
    )
    {
        public char CharacterOrSpace => Character == default ? ' ' : Character;
    }
}
