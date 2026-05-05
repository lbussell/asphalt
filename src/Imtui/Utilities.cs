// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui;

internal static class Utilities
{
    /// <param name="index"></param>
    /// <param name="min">Inclusive</param>
    /// <param name="max">Inclusive</param>
    internal static int Wrap(int index, int min, int max)
    {
        long range = (long)max - min + 1;
        long offset = ((long)index - min) % range;
        if (offset < 0)
            offset += range;
        return (int)(min + offset);
    }
}
