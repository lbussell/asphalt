// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

using System.Text;

// Turns a sequence of TerminalCanvas frames into ANSI bytes on a TextWriter.
// Owns the cross-frame state — previous canvas snapshot, first-present
// cursor setup — needed to drive a real terminal. All actual rendering
// (cell comparison, SGR emission) is delegated to DifferentialRenderer +
// AnsiSink.
//
// One presenter binds to one output and one presentation mode (altScreen
// vs inline). Construct it once per run loop and call Present(canvas) per
// frame; discard it (or let it go out of scope) when the loop ends.
public sealed class TerminalPresenter(TextWriter output, bool altScreen = false)
{
    private readonly TextWriter _output = output ?? throw new ArgumentNullException(nameof(output));
    private readonly bool _altScreen = altScreen;

    private bool _firstPresent = true;
    private TerminalCanvas? _previous;

    public void Present(TerminalCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        // Nothing to render — also avoids emitting ESC[0A in inline mode
        // and avoids snapshotting a zero-sized canvas as _previous, which
        // would later trip DifferentialRenderer.Diff's dimension-match
        // check on the next non-empty present.
        //
        // ESC[{n}A is "cursor up n lines"; ECMA-48 says a parameter of 0
        // means the default (1), so emitting ESC[0A would move the cursor
        // up one row instead of being a no-op.
        if (canvas.Height == 0 || canvas.Width == 0)
            return;

        StringBuilder sb = new StringBuilder(canvas.Width * canvas.Height * 4);

        // First present after a (re)allocation: pass `previous = null` to
        // DifferentialRenderer.Diff so every cell is emitted, including
        // blanks (as spaces). That clears any stale content within the
        // canvas's footprint left over from a previous, smaller canvas.
        //
        // Caveat: this only clears within the new canvas's footprint. If
        // the previous inline canvas was wider/taller, stale content
        // outside the new bounds remains visible. None of the current
        // Imtui samples shrink between frames, so this is acceptable for
        // now.
        bool firstPresent = _firstPresent;
        TerminalCanvas? diffPrevious =
            firstPresent || _previous is null || _previous.Dimensions != canvas.Dimensions
                ? null
                : _previous;

        if (_altScreen)
        {
            // Alt-screen mode: the canvas always occupies (0,0)..(W-1,H-1)
            // of the screen, so the differ's absolute MoveTo is correct.
            // Go home before the diff so any stray cursor state is reset.
            sb.Append("\x1b[H");
            AnsiSink sink = new AnsiSink(sb, CursorAddressing.Absolute);
            DifferentialRenderer.Diff(diffPrevious, canvas, sink);
        }
        else
        {
            // Inline mode: on first present, reserve vertical space and
            // save the cursor at the canvas origin. Emit a SGR reset
            // before the save so terminals that restore SGR alongside the
            // cursor (e.g. ghostty interpreting ESC[u) start each frame
            // from a known default — otherwise any pre-existing terminal
            // styling would come back on every subsequent ESC[u.
            if (firstPresent)
            {
                for (int i = 0; i < canvas.Height; i++)
                    sb.Append('\n');
                sb.Append("\x1b[").Append(canvas.Height).Append('A'); // back to origin
                sb.Append("\x1b[0m"); // default SGR baked into the saved cursor
                sb.Append("\x1b[s"); // save origin for future MoveTo restores
            }

            AnsiSink sink = new AnsiSink(sb, CursorAddressing.OffsetFromSaved);
            DifferentialRenderer.Diff(diffPrevious, canvas, sink);

            // Park the cursor back at the canvas origin so that if the run
            // loop allocates a fresh canvas next frame (e.g. because the
            // laid-out content changed size), its first present saves the
            // cursor at the same position rather than below this canvas.
            sb.Append("\x1b[u");
        }

        _firstPresent = false;
        _output.Write(sb.ToString());
        _output.Flush();

        // Snapshot for next frame's diff.
        if (_previous is null || _previous.Dimensions != canvas.Dimensions)
            _previous = new TerminalCanvas(canvas.Dimensions);
        _previous.CopyFrom(canvas);
    }
}
