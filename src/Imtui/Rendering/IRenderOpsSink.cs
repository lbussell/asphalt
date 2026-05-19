// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

// A dumb translator from terminal rendering operations to bytes (or to a
// recording, in tests). Implementations must not re-order, deduplicate, or
// otherwise optimize the call stream. All optimization lives in DifferentialRenderer
// so that what the differ emits is exactly what reaches the terminal.
public interface IRenderOpsSink
{
    void MoveTo(int column, int row);

    void SetForeground(TerminalColor color);

    void SetBackground(TerminalColor color);

    // Toggles the given SGR text attributes. Both arguments are flag sets:
    // `added` is enabled (e.g. SGR 7 for Reverse), `removed` is disabled
    // (e.g. SGR 27). The sink emits the literal codes; it does not track
    // which flags are currently on — that bookkeeping lives in
    // DifferentialRenderer.
    void SetStyle(TextStyle added, TextStyle removed);

    void ResetSgr();

    void WriteText(ReadOnlySpan<char> text);
}
