// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace Imtui;

public static class AnsiFormatter
{
    public static string Format(ReadOnlySpan<TermOp> operations)
    {
        StringBuilder builder = new();
        bool wroteCell = false;

        foreach (TermOp operation in operations)
        {
            switch (operation.Kind)
            {
                case TermOpKind.MoveCursor:
                    AppendMoveCursor(builder, operation.Position);
                    break;
                case TermOpKind.Write:
                    AppendWrite(builder, operation.Cell);
                    wroteCell = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(operations),
                        operation.Kind,
                        "Unknown terminal operation kind"
                    );
            }
        }

        if (wroteCell)
        {
            builder.Append("\x1b[0m");
        }

        return builder.ToString();
    }

    private static void AppendMoveCursor(StringBuilder builder, CellPosition position)
    {
        if (position.X < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position.X,
                "Cursor X cannot be negative"
            );
        }

        if (position.Y < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position.Y,
                "Cursor Y cannot be negative"
            );
        }

        builder.Append("\x1b[");
        builder.Append((long)position.Y + 1);
        builder.Append(';');
        builder.Append((long)position.X + 1);
        builder.Append('H');
    }

    private static void AppendWrite(StringBuilder builder, Cell cell)
    {
        if (cell.Glyph.Value == 0)
        {
            builder.Append("\x1b[37;41mX");
            return;
        }

        if (Rune.IsControl(cell.Glyph))
        {
            throw new ArgumentException("Control glyphs cannot be formatted", nameof(cell));
        }

        AppendStyle(builder, cell.Style);
        builder.Append(cell.Glyph.ToString());
    }

    private static void AppendStyle(StringBuilder builder, CellStyle style)
    {
        builder.Append("\x1b[");
        AppendColor(builder, style.Foreground, isBackground: false);
        builder.Append(';');
        AppendColor(builder, style.Background, isBackground: true);
        builder.Append('m');
    }

    private static void AppendColor(StringBuilder builder, Color color, bool isBackground)
    {
        switch (color.Kind)
        {
            case ColorKind.Default:
                builder.Append(isBackground ? 49 : 39);
                break;
            case ColorKind.Ansi:
                AppendAnsiColor(builder, color.AnsiColor, isBackground);
                break;
            case ColorKind.Palette256:
                builder.Append(isBackground ? "48;5;" : "38;5;");
                builder.Append(color.PaletteIndex);
                break;
            case ColorKind.Rgb:
                builder.Append(isBackground ? "48;2;" : "38;2;");
                builder.Append(color.R);
                builder.Append(';');
                builder.Append(color.G);
                builder.Append(';');
                builder.Append(color.B);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color.Kind,
                    "Unknown color kind"
                );
        }
    }

    private static void AppendAnsiColor(StringBuilder builder, AnsiColor color, bool isBackground)
    {
        int colorIndex = (int)color;

        if (colorIndex is < 0 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(color), color, "Unknown ANSI color");
        }

        int baseCode = isBackground ? 40 : 30;

        if (colorIndex >= 8)
        {
            baseCode += 60;
            colorIndex -= 8;
        }

        builder.Append(baseCode + colorIndex);
    }
}
