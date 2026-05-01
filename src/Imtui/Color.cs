// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public enum ColorKind : byte
{
    Default = 0,
    Ansi = 1,
    Palette256 = 2,
    Rgb = 3,
}

public readonly record struct Color
{
    private readonly byte _byte1;
    private readonly byte _byte2;
    private readonly byte _byte3;

    private Color(ColorKind kind, byte r, byte g, byte b)
    {
        Kind = kind;
        _byte1 = r;
        _byte2 = g;
        _byte3 = b;
    }

    public ColorKind Kind { get; }

    /// <summary>
    /// The ANSI color index (0–15). Only valid when <see cref="Kind"/> is
    /// <see cref="ColorKind.Ansi"/>.
    /// </summary>
    public AnsiColor AnsiColor =>
        Kind == ColorKind.Ansi
            ? (AnsiColor)_byte1
            : throw new InvalidOperationException("Not an ANSI color");

    /// <summary>
    /// The 256-palette index. Only valid when <see cref="Kind"/> is
    /// <see cref="ColorKind.Palette256"/>.
    /// </summary>
    public byte PaletteIndex =>
        Kind == ColorKind.Palette256
            ? _byte1
            : throw new InvalidOperationException("Not a palette color");

    /// <summary>
    /// The red component. Only valid when <see cref="Kind"/> is
    /// <see cref="ColorKind.Rgb"/>.
    /// </summary>
    public byte R =>
        Kind == ColorKind.Rgb ? _byte1 : throw new InvalidOperationException("Not an RGB color");

    /// <summary>
    /// The green component. Only valid when <see cref="Kind"/> is
    /// <see cref="ColorKind.Rgb"/>.
    /// </summary>
    public byte G =>
        Kind == ColorKind.Rgb ? _byte2 : throw new InvalidOperationException("Not an RGB color");

    /// <summary>
    /// The blue component. Only valid when <see cref="Kind"/> is
    /// <see cref="ColorKind.Rgb"/>.
    /// </summary>
    public byte B =>
        Kind == ColorKind.Rgb ? _byte3 : throw new InvalidOperationException("Not an RGB color");

    public static Color Default => default;

    public static Color Ansi(AnsiColor color) => new(ColorKind.Ansi, (byte)color, 0, 0);

    public static Color Palette256(byte index) => new(ColorKind.Palette256, index, 0, 0);

    public static Color Rgb(byte r, byte g, byte b) => new(ColorKind.Rgb, r, g, b);
}
