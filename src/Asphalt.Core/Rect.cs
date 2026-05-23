// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

public readonly record struct Rect(Position Position, Dimensions Dimensions)
{
    public Rect(int x, int y, int width, int height)
        : this(new Position(x, y), new Dimensions(width, height)) { }
}
