// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

/// <summary>
/// Constants and helpers for ANSI escape sequences.
/// </summary>
public static class Ansi
{
    /// <summary>
    /// Control Sequence Introducer — the two-byte prefix for all CSI escape sequences.
    /// </summary>
    public const string Csi = "\x1b[";

    public const string Reset = Csi + "0m";

    public const string DefaultForeground = "39";
    public const string DefaultBackground = "49";

    public const string Palette256Foreground = "38;5";
    public const string Palette256Background = "48;5";
    public const string RgbForeground = "38;2";
    public const string RgbBackground = "48;2";

    /// <summary>
    /// Formats a CUP (Cursor Position) escape sequence using 1-based coordinates.
    /// </summary>
    public static string CursorPosition(int row, int col) => $"{Csi}{row};{col}H";

    /// <summary>
    /// Formats an SGR (Select Graphic Rendition) escape sequence.
    /// </summary>
    public static string Sgr(params IEnumerable<string> parameters) =>
        $"{Csi}{string.Join(';', parameters)}m";
}
