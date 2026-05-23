// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests.Rendering;

using System.Runtime.InteropServices;
using Asphalt.Rendering;

// Maximally chatty baseline renderer. For every cell in the next canvas,
// emits MoveTo + ResetSgr + SetBackground + SetForeground + (SetStyle) +
// WriteText with no state tracking and no comparison against the previous
// canvas. Used as the upper bound in cost-bound property tests of
// DifferentialRenderer.Diff — the optimized differ must never emit more
// bytes than this baseline.
internal static class NaiveRenderer
{
    public static void DiffNaive(TerminalCanvas previous, TerminalCanvas next, IRenderOpsSink sink)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(sink);

        if (previous.Dimensions != next.Dimensions)
            throw new ArgumentException(
                "Previous and next canvases must have the same dimensions.",
                nameof(next)
            );

        for (int y = 0; y < next.Height; y++)
        {
            for (int x = 0; x < next.Width; x++)
            {
                TerminalCell nextCell = next.GetCell(x, y);
                sink.MoveTo(x, y);
                sink.ResetSgr();
                sink.SetBackground(nextCell.BackgroundColor);
                sink.SetForeground(nextCell.ForegroundColor);
                if (nextCell.Style != TextStyle.None)
                    sink.SetStyle(added: nextCell.Style, removed: TextStyle.None);
                char character = nextCell.CharacterOrSpace;
                sink.WriteText(MemoryMarshal.CreateReadOnlySpan(ref character, 1));
            }
        }

        // Match Diff's contract: leave the sink in default SGR state so that
        // a subsequent Diff call can safely assume currentForeground/Background/Style
        // are default.
        sink.ResetSgr();
    }
}
