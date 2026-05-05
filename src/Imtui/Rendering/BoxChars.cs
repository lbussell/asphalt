// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;

namespace Imtui.Rendering;

public readonly record struct BoxChars(
    Rune TopLeft,
    Rune TopRight,
    Rune BottomLeft,
    Rune BottomRight,
    Rune Horizontal,
    Rune Vertical
)
{
    public static BoxChars Light =>
        new(
            TopLeft: new Rune('┌'),
            TopRight: new Rune('┐'),
            BottomLeft: new Rune('└'),
            BottomRight: new Rune('┘'),
            Horizontal: new Rune('─'),
            Vertical: new Rune('│')
        );
}
