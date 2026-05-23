// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

public readonly record struct FocusNavigation(ConsoleKey PreviousKey, ConsoleKey NextKey)
{
    public static FocusNavigation None { get; } = default;
    public static FocusNavigation Vertical { get; } = new(ConsoleKey.UpArrow, ConsoleKey.DownArrow);
    public static FocusNavigation Horizontal { get; } =
        new(ConsoleKey.LeftArrow, ConsoleKey.RightArrow);
    public static FocusNavigation VimVertical { get; } = new(ConsoleKey.K, ConsoleKey.J);
    public static FocusNavigation VimHorizontal { get; } = new(ConsoleKey.H, ConsoleKey.L);

    internal bool TryGetDirection(ConsoleKeyInfo input, out int direction)
    {
        if (this == None)
        {
            direction = 0;
            return false;
        }

        if (input.Key == PreviousKey)
        {
            direction = -1;
            return true;
        }

        if (input.Key == NextKey)
        {
            direction = 1;
            return true;
        }

        direction = 0;
        return false;
    }
}
