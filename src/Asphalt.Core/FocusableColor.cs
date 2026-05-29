// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

using Asphalt.Rendering;

// A pair of colors that vary by focus state. Used by themes to express
// "this surface looks one way when its widget is focused, another when
// it is not" without forcing widgets to read two separate properties
// and pick the right one at every call site.
public readonly record struct FocusableColor(TerminalColor Unfocused, TerminalColor Focused)
{
    // Returns Focused when `focused` is true, otherwise Unfocused.
    public TerminalColor Resolve(bool focused) => focused ? Focused : Unfocused;
}
