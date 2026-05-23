// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests.Rendering;

using System.Text;
using Asphalt.Rendering;
using CsCheck;

[TestClass]
public class DifferentialRendererTests
{
    /// <summary>
    /// Apply(previous, Diff(previous, next)) must produce a canvas equal to
    /// next.
    /// </summary>
    [TestMethod]
    public void Correctness_ApplyingDiffProducesNextCanvas()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            TerminalCell[,] applied = ApplyDiff(pair.Previous, pair.Next);
            return CanvasTestHelpers.CellsEqual(
                applied,
                CanvasTestHelpers.SnapshotCells(pair.Next)
            );
        });
    }

    /// <summary>
    /// Diffing a canvas against itself must emit no operations at all.
    /// </summary>
    [TestMethod]
    public void Identity_DiffOfEqualCanvasesEmitsNoOperations()
    {
        CanvasGenerators.AnyCanvas.Sample(canvas =>
        {
            RecordingSink sink = new RecordingSink();
            DifferentialRenderer.Diff(canvas, canvas, sink);
            return sink.Operations.Count == 0;
        });
    }

    /// <summary>
    /// Apply(s1, Diff(s1, s2) ++ Diff(s2, s3)) must equal s3. The diffs must
    /// compose into a single valid transformation from s1 to s3.
    /// </summary>
    [TestMethod]
    public void Composition_ConsecutiveDiffsApplyToFinalCanvas()
    {
        CanvasGenerators.CanvasTriple.Sample(triple =>
        {
            CanvasApplierSink sink = new CanvasApplierSink(triple.First);
            DifferentialRenderer.Diff(triple.First, triple.Second, sink);
            DifferentialRenderer.Diff(triple.Second, triple.Third, sink);
            return CanvasTestHelpers.CellsEqual(
                sink.Result,
                CanvasTestHelpers.SnapshotCells(triple.Third)
            );
        });
    }

    /// <summary>
    /// Diff is a pure function of its inputs: two runs must emit the same ops.
    /// </summary>
    [TestMethod]
    public void Determinism_DiffOfSameInputsEmitsSameOperations()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            RecordingSink first = new RecordingSink();
            RecordingSink second = new RecordingSink();
            DifferentialRenderer.Diff(pair.Previous, pair.Next, first);
            DifferentialRenderer.Diff(pair.Previous, pair.Next, second);
            return first.Operations.SequenceEqual(second.Operations);
        });
    }

    /// <summary>
    /// The optimized diff must never emit more bytes than the naive baseline.
    /// </summary>
    [TestMethod]
    public void Cost_OptimizedDiffNeverExceedsNaiveDiff()
    {
        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            int optimizedBytes = MeasureBytes(pair.Previous, pair.Next, DifferentialRenderer.Diff);
            int naiveBytes = MeasureBytes(pair.Previous, pair.Next, NaiveRenderer.DiffNaive);
            return optimizedBytes <= naiveBytes;
        });
    }

    /// <summary>
    /// When N cells differ between two canvases, the optimized diff must emit
    /// at most O(N) operations — roughly bounded by a small constant per cell
    /// (move, optional colors, write). This catches "full screen redraw" bugs
    /// that don't violate correctness but defeat the entire optimization.
    /// </summary>
    [TestMethod]
    public void Locality_OperationCountScalesWithChangedCells()
    {
        // Per cell: at most 1 MoveTo + 1 ResetSgr + 1 SetBackground + 1 SetForeground + 1 SetStyle + 1 WriteText
        const int maximumOperationsPerChangedCell = 6;

        CanvasGenerators.CanvasPair.Sample(pair =>
        {
            int changedCells = CountChangedCells(pair.Previous, pair.Next);
            RecordingSink sink = new RecordingSink();
            DifferentialRenderer.Diff(pair.Previous, pair.Next, sink);
            // +1 accounts for the trailing ResetSgr emitted when the diff
            // ends with non-default colors active.
            return sink.Operations.Count <= changedCells * maximumOperationsPerChangedCell + 1;
        });
    }

    private static TerminalCell[,] ApplyDiff(TerminalCanvas previous, TerminalCanvas next)
    {
        CanvasApplierSink sink = new CanvasApplierSink(previous);
        DifferentialRenderer.Diff(previous, next, sink);
        return sink.Result;
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
