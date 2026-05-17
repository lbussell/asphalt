// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

// A single-character animated spinner. The displayed glyph is a pure
// function of the current frame's Time and the chosen frame duration —
// there is no per-spinner state. Multiple spinners with the same frame
// duration are phase-locked because they all index into their glyph array
// using the same time value.
//
// Each call requests a follow-up redraw at the next glyph boundary so the
// run loop wakes up exactly when the next frame is due. Several spinners
// in one frame still result in a single wake-up because requests aggregate
// by minimum on the ImtuiContext.
public static class SpinnerExtensions
{
    // Braille-dot rotation, identical to the cli-spinners "dots" preset.
    // Reads as a single rotating glyph at small sizes.
    public static readonly char[] DefaultGlyphs =
    [
        '⠋',
        '⠙',
        '⠹',
        '⠸',
        '⠼',
        '⠴',
        '⠦',
        '⠧',
        '⠇',
        '⠏',
    ];

    public static readonly TimeSpan DefaultFrameDuration = TimeSpan.FromMilliseconds(80);

    extension(ImtuiContext context)
    {
        public void Spinner(
            LayoutStyle? style = null,
            TerminalColor foregroundColor = default,
            IReadOnlyList<char>? glyphs = null,
            TimeSpan frameDuration = default
        )
        {
            IReadOnlyList<char> effectiveGlyphs = glyphs ?? DefaultGlyphs;

            if (effectiveGlyphs.Count == 0)
                throw new ArgumentException("Spinner requires at least one glyph.", nameof(glyphs));

            TimeSpan effectiveDuration =
                frameDuration == default ? DefaultFrameDuration : frameDuration;

            if (effectiveDuration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(frameDuration),
                    "Spinner frame duration must be positive."
                );

            double frameMs = effectiveDuration.TotalMilliseconds;
            double elapsedMs = context.Time.TotalMilliseconds;

            // Compute the current glyph as a pure function of time. Use
            // floor-style modulo for negative inputs so the index is always
            // valid (defensive — Time is monotonic in practice).
            long frameNumber = (long)Math.Floor(elapsedMs / frameMs);
            int index = (int)(
                ((frameNumber % effectiveGlyphs.Count) + effectiveGlyphs.Count)
                % effectiveGlyphs.Count
            );

            // Phase-lock: ask for the next redraw at the next glyph boundary
            // (not "frameMs from now"). This keeps every spinner aligned and
            // avoids cumulative drift across frames.
            double phaseMs = elapsedMs - (frameNumber * frameMs);
            TimeSpan delay = TimeSpan.FromMilliseconds(frameMs - phaseMs);
            context.RequestRedrawIn(delay);

            context.Text(
                effectiveGlyphs[index].ToString(),
                style: style,
                wrappingMode: TextWrappingMode.Truncate,
                foregroundColor: foregroundColor
            );
        }
    }
}
