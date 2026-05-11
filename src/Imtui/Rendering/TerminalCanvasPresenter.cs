// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

using System.Runtime.CompilerServices;
using System.Text;

public static class TerminalCanvasPresenter
{
    private static readonly ConditionalWeakTable<TerminalCanvas, PresentationState> s_states = [];

    public static void Present(this TerminalCanvas canvas, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(output);

        PresentationState state = s_states.GetOrCreateValue(canvas);
        StringBuilder sb = new StringBuilder(canvas.Width * canvas.Height * 24);

        if (state.FirstPresent)
        {
            sb.Append("\x1b[s"); // save cursor position

            // Reserve vertical space by emitting blank lines, then come back up.
            // This ensures we don't get pushed off-screen when starting near the bottom.
            for (int i = 0; i < canvas.Height; i++)
                sb.Append('\n');

            sb.Append("\x1b[").Append(canvas.Height).Append('A'); // move cursor up Height lines
            state.FirstPresent = false;
        }
        else
        {
            sb.Append("\x1b[u"); // restore to saved position
        }

        TerminalColor foregroundColor = default;
        TerminalColor backgroundColor = default;

        for (int y = 0; y < canvas.Height; y++)
        {
            sb.Append("\x1b[G"); // move to column 1 of current line
            for (int x = 0; x < canvas.Width; x++)
            {
                TerminalCell cell = canvas.GetCell(x, y);
                AppendColorSequences(sb, cell, ref foregroundColor, ref backgroundColor);
                sb.Append(cell.CharacterOrSpace);
            }
            sb.Append("\x1b[0m");
            foregroundColor = default;
            backgroundColor = default;
            if (y < canvas.Height - 1)
                sb.Append('\n');
        }

        sb.Append('\n');
        sb.Append("\x1b[u"); // restore cursor position
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

    private sealed class PresentationState
    {
        public bool FirstPresent { get; set; } = true;
    }
}
