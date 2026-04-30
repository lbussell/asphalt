// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace ImtuiLib;

internal static class ConsoleUtilities
{
    static Size ConsoleSize() => new Size(Console.WindowHeight, Console.WindowWidth);
}

internal readonly record struct Size(int Height, int Width);
