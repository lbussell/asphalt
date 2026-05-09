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
    using (imtui.BorderPanel(BorderStyle.Square, new LayoutStyle { Width = LayoutLength.Fixed(18) }))
    {
        imtui.Text("Fixed width BorderPanel");
        imtui.Text("");
    }

    using (
        imtui.BorderPanel(
            BorderStyle.Ascii,
            new LayoutStyle { ChildGap = 1 },
            direction: Direction.Vertical
        )
    )
    {
        imtui.Text("Text can live inside bordered containers.");
        using (imtui.BorderPanel(style: new LayoutStyle { Height = LayoutLength.Fixed(5) }))
        {
            imtui.Text("Nested panel");
        }
        imtui.Text("The middle panel grows to fill.");
    }

    using (imtui.BorderPanel(BorderStyle.Round, new LayoutStyle { Width = LayoutLength.Fixed(16) }))
    {
        imtui.Text("Right fixed");
    }
}

// Build the layout - calculates precise dimensions for all widgets/elements
// according to constraints.
LayoutNode layout = imtui.Build(dimensions);

// Ask each widget to draw itself given its calculated dimensions.
Renderer.Render(layout, canvas);
// Render the app to the terminal.
canvas.Present(Console.Out);
