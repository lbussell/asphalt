// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace ImtuiLib;

/// <summary>
/// Describes the content and style of a single terminal cell.
/// </summary>
public readonly record struct Cell
{
    /// <summary>
    /// Gets an empty cell.
    /// </summary>
    public static Cell Empty { get; } = new(new Rune(' '), CellStyle.Default);

    /// <summary>
    /// Initializes a new instance of the <see cref="Cell"/> struct.
    /// </summary>
    /// <param name="glyph">The Unicode scalar value rendered in the cell.</param>
    /// <param name="style">The cell style.</param>
    public Cell(Rune glyph, CellStyle style = default)
    {
        Glyph = glyph;
        Style = style;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Cell"/> struct.
    /// </summary>
    /// <param name="glyph">The UTF-16 character rendered in the cell.</param>
    /// <param name="style">The cell style.</param>
    public Cell(char glyph, CellStyle style = default)
        : this(new Rune(glyph), style) { }

    /// <summary>
    /// Gets the Unicode scalar value rendered in the cell.
    /// </summary>
    public Rune Glyph { get; }

    /// <summary>
    /// Gets the cell style.
    /// </summary>
    public CellStyle Style { get; }
}
