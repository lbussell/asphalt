// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Rendering;

// SGR text attributes applied to a cell, in addition to its foreground and
// background colors. Flags compose with `|`. Add new flags as widgets need
// them — each new flag requires only an enum member and an emit case in
// AnsiSink.
[Flags]
public enum TextStyle : byte
{
    None = 0,
    Reverse = 1 << 0,
    Bold = 1 << 1,
}
