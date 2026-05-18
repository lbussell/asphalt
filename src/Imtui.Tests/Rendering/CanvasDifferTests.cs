// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Rendering;

using System.Text;
using CsCheck;
using Imtui.Rendering;
using Imtui.Rendering.Diffing;

[TestClass]
public class CanvasDifferTests
{
    // Apply(previous, Diff(previous, next)) must produce a canvas equal to next.
    [TestMethod]
    public void Correctness_ApplyingDiffProducesNextCanvas()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            TerminalCell[,] applied = ApplyDiff(pair.Previous, pair.Next);
            return CellsEqual(applied, Snapshot(pair.Next));
        });
    }

    // Diffing a canvas against itself must emit no operations at all.
    [TestMethod]
    public void Identity_DiffOfEqualCanvasesEmitsNoOperations()
    {
        CanvasGenerators.AnyCanvas.Sample(canvas =>
        {
            RecordingSink sink = new RecordingSink();
            CanvasDiffer.Diff(canvas, canvas, sink);
            return sink.Operations.Count == 0;
        });
    }

    // Apply(s1, Diff(s1, s2) ++ Diff(s2, s3)) must equal s3. The diffs must
    // compose into a single valid transformation from s1 to s3.
    [TestMethod]
    public void Composition_ConsecutiveDiffsApplyToFinalCanvas()
    {
        CanvasGenerators.CanvasTriple.Sample(triple =>
        {
            CanvasApplierSink applier = new CanvasApplierSink(triple.First);
            CanvasDiffer.Diff(triple.First, triple.Second, applier);
            CanvasDiffer.Diff(triple.Second, triple.Third, applier);
            return CellsEqual(applier.Result, Snapshot(triple.Third));
        });
    }

    // Diff is a pure function of its inputs: two runs must emit the same ops.
    [TestMethod]
    public void Determinism_DiffOfSameInputsEmitsSameOperations()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            RecordingSink first = new RecordingSink();
            RecordingSink second = new RecordingSink();
            CanvasDiffer.Diff(pair.Previous, pair.Next, first);
            CanvasDiffer.Diff(pair.Previous, pair.Next, second);
            return first.Operations.SequenceEqual(second.Operations);
        });
    }

    // The optimized diff must never emit more bytes than the naive baseline.
    [TestMethod]
    public void Cost_OptimizedDiffNeverExceedsNaiveDiff()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            int optimizedBytes = MeasureBytes(pair.Previous, pair.Next, CanvasDiffer.Diff);
            int naiveBytes = MeasureBytes(pair.Previous, pair.Next, CanvasDiffer.DiffNaive);
            return optimizedBytes <= naiveBytes;
        });
    }

    // When N cells differ between two canvases, the optimized diff must emit
    // at most O(N) operations — roughly bounded by a small constant per cell
    // (move, optional colors, write). This catches "full screen redraw" bugs
    // that don't violate correctness but defeat the entire optimization.
    [TestMethod]
    public void Locality_OperationCountScalesWithChangedCells()
    {
        // Per cell: at most 1 MoveTo + 1 ResetSgr + 1 SetBackground + 1 SetForeground + 1 WriteText
        const int maximumOperationsPerChangedCell = 5;

        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            int changedCells = CountChangedCells(pair.Previous, pair.Next);
            RecordingSink sink = new RecordingSink();
            CanvasDiffer.Diff(pair.Previous, pair.Next, sink);
            // +1 accounts for the trailing ResetSgr emitted when the diff
            // ends with non-default colors active.
            return sink.Operations.Count <= changedCells * maximumOperationsPerChangedCell + 1;
        });
    }

    private static TerminalCell[,] ApplyDiff(TerminalCanvas previous, TerminalCanvas next)
    {
        CanvasApplierSink applier = new CanvasApplierSink(previous);
        CanvasDiffer.Diff(previous, next, applier);
        return applier.Result;
    }

    private static int MeasureBytes(
        TerminalCanvas previous,
        TerminalCanvas next,
        Action<TerminalCanvas, TerminalCanvas, IRenderOpsSink> diff
    )
    {
        StringBuilder output = new StringBuilder();
        AnsiSink sink = new AnsiSink(output);
        diff(previous, next, sink);
        return output.Length;
    }

    private static TerminalCell[,] Snapshot(TerminalCanvas canvas)
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

    private static bool CellsEqual(TerminalCell[,] left, TerminalCell[,] right)
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

    private static int CountChangedCells(TerminalCanvas previous, TerminalCanvas next)
    {
        int count = 0;
        for (int y = 0; y < next.Height; y++)
        {
            for (int x = 0; x < next.Width; x++)
            {
                if (previous.GetCell(x, y) != next.GetCell(x, y))
                {
                    count++;
                }
            }
        }
        return count;
    }
}
