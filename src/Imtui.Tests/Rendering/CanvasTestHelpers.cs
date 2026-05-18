// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Rendering;

using Imtui.Rendering;

// Shared helpers for comparing canvas state in rendering tests. Cell snapshots
// (TerminalCell[,]) are used instead of TerminalCanvas wrappers so tests can
// capture the pre-mutation state of a canvas and compare it after an operation.
internal static class CanvasTestHelpers
{
    public static TerminalCell[,] SnapshotCells(TerminalCanvas canvas)
    {
        TerminalCell[,] cells = new TerminalCell[canvas.Height, canvas.Width];
        for (int y = 0; y < canvas.Height; y++)
        {
            for (int x = 0; x < canvas.Width; x++)
            {
                cells[y, x] = canvas.GetCell(x, y);
            }
        }
        return cells;
    }

    public static bool CellsEqual(TerminalCell[,] left, TerminalCell[,] right)
    {
        if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1))
        {
            return false;
        }

        for (int y = 0; y < left.GetLength(0); y++)
        {
            for (int x = 0; x < left.GetLength(1); x++)
            {
                if (left[y, x] != right[y, x])
                {
                    return false;
                }
            }
        }
        return true;
    }
}
