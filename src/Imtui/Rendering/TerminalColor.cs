// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

using System.Runtime.InteropServices;

public enum TerminalColorKind : byte
{
    Default,
    Ansi16,
    Palette256,
    Rgb,
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public readonly record struct TerminalColor
{
    private readonly TerminalColorKind _kind;
    private readonly byte _first;
    private readonly byte _second;
    private readonly byte _third;

    internal TerminalColor(TerminalColorKind kind, byte first, byte second, byte third)
    {
        _kind = kind;
        _first = first;
        _second = second;
        _third = third;
    }

    public TerminalColorKind Kind => _kind;
    public byte AnsiIndex => _first;
    public byte PaletteIndex => _first;
    public byte R => _first;
    public byte G => _second;
    public byte B => _third;
}

public static class TerminalColorExtensions
{
    extension(TerminalColor)
    {
        public static TerminalColor Default => default;

        public static TerminalColor Black => AnsiColor(0);
        public static TerminalColor Red => AnsiColor(1);
        public static TerminalColor Green => AnsiColor(2);
        public static TerminalColor Yellow => AnsiColor(3);
        public static TerminalColor Blue => AnsiColor(4);
        public static TerminalColor Magenta => AnsiColor(5);
        public static TerminalColor Cyan => AnsiColor(6);
        public static TerminalColor White => AnsiColor(7);
        public static TerminalColor BrightBlack => AnsiColor(8);
        public static TerminalColor BrightRed => AnsiColor(9);
        public static TerminalColor BrightGreen => AnsiColor(10);
        public static TerminalColor BrightYellow => AnsiColor(11);
        public static TerminalColor BrightBlue => AnsiColor(12);
        public static TerminalColor BrightMagenta => AnsiColor(13);
        public static TerminalColor BrightCyan => AnsiColor(14);
        public static TerminalColor BrightWhite => AnsiColor(15);

        public static TerminalColor Ansi(byte color)
        {
            return color > 15
                ? throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color,
                    "ANSI color must be between 0 and 15."
                )
                : AnsiColor(color);
        }

        public static TerminalColor Palette(byte color) =>
            new(TerminalColorKind.Palette256, color, 0, 0);

        public static TerminalColor Rgb(byte red, byte green, byte blue) =>
            new(TerminalColorKind.Rgb, red, green, blue);
    }

    private static TerminalColor AnsiColor(byte color) =>
        new(TerminalColorKind.Ansi16, color, 0, 0);
}
