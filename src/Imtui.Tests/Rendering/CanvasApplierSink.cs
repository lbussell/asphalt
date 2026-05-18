// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Rendering;

using Imtui.Rendering;
using Imtui.Rendering.Diffing;

// A test sink that simulates a terminal: it applies render operations to a
// mutable grid of cells starting from a snapshot of the previous canvas. The
// resulting grid is what the terminal would display after receiving the same
// sequence of operations. Used to verify Correctness (Apply(prev, Diff) == next).
internal sealed class CanvasApplierSink : IRenderOpsSink
{
    private readonly TerminalCell[,] _cells;
    private readonly int _width;
    private readonly int _height;

    private int _cursorColumn;
    private int _cursorRow;
    private TerminalColor _foreground;
    private TerminalColor _background;

    public CanvasApplierSink(TerminalCanvas previous)
    {
        ArgumentNullException.ThrowIfNull(previous);
        _width = previous.Width;
        _height = previous.Height;
        _cells = new TerminalCell[_height, _width];
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                _cells[y, x] = previous.GetCell(x, y);
            }
        }
    }

    public TerminalCell[,] Result => _cells;

    public void MoveTo(int column, int row)
    {
        _cursorColumn = column;
        _cursorRow = row;
    }

    public void SetForeground(TerminalColor color) => _foreground = color;

    public void SetBackground(TerminalColor color) => _background = color;

    public void ResetSgr()
    {
        _foreground = default;
        _background = default;
    }

    public void WriteText(ReadOnlySpan<char> text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (
                _cursorColumn >= 0
                && _cursorColumn < _width
                && _cursorRow >= 0
                && _cursorRow < _height
            )
            {
                _cells[_cursorRow, _cursorColumn] = new TerminalCell(
                    text[i],
                    _foreground,
                    _background
                );
            }
            _cursorColumn++;
        }
    }
}
