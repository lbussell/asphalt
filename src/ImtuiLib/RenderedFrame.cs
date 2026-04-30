// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace ImtuiLib;

/// <summary>
/// Describes a changed cell in a rendered frame.
/// </summary>
public readonly record struct CellChange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CellChange"/> struct.
    /// </summary>
    /// <param name="position">The position of the changed cell.</param>
    /// <param name="cell">The new cell value.</param>
    public CellChange(CellPosition position, Cell cell)
    {
        Position = position;
        Cell = cell;
    }

    /// <summary>
    /// Gets the changed cell position.
    /// </summary>
    public CellPosition Position { get; }

    /// <summary>
    /// Gets the new cell value.
    /// </summary>
    public Cell Cell { get; }
}

/// <summary>
/// Describes the read-only result of rendering one immediate-mode frame.
/// </summary>
public sealed class RenderedFrame
{
    private readonly Cell[] _cells;
    private readonly CellChange[] _changes;

    internal RenderedFrame(ViewportSize size, Cell[] cells, CellChange[] changes)
    {
        Size = size;
        _cells = cells;
        _changes = changes;
    }

    /// <summary>
    /// Gets the rendered viewport size.
    /// </summary>
    public ViewportSize Size { get; }

    /// <summary>
    /// Gets the rendered viewport width, in terminal cells.
    /// </summary>
    public int Width => Size.Width;

    /// <summary>
    /// Gets the rendered viewport height, in terminal cells.
    /// </summary>
    public int Height => Size.Height;

    /// <summary>
    /// Gets the changed cells since the previous rendered frame.
    /// </summary>
    public IReadOnlyList<CellChange> Changes => _changes;

    /// <summary>
    /// Gets the rendered cell at the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal cell coordinate.</param>
    /// <param name="y">The vertical cell coordinate.</param>
    /// <returns>The rendered cell.</returns>
    public Cell this[int x, int y] => GetCell(new CellPosition(x, y));

    /// <summary>
    /// Gets the rendered cell at the specified position.
    /// </summary>
    /// <param name="position">The cell position.</param>
    /// <returns>The rendered cell.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="position"/> is outside the rendered viewport.
    /// </exception>
    public Cell GetCell(CellPosition position)
    {
        if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        return _cells[GetIndex(Size, position)];
    }

    internal static int GetIndex(ViewportSize size, CellPosition position) =>
        (position.Y * size.Width) + position.X;
}
