// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using Imtui.Rendering;

namespace Imtui;

public static class ScreenExtensions
{
    public static bool IsInBounds(this Screen screen, CellPosition position) =>
        position.X >= 0
        && position.X < screen.Size.Width
        && position.Y >= 0
        && position.Y < screen.Size.Height;
}
