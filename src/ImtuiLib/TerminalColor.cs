// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace ImtuiLib;

/// <summary>
/// Describes the representation used by a terminal color.
/// </summary>
public enum TerminalColorKind
{
    /// <summary>
    /// Uses the terminal's default color.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Uses one of the 16 ANSI colors.
    /// </summary>
    Ansi16,

    /// <summary>
    /// Uses an indexed 256-color terminal palette entry.
    /// </summary>
    Indexed256,

    /// <summary>
    /// Uses a 24-bit RGB color.
    /// </summary>
    Rgb,
}

/// <summary>
/// Identifies one of the standard 16 ANSI terminal colors.
/// </summary>
public enum AnsiColor
{
    /// <summary>
    /// Black.
    /// </summary>
    Black = 0,

    /// <summary>
    /// Red.
    /// </summary>
    Red = 1,

    /// <summary>
    /// Green.
    /// </summary>
    Green = 2,

    /// <summary>
    /// Yellow.
    /// </summary>
    Yellow = 3,

    /// <summary>
    /// Blue.
    /// </summary>
    Blue = 4,

    /// <summary>
    /// Magenta.
    /// </summary>
    Magenta = 5,

    /// <summary>
    /// Cyan.
    /// </summary>
    Cyan = 6,

    /// <summary>
    /// White.
    /// </summary>
    White = 7,

    /// <summary>
    /// Bright black.
    /// </summary>
    BrightBlack = 8,

    /// <summary>
    /// Bright red.
    /// </summary>
    BrightRed = 9,

    /// <summary>
    /// Bright green.
    /// </summary>
    BrightGreen = 10,

    /// <summary>
    /// Bright yellow.
    /// </summary>
    BrightYellow = 11,

    /// <summary>
    /// Bright blue.
    /// </summary>
    BrightBlue = 12,

    /// <summary>
    /// Bright magenta.
    /// </summary>
    BrightMagenta = 13,

    /// <summary>
    /// Bright cyan.
    /// </summary>
    BrightCyan = 14,

    /// <summary>
    /// Bright white.
    /// </summary>
    BrightWhite = 15,
}

/// <summary>
/// Describes a terminal color without binding the renderer to a specific console API.
/// </summary>
public readonly record struct TerminalColor
{
    /// <summary>
    /// Gets the terminal default color.
    /// </summary>
    public static TerminalColor Default => default;

    private TerminalColor(TerminalColorKind kind, int index, byte red, byte green, byte blue)
    {
        Kind = kind;
        Index = index;
        Red = red;
        Green = green;
        Blue = blue;
    }

    /// <summary>
    /// Gets the color representation.
    /// </summary>
    public TerminalColorKind Kind { get; }

    /// <summary>
    /// Gets the ANSI color number or 256-color palette index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the red component for RGB colors.
    /// </summary>
    public byte Red { get; }

    /// <summary>
    /// Gets the green component for RGB colors.
    /// </summary>
    public byte Green { get; }

    /// <summary>
    /// Gets the blue component for RGB colors.
    /// </summary>
    public byte Blue { get; }

    /// <summary>
    /// Creates a terminal color from one of the standard 16 ANSI colors.
    /// </summary>
    /// <param name="color">The ANSI color.</param>
    /// <returns>A terminal color value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="color"/> is outside the defined ANSI color range.
    /// </exception>
    public static TerminalColor FromAnsi(AnsiColor color)
    {
        if (color is < AnsiColor.Black or > AnsiColor.BrightWhite)
        {
            throw new ArgumentOutOfRangeException(nameof(color));
        }

        return new TerminalColor(TerminalColorKind.Ansi16, (int)color, 0, 0, 0);
    }

    /// <summary>
    /// Creates a terminal color from a 256-color palette index.
    /// </summary>
    /// <param name="index">The 256-color palette index.</param>
    /// <returns>A terminal color value.</returns>
    public static TerminalColor FromIndex(byte index) =>
        new(TerminalColorKind.Indexed256, index, 0, 0, 0);

    /// <summary>
    /// Creates a terminal color from 24-bit RGB components.
    /// </summary>
    /// <param name="red">The red component.</param>
    /// <param name="green">The green component.</param>
    /// <param name="blue">The blue component.</param>
    /// <returns>A terminal color value.</returns>
    public static TerminalColor FromRgb(byte red, byte green, byte blue) =>
        new(TerminalColorKind.Rgb, 0, red, green, blue);
}
