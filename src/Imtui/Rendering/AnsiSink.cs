// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

using System.Text;

// Writes render operations as ANSI escape sequences to an underlying
// StringBuilder. This is the production sink; it performs no optimization
// of its own and emits exactly what the differ tells it to.
//
// MoveTo uses one of two cursor-addressing strategies. Absolute (default)
// emits CUP — `ESC [ row+1 ; col+1 H` — which positions the cursor at the
// terminal-absolute coordinates. That's correct when the canvas origin is
// at terminal (1, 1), e.g. in alt-screen mode. OffsetFromSaved emits
// ESC [ u once to restore the cursor to the previously-saved canvas
// origin, then tracks the cursor internally and emits only relative
// motion (ESC [ B / C / A / D) thereafter. We avoid re-emitting ESC [ u
// per MoveTo because some terminals (e.g. ghostty) restore SGR attributes
// alongside the cursor, which silently clobbers colors and styles the
// differ thinks are still active on the terminal.
public sealed class AnsiSink(
    StringBuilder output,
    CursorAddressing addressing = CursorAddressing.Absolute
) : IRenderOpsSink
{
    private readonly StringBuilder _output = output;
    private readonly CursorAddressing _addressing = addressing;

    // Tracked cursor for OffsetFromSaved mode. Negative means "unknown —
    // emit ESC [ u to establish a canvas-origin reference on the next
    // MoveTo, then start tracking from (0, 0)".
    private int _trackedCol = -1;
    private int _trackedRow = -1;

    public void MoveTo(int column, int row)
    {
        switch (_addressing)
        {
            case CursorAddressing.Absolute:
                // CUP is 1-based: ESC [ row ; col H
                _output.Append("\x1b[").Append(row + 1).Append(';').Append(column + 1).Append('H');
                break;
            case CursorAddressing.OffsetFromSaved:
                if (_trackedCol < 0)
                {
                    _output.Append("\x1b[u"); // restore once to canvas origin
                    _trackedCol = 0;
                    _trackedRow = 0;
                }
                int deltaRow = row - _trackedRow;
                int deltaCol = column - _trackedCol;
                if (deltaRow > 0)
                    _output.Append("\x1b[").Append(deltaRow).Append('B');
                else if (deltaRow < 0)
                    _output.Append("\x1b[").Append(-deltaRow).Append('A');
                if (deltaCol > 0)
                    _output.Append("\x1b[").Append(deltaCol).Append('C');
                else if (deltaCol < 0)
                    _output.Append("\x1b[").Append(-deltaCol).Append('D');
                _trackedCol = column;
                _trackedRow = row;
                break;
        }
    }

    public void SetForeground(TerminalColor color) => AppendColor(color, foreground: true);

    public void SetBackground(TerminalColor color) => AppendColor(color, foreground: false);

    public void SetStyle(TextStyle added, TextStyle removed)
    {
        EmitStyleCodes(added, on: true);
        EmitStyleCodes(removed, on: false);
    }

    public void ResetSgr() => _output.Append("\x1b[0m");

    public void WriteText(ReadOnlySpan<char> text)
    {
        _output.Append(text);
        if (_addressing == CursorAddressing.OffsetFromSaved && _trackedCol >= 0)
            _trackedCol += text.Length;
    }

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

public enum CursorAddressing
{
    // ESC [ row+1 ; col+1 H — positions cursor at absolute terminal
    // coordinates. Correct when the canvas origin is at terminal (1,1).
    Absolute,

    // ESC [ u then relative ESC [ {n} B / ESC [ {n} C — positions cursor
    // relative to a previously-saved cursor position. The caller is
    // responsible for emitting ESC [ s once before any MoveTo, at the
    // intended canvas origin.
    OffsetFromSaved,
}
