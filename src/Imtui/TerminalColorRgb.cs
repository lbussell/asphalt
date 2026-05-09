// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public readonly record struct TerminalColorRgb(byte R, byte G, byte B)
{
    public static TerminalColorRgb Random()
    {
        Random rng = System.Random.Shared;
        return new TerminalColorRgb(
            R: (byte)rng.Next(256),
            G: (byte)rng.Next(256),
            B: (byte)rng.Next(256)
        );
    }
}
