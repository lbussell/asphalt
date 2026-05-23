// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests.Rendering;

using Asphalt.Rendering;
using CsCheck;

[TestClass]
public class CanvasDebugHighlighterTests
{
    private static readonly TerminalColor s_highlightBackground = TerminalColor.Red;

    // Cells that differ between previous and next must have their background
    // replaced by the highlight color; everything else about the cell is kept.
    [TestMethod]
    public void ChangedCells_AreHighlighted_AndKeepCharacterAndForeground()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            TerminalCanvas highlighted = CanvasDebugHighlighter.HighlightChanges(
                pair.Previous,
                pair.Next,
                s_highlightBackground
            );

            for (int y = 0; y < pair.Next.Height; y++)
            {
                for (int x = 0; x < pair.Next.Width; x++)
                {
                    TerminalCell nextCell = pair.Next.GetCell(x, y);
                    TerminalCell previousCell = pair.Previous.GetCell(x, y);
                    TerminalCell highlightedCell = highlighted.GetCell(x, y);

                    if (nextCell == previousCell)
                    {
                        if (highlightedCell != nextCell)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (highlightedCell.Character != nextCell.Character)
                        {
                            return false;
                        }
                        if (highlightedCell.ForegroundColor != nextCell.ForegroundColor)
                        {
                            return false;
                        }
                        if (highlightedCell.BackgroundColor != s_highlightBackground)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        });
    }

    // Highlighting against an identical canvas must produce a canvas equal to
    // the input — there are no changes to mark.
    [TestMethod]
    public void HighlightingEqualCanvases_ProducesAnEquivalentCanvas()
    {
        CanvasGenerators.AnyCanvas.Sample(canvas =>
        {
            TerminalCanvas highlighted = CanvasDebugHighlighter.HighlightChanges(
                canvas,
                canvas,
                s_highlightBackground
            );

            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    if (highlighted.GetCell(x, y) != canvas.GetCell(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        });
    }

    // The function must not mutate either input canvas.
    [TestMethod]
    public void Highlighting_DoesNotMutateInputCanvases()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            TerminalCell[,] previousBefore = CanvasTestHelpers.SnapshotCells(pair.Previous);
            TerminalCell[,] nextBefore = CanvasTestHelpers.SnapshotCells(pair.Next);

            CanvasDebugHighlighter.HighlightChanges(
                pair.Previous,
                pair.Next,
                s_highlightBackground
            );

            return CanvasTestHelpers.CellsEqual(
                    previousBefore,
                    CanvasTestHelpers.SnapshotCells(pair.Previous)
                )
                && CanvasTestHelpers.CellsEqual(
                    nextBefore,
                    CanvasTestHelpers.SnapshotCells(pair.Next)
                );
        });
    }
}
