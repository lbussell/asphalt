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
using (imtui.Container(Direction.Horizontal, new LayoutStyle { ChildGap = 1 }))
{
    imtui.BorderPanel(BorderStyle.Square, new LayoutStyle { Width = LayoutLength.Fixed(18) });

    using (
        imtui.Container(
            Direction.Vertical,
            new LayoutStyle { Padding = new Padding(1), ChildGap = 1 }
        )
    )
    {
        imtui.BorderPanel(BorderStyle.Ascii, new LayoutStyle { Height = LayoutLength.Fixed(5) });
        imtui.BorderPanel();
        imtui.BorderPanel(BorderStyle.Square, new LayoutStyle { Height = LayoutLength.Fixed(3) });
    }

    imtui.BorderPanel(BorderStyle.Round, new LayoutStyle { Width = LayoutLength.Fixed(16) });
}

// Build the layout - calculates precise dimensions for all widgets/elements
// according to constraints.
LayoutNode layout = imtui.Build(dimensions);

// Ask each widget to draw itself given its calculated dimensions.
Renderer.Render(layout, canvas);
// Render the app to the terminal.
canvas.Present(Console.Out);
