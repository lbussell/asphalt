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
    //
    // Passing null for `previous` treats every cell as different — useful for
    // a full repaint after a (re)allocation when stale terminal content may
    // exist within the canvas's footprint. The SGR-tracking optimization
    // still applies, so the full repaint emits the minimum-byte encoding.
    public static void Diff(TerminalCanvas? previous, TerminalCanvas next, IRenderOpsSink sink)
    {
        if (previous is not null && previous.Dimensions != next.Dimensions)
        {
            throw new ArgumentException(
                "Previous and next canvases must have the same dimensions.",
                nameof(next)
            );
        }

        TerminalColor currentForeground = default;
        TerminalColor currentBackground = default;
        TextStyle currentStyle = TextStyle.None;

        for (int y = 0; y < next.Height; y++)
        {
            for (int x = 0; x < next.Width; x++)
            {
                TerminalCell nextCell = next.GetCell(x, y);

                if (previous is not null && previous.GetCell(x, y) == nextCell)
                {
                    continue;
                }

                sink.MoveTo(x, y);

                EmitStyleTransition(
                    sink,
                    nextCell,
                    ref currentForeground,
                    ref currentBackground,
                    ref currentStyle
                );

                char character = nextCell.CharacterOrSpace;
                sink.WriteText(MemoryMarshal.CreateReadOnlySpan(ref character, 1));
            }
        }

        // Leave the sink in default SGR state so that consecutive diffs compose
        // correctly: the next Diff call always assumes a fresh default state.
        if (
            !IsDefault(currentForeground)
            || !IsDefault(currentBackground)
            || currentStyle != TextStyle.None
        )
        {
            sink.ResetSgr();
        }
    }

    private static void EmitStyleTransition(
        IRenderOpsSink sink,
        TerminalCell nextCell,
        ref TerminalColor currentForeground,
        ref TerminalColor currentBackground,
        ref TextStyle currentStyle
    )
    {
        // When transitioning to a default color, a single ResetSgr is shorter
        // than emitting explicit "default fg" / "default bg" SGR codes.
        // ResetSgr also clears text-style attributes, so we track that here.
        bool needsReset =
            (IsDefault(nextCell.ForegroundColor) && !IsDefault(currentForeground))
            || (IsDefault(nextCell.BackgroundColor) && !IsDefault(currentBackground));

        if (needsReset)
        {
            sink.ResetSgr();
            currentForeground = default;
            currentBackground = default;
            currentStyle = TextStyle.None;
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

        if (nextCell.Style != currentStyle)
        {
            TextStyle added = nextCell.Style & ~currentStyle;
            TextStyle removed = currentStyle & ~nextCell.Style;
            sink.SetStyle(added, removed);
            currentStyle = nextCell.Style;
        }
    }

    private static bool IsDefault(TerminalColor color) => color.Kind == TerminalColorKind.Default;
}
