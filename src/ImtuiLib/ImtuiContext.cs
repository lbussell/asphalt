// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace ImtuiLib;

/// <summary>
/// Provides an instance-based immediate-mode terminal UI context.
/// </summary>
public sealed class ImtuiContext
{
    private Cell[]? _previousCells;
    private ViewportSize? _previousSize;
    private Cell[]? _currentCells;
    private ViewportSize _currentSize;
    private int _layoutLine;
    private bool _isFrameActive;

    /// <summary>
    /// Starts a new immediate-mode frame.
    /// </summary>
    /// <param name="request">The frame request.</param>
    /// <exception cref="InvalidOperationException">Thrown when a frame is already active.</exception>
    public void NewFrame(FrameRequest request)
    {
        if (_isFrameActive)
        {
            throw new InvalidOperationException("A frame is already active.");
        }

        _currentSize = request.Size;
        _currentCells = CreateEmptyCells(_currentSize);
        _layoutLine = 0;
        _isFrameActive = true;
    }

    /// <summary>
    /// Writes text at the next layout line.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="style">The cell style.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no frame is active.</exception>
    public void Text(string text, CellStyle style = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        EnsureFrameActive();

        TextAt(new CellPosition(0, _layoutLine), text, style);
        _layoutLine++;
    }

    /// <summary>
    /// Writes text at a specific cell position.
    /// </summary>
    /// <param name="position">The starting cell position.</param>
    /// <param name="text">The text to write.</param>
    /// <param name="style">The cell style.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no frame is active.</exception>
    public void TextAt(CellPosition position, string text, CellStyle style = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        EnsureFrameActive();

        int x = position.X;
        int y = position.Y;

        foreach (Rune rune in text.EnumerateRunes())
        {
            if (rune.Value is '\r')
            {
                continue;
            }

            if (rune.Value is '\n')
            {
                x = position.X;
                y++;
                continue;
            }

            SetCell(new CellPosition(x, y), new Cell(rune, style));
            x++;
        }
    }

    /// <summary>
    /// Sets a single cell in the active frame.
    /// </summary>
    /// <param name="position">The cell position.</param>
    /// <param name="cell">The cell value.</param>
    /// <exception cref="InvalidOperationException">Thrown when no frame is active.</exception>
    public void SetCell(CellPosition position, Cell cell)
    {
        EnsureFrameActive();

        if (
            position.X < 0
            || position.X >= _currentSize.Width
            || position.Y < 0
            || position.Y >= _currentSize.Height
        )
        {
            return;
        }

        _currentCells![RenderedFrame.GetIndex(_currentSize, position)] = cell;
    }

    /// <summary>
    /// Completes the active frame and returns the rendered cell grid.
    /// </summary>
    /// <returns>The rendered frame.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no frame is active.</exception>
    public RenderedFrame Render()
    {
        EnsureFrameActive();

        Cell[] renderedCells = _currentCells!;
        CellChange[] changes = CreateChanges(renderedCells);
        RenderedFrame frame = new(_currentSize, renderedCells, changes);

        _previousCells = renderedCells;
        _previousSize = _currentSize;
        _currentCells = null;
        _isFrameActive = false;

        return frame;
    }

    private static Cell[] CreateEmptyCells(ViewportSize size)
    {
        Cell[] cells = new Cell[size.Width * size.Height];
        Array.Fill(cells, Cell.Empty);
        return cells;
    }

    private CellChange[] CreateChanges(Cell[] renderedCells)
    {
        bool hasPreviousFrame =
            _previousCells is not null
            && _previousSize == _currentSize
            && _previousCells.Length == renderedCells.Length;

        List<CellChange> changes = [];
        for (int y = 0; y < _currentSize.Height; y++)
        {
            for (int x = 0; x < _currentSize.Width; x++)
            {
                CellPosition position = new(x, y);
                int index = RenderedFrame.GetIndex(_currentSize, position);
                Cell cell = renderedCells[index];

                if (!hasPreviousFrame || _previousCells![index] != cell)
                {
                    changes.Add(new CellChange(position, cell));
                }
            }
        }

        return [.. changes];
    }

    private void EnsureFrameActive()
    {
        if (!_isFrameActive)
        {
            throw new InvalidOperationException("No frame is active.");
        }
    }
}
