#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui
#:property PublishAot=false

using Imtui;

ImtuiContext imtui = new ImtuiContext();
ImtuiInput input = default;
bool enabled = false;
string name = "";

Console.CursorVisible = false;
Console.Clear();

try
{
    while (true)
    {
        imtui.NewFrame(input: input);
        imtui.Text("Tab/Shift+Tab changes focus. Enter/Space activates.");
        imtui.Checkbox("Enabled", ref enabled);
        imtui.TextField("Name", ref name);

        if (imtui.Button("Quit"))
        {
            return;
        }

        Console.Write(imtui.RenderFrame());
        Console.Out.Flush();

        input = ReadInput(out bool quit);

        if (quit)
        {
            return;
        }
    }
}
finally
{
    Console.CursorVisible = true;
    Console.Clear();
}

static ImtuiInput ReadInput(out bool quit)
{
    if (Console.IsInputRedirected)
    {
        int input = Console.In.Read();

        if (input < 0)
        {
            quit = true;
            return default;
        }

        char character = (char)input;
        quit = character is '\u001b' or 'q' or 'Q';
        return ImtuiInput.FromCharacter(character);
    }

    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
    quit = key.Key is ConsoleKey.Escape or ConsoleKey.Q;

    return ImtuiInput.FromConsoleKeyInfo(key);
}
