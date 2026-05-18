// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

using System.Runtime.InteropServices;

// Computes the minimal stream of render operations that transforms the
// previous canvas into the next canvas, and emits them to the given sink. All
// output optimization (skipping unchanged cells, tracking SGR state, eliding
// redundant moves, etc.) lives here so that sinks remain dumb translators.
public static class DifferentialRenderer
{
    // Optimized diff. Emits operations only for cells that actually changed
    // and tracks the cursor's SGR state to avoid re-emitting unchanged
    // foreground/background colors.
    public static void Diff(TerminalCanvas previous, TerminalCanvas next, IRenderOpsSink sink)
    {
        if (previous.Dimensions != next.Dimensions)
        {
            throw new ArgumentException(
                "Previous and next canvases must have the same dimensions.",
                nameof(next)
            );
        }

        TerminalColor currentForeground = default;
        TerminalColor currentBackground = default;

        for (int y = 0; y < next.Height; y++)
        {
            for (int x = 0; x < next.Width; x++)
            {
                TerminalCell previousCell = previous.GetCell(x, y);
                TerminalCell nextCell = next.GetCell(x, y);

                if (previousCell == nextCell)
                {
                    continue;
                }

                sink.MoveTo(x, y);

                EmitStyleTransition(sink, nextCell, ref currentForeground, ref currentBackground);

                char character = nextCell.CharacterOrSpace;
                sink.WriteText(MemoryMarshal.CreateReadOnlySpan(ref character, 1));
            }
        }

        // Leave the sink in default SGR state so that consecutive diffs compose
        // correctly: the next Diff call always assumes a fresh default state.
        if (!IsDefault(currentForeground) || !IsDefault(currentBackground))
        {
            sink.ResetSgr();
        }
    }

    // Maximally chatty baseline. For every cell in the next canvas, emits
    // MoveTo + ResetSgr + SetBackground + SetForeground + WriteText with no
    // state tracking and no comparison against the previous canvas. Used as
    // the upper bound in cost-bound property tests.
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
                char character = nextCell.CharacterOrSpace;
                sink.WriteText(MemoryMarshal.CreateReadOnlySpan(ref character, 1));
            }
        }
    }

    private static void EmitStyleTransition(
        IRenderOpsSink sink,
        TerminalCell nextCell,
        ref TerminalColor currentForeground,
        ref TerminalColor currentBackground
    )
    {
        // When transitioning to a default color, a single ResetSgr is shorter
        // than emitting explicit "default fg" / "default bg" SGR codes.
        bool needsReset =
            (IsDefault(nextCell.ForegroundColor) && !IsDefault(currentForeground))
            || (IsDefault(nextCell.BackgroundColor) && !IsDefault(currentBackground));

        if (needsReset)
        {
            sink.ResetSgr();
            currentForeground = default;
            currentBackground = default;
        }

        if (nextCell.BackgroundColor != currentBackground)
        {
            sink.SetBackground(nextCell.BackgroundColor);
            currentBackground = nextCell.BackgroundColor;
        }

        if (nextCell.ForegroundColor != currentForeground)
        {
            sink.SetForeground(nextCell.ForegroundColor);
            currentForeground = nextCell.ForegroundColor;
        }
    }

    private static bool IsDefault(TerminalColor color) => color.Kind == TerminalColorKind.Default;
}
