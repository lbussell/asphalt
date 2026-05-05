// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui.Rendering;

public readonly record struct Rect(int X, int Y, int Width, int Height)
{
    public Rect(CellPosition position, Size size)
        : this(position.X, position.Y, size.Width, size.Height) { }

    public int Right => X + Width;
    public int Bottom => Y + Height;
}
