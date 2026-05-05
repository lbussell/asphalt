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

        return FromCharacter((char)input, out quit);
    }

    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
    quit = key.Key is ConsoleKey.Escape or ConsoleKey.Q;

    if ((key.Modifiers & ConsoleModifiers.Shift) != 0 && key.Key == ConsoleKey.Tab)
    {
        return new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.ShiftTab));
    }

    return key.Key switch
    {
        ConsoleKey.Tab => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Tab)),
        ConsoleKey.Enter => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter)),
        ConsoleKey.Spacebar => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Space)),
        ConsoleKey.LeftArrow => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.LeftArrow)),
        ConsoleKey.RightArrow => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.RightArrow)),
        ConsoleKey.Backspace => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Backspace)),
        ConsoleKey.Delete => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Delete)),
        _ when !char.IsControl(key.KeyChar) => FromCharacter(key.KeyChar, out quit),
        _ => default,
    };
}

static ImtuiInput FromCharacter(char character, out bool quit)
{
    quit = character is '\u001b' or 'q' or 'Q';

    return character switch
    {
        '\t' => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Tab)),
        '\r' or '\n' => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter)),
        ' ' => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Space)),
        '\b' => new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Backspace)),
        _ when !char.IsControl(character) =>
            new ImtuiInput(ImtuiInputEvent.FromCharacter(character)),
        _ => default,
    };
}
