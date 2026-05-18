// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

/// <summary>
/// Produces a debug-visualization variant of a canvas in which every cell that
/// differs from a reference canvas has its background overridden with a
/// highlight color. Used by debug presenters to make per-frame redraw activity
/// visible without altering the differ or the sink.
/// </summary>
public static class CanvasDebugHighlighter
{
    /// <summary>
    /// Returns a new canvas that is a copy of <paramref name="next"/> with the
    /// background of every cell that differs from <paramref name="previous"/>
    /// replaced by <paramref name="highlightBackground"/>. The original input
    /// canvases are not mutated.
    /// </summary>
    /// <remarks>
    /// To get correct per-frame highlight semantics across a stream of frames,
    /// the presenter should diff against the <em>previous highlighted</em>
    /// canvas, not the raw previous canvas — that way the differ's view of
    /// what is on screen stays consistent with what was actually emitted.
    /// </remarks>
    public static TerminalCanvas HighlightChanges(
        TerminalCanvas previous,
        TerminalCanvas next,
        TerminalColor highlightBackground
    )
    {
        if (previous.Dimensions != next.Dimensions)
        {
            throw new ArgumentException(
                "Previous and next canvases must have the same dimensions.",
                nameof(next)
            );
        }

        TerminalCanvas highlighted = new TerminalCanvas(next.Dimensions);
        for (int y = 0; y < next.Height; y++)
        {
            for (int x = 0; x < next.Width; x++)
            {
                TerminalCell nextCell = next.GetCell(x, y);
                TerminalCell previousCell = previous.GetCell(x, y);
                TerminalCell cellToStore =
                    nextCell == previousCell
                        ? nextCell
                        : nextCell with
                        {
                            BackgroundColor = highlightBackground,
                        };
                highlighted.SetCell(x, y, cellToStore);
            }
        }
        return highlighted;
    }
}
