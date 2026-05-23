// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

// Inputs to a single frame of an Asphalt application. Bundles every external
// signal a frame depends on so that frame production is a pure function of
// (dimensions, input). Tests can construct any FrameInput they want without
// touching the real keyboard or wall clock.
//
// Keys is the ordered sequence of keypresses observed since the previous
// frame. Multiple keys per frame are common when the user types faster than
// the application renders; widgets see each in the order it was pressed.
//
// Time is monotonic time since application start (or any chosen epoch).
// Animated widgets compute their state from Time directly.
public readonly record struct FrameInput(
    IReadOnlyList<ConsoleKeyInfo>? Keys = null,
    TimeSpan Time = default
)
{
    // Convenience constructor for the common single-key case.
    public FrameInput(ConsoleKeyInfo key, TimeSpan time = default)
        : this(new[] { key }, time) { }
}
