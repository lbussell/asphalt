#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui
#:property PublishAot=false

using Imtui;
using Imtui.Rendering;

const int BoxWidth = 6;
const int BoxHeight = 3;

int x = 2;
int y = 2;

ImtuiContext imtui = new ImtuiContext();

Console.CursorVisible = false;
Console.Clear();

try
{
    while (true)
    {
        imtui.NewFrame();
        imtui.Box(x, y, x + BoxWidth - 1, y + BoxHeight - 1, AnsiColor.Cyan);
        Console.Write(imtui.RenderFrame());
        Console.Out.Flush();

        ConsoleKeyInfo key = Console.ReadKey(intercept: true);

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                y = Math.Max(0, y - 1);
                break;
            case ConsoleKey.DownArrow:
                y = Math.Min(Console.WindowHeight - BoxHeight, y + 1);
                break;
            case ConsoleKey.LeftArrow:
                x = Math.Max(0, x - 1);
                break;
            case ConsoleKey.RightArrow:
                x = Math.Min(Console.WindowWidth - BoxWidth, x + 1);
                break;
            case ConsoleKey.Q:
            case ConsoleKey.Escape:
                return;
        }
    }
}
finally
{
    Console.CursorVisible = true;
    Console.Clear();
}
