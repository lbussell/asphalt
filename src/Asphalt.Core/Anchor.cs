// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

/// <summary>
/// Specifies where an overlay should be positioned relative to the screen.
/// Compose horizontal and vertical edges with bitwise OR — for example,
/// <c>Anchor.Bottom | Anchor.Right</c> pins to the bottom-right corner.
/// Omitting an axis centres on that axis; <see cref="Center"/> centres both.
/// Combining mutually exclusive flags (Top + Bottom or Left + Right) throws.
/// </summary>
[Flags]
public enum Anchor
{
    /// <summary>Centred on both axes.</summary>
    Center = 0,

    /// <summary>Pin to the top edge.</summary>
    Top = 1,

    /// <summary>Pin to the bottom edge.</summary>
    Bottom = 2,

    /// <summary>Pin to the left edge.</summary>
    Left = 4,

    /// <summary>Pin to the right edge.</summary>
    Right = 8,
}
