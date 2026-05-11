#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

Dimensions dimensions = new(30, 10);
TerminalCanvas canvas = new(dimensions);
ImtuiContext imtui = new();

bool showText = false;
int counter = 0;

while (true)
{
    ConsoleKeyInfo input = Console.ReadKey(intercept: true);
    imtui.BeginLayout(dimensions, input);

    using (imtui.Panel("Application Example"))
    {
        imtui.Text("This app demonstrates a simple application loop.");
        imtui.Text("Imtui recalculates the layout every time the user presses a key.");

        imtui.HRule("Buttons");
        imtui.Text("You check whether a button is activated at the same time that the button is declared.");

        if (Imtui.Button($"Count: {counter}"))
            counter += 1;

        if (imtui.Button("Toggle"))
            showText = !showText;

        if (showText)
            imtui.Text("Button was pressed!");

        if (imtui.Button("Quit"))
            break;
    }

    LayoutNode root = imtui.EndLayout();
    LayoutRenderer.Render(root, canvas);
    canvas.Present(Console.Out);
}
