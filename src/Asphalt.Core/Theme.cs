// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

using Asphalt.Rendering;

// A set of semantic colors applied to built-in widgets when an explicit color
// is not supplied at the call site. Themes are immutable records: mutate at
// runtime by assigning a new instance to AsphaltContext.Theme, optionally using
// `with` to derive from an existing theme.
//
//     context.Theme = Theme.Default with { Border = Theme.Default.Border with { Focused = TerminalColor.Red } };
//
// Custom widgets are encouraged to read context.Theme so they pick up the
// active theme alongside the built-ins.
public sealed record Theme
{
    public static Theme Default { get; } = new();

    /// <summary>
    /// Used for things like buttons slider handles, scrollbars.
    /// </summary>
    public FocusableColorPair InteractableSurface { get; init; } =
        new(
            Unfocused: new ColorPair(Foreground: default, Background: TerminalColor.BrightBlack),
            Focused: new ColorPair(Foreground: TerminalColor.White, Background: TerminalColor.Blue)
        );

    /// <summary>
    /// Used for text boxes, etc.
    /// </summary>
    public FocusableColorPair InputSurface { get; init; } =
        new(
            Unfocused: new ColorPair(Foreground: default, Background: TerminalColor.Palette(235)),
            Focused: new ColorPair(Foreground: default, Background: TerminalColor.Palette(238))
        );

    /// <summary>
    /// Used for placeholder text in text inputs.
    /// </summary>
    public TerminalColor PlaceholderText { get; init; } = TerminalColor.Palette(248);

    /// <summary>
    /// Lines and borders.
    /// </summary>
    public FocusableColor Border { get; init; } =
        new(Unfocused: TerminalColor.BrightBlack, Focused: TerminalColor.Green);
}
