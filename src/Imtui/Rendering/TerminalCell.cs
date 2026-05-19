// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

internal readonly record struct TerminalCell(
    char Character,
    TerminalColor ForegroundColor,
    TerminalColor BackgroundColor,
    TextStyle Style = TextStyle.None
)
{
    public char CharacterOrSpace => Character == default ? ' ' : Character;
}
