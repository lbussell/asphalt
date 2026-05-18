// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using Imtui.Rendering;

// A set of semantic colors applied to built-in widgets when an explicit color
// is not supplied at the call site. Themes are immutable records: mutate at
// runtime by assigning a new instance to ImtuiContext.Theme, optionally using
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
    public TerminalColor Surface { get; init; } = TerminalColor.Rgb(0x3F, 0x3F, 0x48);

    // Focused variant of Surface, also used as the title-bar background on
    // titled panels.
    public TerminalColor SurfaceFocused { get; init; } = TerminalColor.Rgb(0x29, 0x4A, 0x7A);

    // Background fill for panel bodies.
    public TerminalColor PanelBackground { get; init; } = TerminalColor.Rgb(0x0F, 0x0F, 0x0F);

    // Subtle foreground used for placeholder text and slider handles at rest.
    public TerminalColor Placeholder { get; init; } = TerminalColor.Rgb(0x80, 0x80, 0x80);

    // Lines and borders (HRule, VRule, BorderPanel borders).
    public TerminalColor Border { get; init; } = TerminalColor.Rgb(0x3F, 0x3F, 0x48);

    // Accent color used for emphasis (e.g. the focused slider handle).
    public TerminalColor Accent { get; init; } = TerminalColor.Rgb(0x4A, 0x90, 0xE2);

    // Drop-shadow fill color.
    public TerminalColor Shadow { get; init; } = TerminalColor.Rgb(0, 0, 0);
}
