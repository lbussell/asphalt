// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

// Inputs to a single frame of an Imtui application. Bundles every external
// signal a frame depends on so that frame production is a pure function of
// (dimensions, input). Tests can construct any FrameInput they want without
// touching the real keyboard or wall clock.
//
// Time is monotonic time since application start (or any chosen epoch).
// Animated widgets compute their state from Time directly.
public readonly record struct FrameInput(ConsoleKeyInfo? Key = null, TimeSpan Time = default);
