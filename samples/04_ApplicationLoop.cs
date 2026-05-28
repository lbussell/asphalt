#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

Dimensions dimensions = new(40, 15);
TerminalCanvas canvas = new(dimensions);
TerminalPresenter presenter = new(Console.Out);
AsphaltContext asphalt = new();

bool showText = false;
int counter = 0;

ConsoleKeyInfo? key = null;
System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

while (true)
{
    FrameInput frameInput = key is { } singleKey
        ? new FrameInput(singleKey, stopwatch.Elapsed)
        : new FrameInput(Time: stopwatch.Elapsed);
    asphalt.BeginLayout(dimensions, frameInput);

    using (asphalt.Panel("Application Example", style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }))
    {
        asphalt.Text("This app demonstrates a simple application loop.");
        asphalt.Text("Asphalt recalculates the layout every time the user presses a key.");

        asphalt.HRule("Buttons");
        asphalt.Text("You check whether a button is activated at the same time that the button is declared.");

        if (asphalt.Button($"Increment"))
            counter += 1;

        asphalt.Text($"Count: {counter}");

        if (asphalt.Button("Toggle"))
            showText = !showText;

        if (showText)
            asphalt.Text("Button was pressed!");

        if (asphalt.Button("Quit"))
            break;
    }

    LayoutNode root = asphalt.EndLayout();
    LayoutRenderer.Render(root, canvas);
    presenter.Present(canvas);
    asphalt.EndFrame();

    // Capture input for the next frame.
    key = Console.ReadKey(intercept: true);
}
