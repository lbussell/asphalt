// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using ImtuiLib;

Render();

while (true)
{
    ConsoleKey key = Console.ReadKey(intercept: true).Key;

    if (key is ConsoleKey.Escape)
    {
        break;
    }

    if (key is ConsoleKey.Spacebar)
    {
        Render();
    }
}

static void Render()
{
    Console.Clear();
    Console.WriteLine($"Hello from {nameof(Imtui)}!");
    Console.WriteLine();
    Console.WriteLine(
        $"Console window size: {Console.WindowWidth} x {Console.WindowHeight} (width x height)"
    );
    Console.WriteLine();
    Console.WriteLine("Press Spacebar to refresh. Press Escape to exit.");
}
