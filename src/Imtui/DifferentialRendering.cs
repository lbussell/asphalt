// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public static class DifferentialRendering
{
    public static TermOp[] Render(Screen previous, Screen next)
    {
        return [];
    }

    public static Screen Apply(Screen screen, IEnumerable<TermOp> operations)
    {
        return screen;
    }
}
