// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public static class DifferentialRendering
{
    public static TermOp[] Render(Screen previous, Screen next)
    {
        if (previous.Size == next.Size && ReferenceEquals(previous.Cells, next.Cells))
        {
            return [];
        }

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

    public static Screen Apply(Screen screen, TermOp[] operations)
    {
        Cell[] cells = (Cell[])screen.Cells.Clone();
        CellPosition position = new CellPosition(0, 0);

        foreach (TermOp operation in operations)
        {
            switch (operation.Kind)
            {
                case TermOpKind.MoveCursor:
                    position = operation.Position;
                    break;
                case TermOpKind.Write:
                    cells[position.Y * screen.Size.Width + position.X] = operation.Cell;
                    break;
            }
        }

        return new Screen(screen.Size, cells);
    }
}
