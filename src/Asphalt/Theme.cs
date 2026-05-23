// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

using Asphalt.Rendering;

// A set of semantic colors applied to built-in widgets when an explicit color
// is not supplied at the call site. Themes are immutable records: mutate at
// runtime by assigning a new instance to AsphaltContext.Theme, optionally using
// `with` to derive from an existing theme.
//
//     context.Theme = Theme.Default with { Accent = TerminalColor.Red };
//
// Custom widgets are encouraged to read context.Theme so they pick up the
// active theme alongside the built-ins.
public sealed record Theme
{
    public static Theme Default { get; } = new();

    // Neutral interactive surface (button, input text, scalar input
    // backgrounds; slider bar).
    public TerminalColor Surface { get; init; } = TerminalColor.BrightBlack;

    // Focused variant of Surface, also used as the title-bar background on
    // titled panels.
    public TerminalColor SurfaceFocused { get; init; } = TerminalColor.Blue;

    // Subtle foreground used for placeholder text and slider handles at rest.
    public TerminalColor Placeholder { get; init; } = TerminalColor.BrightBlack;

    // Lines and borders (HRule, VRule, Panel borders).
    public TerminalColor Border { get; init; } = TerminalColor.BrightBlack;

    // Border color used when a Panel is on the focused scope chain.
    public TerminalColor BorderFocused { get; init; } = TerminalColor.Green;

    // Accent color used for emphasis (e.g. the focused slider handle).
    public TerminalColor Accent { get; init; } = TerminalColor.Cyan;
}
