// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

// Internal event types that drive the run loop. All wake-ups - keypresses,
// task completions, future signals - flow through a single Channel<FrameEvent>
// so the loop has one place to await and one place to coalesce.
internal abstract record FrameEvent;

internal sealed record KeyEvent(ConsoleKeyInfo Key) : FrameEvent;

// A signal that the loop should produce another frame, but carrying no
// payload of its own. Used by Task wake-ups and any other source that just
// wants to say "re-evaluate."
internal sealed record WakeEvent : FrameEvent
{
    public static readonly WakeEvent Instance = new WakeEvent();
}
