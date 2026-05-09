// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public readonly record struct BorderStyle(
    char TopLeft,
    char TopRight,
    char BottomLeft,
    char BottomRight,
    char Horizontal,
    char Vertical
)
{
    public static BorderStyle Square { get; } = new('┌', '┐', '└', '┘', '─', '│');

    public static BorderStyle Round { get; } = new('╭', '╮', '╰', '╯', '─', '│');

    public static BorderStyle Ascii { get; } = new('+', '+', '+', '+', '-', '|');
}
