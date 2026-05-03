// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text;

namespace Imtui;

public static class Renderer
{
    public static string Render(Screen previous, Screen next)
    {
        return Format(Diff(previous, next));
    }

    internal static TermOp[] Diff(Screen previous, Screen next)
    {
        TermOp[] operations = new TermOp[next.Cells.Length * 2];
        int operationIndex = 0;

        for (int cellIndex = 0; cellIndex < next.Cells.Length; cellIndex++)
        {
            operations[operationIndex++] = TermOp.MoveCursor(
                new CellPosition(cellIndex % next.Size.Width, cellIndex / next.Size.Width)
            );
            operations[operationIndex++] = TermOp.Write(next.Cells[cellIndex]);
        }

        return operations;
    }

    internal static Screen Apply(Screen screen, TermOp[] operations)
    {
        Cell[] cells = (Cell[])screen.Cells.Clone();
        Screen result = new Screen(screen.Size, cells);
        CellPosition position = new CellPosition(0, 0);

        foreach (TermOp operation in operations)
        {
            switch (operation.Kind)
            {
                case TermOpKind.MoveCursor:
                    position = operation.Position;
                    break;
                case TermOpKind.Write:
                    result[position] = operation.Cell;
                    break;
            }
        }

        return result;
    }

    internal static string Format(ReadOnlySpan<TermOp> operations)
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
            builder.Append(Ansi.Reset);
        }

        return builder.ToString();
    }

    private static void AppendMoveCursor(StringBuilder builder, CellPosition position)
    {
        Debug.Assert(position.X >= 0 && position.Y >= 0);

        builder.Append(Ansi.Csi);
        builder.Append((long)position.Y + 1);
        builder.Append(';');
        builder.Append((long)position.X + 1);
        builder.Append('H');
    }

    private static void AppendWrite(StringBuilder builder, Cell cell)
    {
        // Hardcoded formatting for NUL
        if (cell.Glyph.Value == 0)
        {
            builder.Append(Ansi.Csi + "37;41mX");
            return;
        }

        if (Rune.IsControl(cell.Glyph))
        {
            throw new InvalidOperationException("Control glyphs cannot be formatted.");
        }

        AppendStyle(builder, cell.Style);
        builder.Append(cell.Glyph.ToString());
    }

    private static void AppendStyle(StringBuilder builder, CellStyle style)
    {
        builder.Append(Ansi.Csi);
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
                builder.Append(isBackground ? Ansi.DefaultBackground : Ansi.DefaultForeground);
                break;
            case ColorKind.Ansi:
                AppendAnsiColor(builder, color.AnsiColor, isBackground);
                break;
            case ColorKind.Palette256:
                builder.Append(
                    isBackground ? Ansi.Palette256Background : Ansi.Palette256Foreground
                );
                builder.Append(';');
                builder.Append(color.PaletteIndex);
                break;
            case ColorKind.Rgb:
                builder.Append(isBackground ? Ansi.RgbBackground : Ansi.RgbForeground);
                builder.Append(';');
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
        Debug.Assert(colorIndex >= 0 && colorIndex <= 15);

        int baseCode = isBackground ? 40 : 30;

        if (colorIndex >= 8)
        {
            baseCode += 60;
            colorIndex -= 8;
        }

        builder.Append(baseCode + colorIndex);
    }
}
