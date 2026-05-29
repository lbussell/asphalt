// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

using Asphalt.Rendering;

// A foreground/background color pairing for cells that need both at once,
// e.g. the text drawn on top of a button or input surface.
public readonly record struct ColorPair(TerminalColor Foreground, TerminalColor Background);

// A focus-varying ColorPair: the analogue of FocusableColor for places
// where both foreground and background change together with focus state.
// Built by layering ColorPair inside the same Unfocused/Focused shape
// FocusableColor uses.
public readonly record struct FocusableColorPair(ColorPair Unfocused, ColorPair Focused)
{
    // Returns Focused when `focused` is true, otherwise Unfocused.
    public ColorPair Resolve(bool focused) => focused ? Focused : Unfocused;
}
