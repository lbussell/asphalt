// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace ImtuiLib;

/// <summary>
/// Describes terminal text attributes for a cell.
/// </summary>
[Flags]
public enum TextAttributes
{
    /// <summary>
    /// No additional text attributes.
    /// </summary>
    None = 0,

    /// <summary>
    /// Bold text.
    /// </summary>
    Bold = 1 << 0,

    /// <summary>
    /// Dim text.
    /// </summary>
    Dim = 1 << 1,

    /// <summary>
    /// Italic text.
    /// </summary>
    Italic = 1 << 2,

    /// <summary>
    /// Underlined text.
    /// </summary>
    Underline = 1 << 3,

    /// <summary>
    /// Strikethrough text.
    /// </summary>
    Strikethrough = 1 << 4,

    /// <summary>
    /// Reverses the foreground and background colors.
    /// </summary>
    Reverse = 1 << 5,
}

/// <summary>
/// Describes the style of a terminal cell.
/// </summary>
public readonly record struct CellStyle
{
    /// <summary>
    /// Gets the default cell style.
    /// </summary>
    public static CellStyle Default => default;

    /// <summary>
    /// Initializes a new instance of the <see cref="CellStyle"/> struct.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="attributes">The text attributes.</param>
    public CellStyle(
        TerminalColor foreground,
        TerminalColor background,
        TextAttributes attributes = TextAttributes.None
    )
    {
        Foreground = foreground;
        Background = background;
        Attributes = attributes;
    }

    /// <summary>
    /// Gets the foreground color.
    /// </summary>
    public TerminalColor Foreground { get; }

    /// <summary>
    /// Gets the background color.
    /// </summary>
    public TerminalColor Background { get; }

    /// <summary>
    /// Gets the text attributes.
    /// </summary>
    public TextAttributes Attributes { get; }
}
