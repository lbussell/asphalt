#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui
#:property PublishAot=false

using Imtui;
using Imtui.Widgets;

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

        imtui.Text("Containers demo. Tab/Shift+Tab moves focus, q/Esc quits.");

        imtui.Panel("Settings", panel =>
        {
            panel.Checkbox("Enabled", ref enabled);
            panel.TextField("Name", ref name);
        });

        imtui.Panel("Padding showcase", panel =>
        {
            panel.Padding(horizontal: 2, vertical: 1, padded =>
            {
                padded.Text("Children inside Padding render shifted.");
                padded.Text("Padding inherits the panel's colors.");
            });
        });

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
