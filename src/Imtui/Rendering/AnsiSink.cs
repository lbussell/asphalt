// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

using System.Text;

// Writes render operations as ANSI escape sequences to an underlying
// StringBuilder. This is the production sink; it performs no optimization
// of its own and emits exactly what the differ tells it to.
public sealed class AnsiSink(StringBuilder output) : IRenderOpsSink
{
    private readonly StringBuilder _output = output;

    public void MoveTo(int column, int row)
    {
        // CUP is 1-based: ESC [ row ; col H
        _output.Append("\x1b[").Append(row + 1).Append(';').Append(column + 1).Append('H');
    }

    public void SetForeground(TerminalColor color) => AppendColor(color, foreground: true);

    public void SetBackground(TerminalColor color) => AppendColor(color, foreground: false);

    public void SetStyle(TextStyle added, TextStyle removed)
    {
        EmitStyleCodes(added, on: true);
        EmitStyleCodes(removed, on: false);
    }

    public void ResetSgr() => _output.Append("\x1b[0m");

    public void WriteText(ReadOnlySpan<char> text) => _output.Append(text);

    private void AppendColor(TerminalColor color, bool foreground)
    {
        _output.Append("\x1b[");
        switch (color.Kind)
        {
            case TerminalColorKind.Default:
                _output.Append(foreground ? 39 : 49);
                break;
            case TerminalColorKind.Ansi16:
                _output.Append(GetAnsi16Code(color.AnsiIndex, foreground));
                break;
            case TerminalColorKind.Palette256:
                _output.Append(foreground ? 38 : 48).Append(";5;").Append(color.PaletteIndex);
                break;
            case TerminalColorKind.Rgb:
                _output
                    .Append(foreground ? 38 : 48)
                    .Append(";2;")
                    .Append(color.R)
                    .Append(';')
                    .Append(color.G)
                    .Append(';')
                    .Append(color.B);
                break;
        }
        _output.Append('m');
    }

    private static int GetAnsi16Code(byte colorIndex, bool foreground)
    {
        if (colorIndex < 8)
            return (foreground ? 30 : 40) + colorIndex;

        return (foreground ? 90 : 100) + colorIndex - 8;
    }

    private void EmitStyleCodes(TextStyle flags, bool on)
    {
        if (flags == TextStyle.None)
            return;

        if (flags.HasFlag(TextStyle.Reverse))
            _output.Append(on ? "\x1b[7m" : "\x1b[27m");
    }
}
