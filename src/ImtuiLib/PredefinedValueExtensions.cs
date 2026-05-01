// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace ImtuiLib;

public static class PredefinedValueExtensions
{
    extension(Cell cell)
    {
        public static Cell Empty => new(new Rune(' '), default);
    }
}
