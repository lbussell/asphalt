#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

Dimensions dimensions = new(40, 15);
TerminalCanvas canvas = new(dimensions);
ImtuiContext imtui = new();

bool showText = false;
int counter = 0;

ConsoleKeyInfo? key = null;
System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

while (true)
{
    imtui.BeginLayout(dimensions, new FrameInput(Key: key, Time: stopwatch.Elapsed));

    using (imtui.Panel("Application Example", style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }))
    {
        imtui.Text("This app demonstrates a simple application loop.");
        imtui.Text("Imtui recalculates the layout every time the user presses a key.");

        imtui.HRule("Buttons");
        imtui.Text("You check whether a button is activated at the same time that the button is declared.");

        if (imtui.Button($"Increment"))
            counter += 1;

        imtui.Text($"Count: {counter}");

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
    imtui.EndFrame();

    // Capture input for the next frame.
    key = Console.ReadKey(intercept: true);
}
