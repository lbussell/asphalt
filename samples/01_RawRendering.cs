#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;

// Setup
Dimensions dimensions = new Dimensions(80, 20);
TerminalCanvas canvas = new TerminalCanvas(dimensions);
ImtuiContext imtui = new ImtuiContext();

// Layout the app, one time.
using (imtui.Container(Direction.Horizontal))
{
    imtui.BorderPanel();
    using (imtui.Container(Direction.Vertical))
    {
        imtui.BorderPanel(BorderStyle.Square);
        imtui.BorderPanel(BorderStyle.Ascii);
    }
    imtui.BorderPanel(BorderStyle.Round);
}

// Build the layout - calculates precise dimensions for all widgets/elements
// according to constraints.
LayoutNode layout = imtui.Build(dimensions);

// Ask each widget to draw itself given its calculated dimensions.
Renderer.Render(layout, canvas);
// Render the app to the terminal.
canvas.Present(Console.Out);
