#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

Dimensions dimensions = new(72, 18);
ImtuiContext imtui = new();
imtui.BeginLayout(dimensions);

using (
    imtui.Panel(
        "Imtui text layout",
        style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
    )
)
{
    using (
        imtui.Panel("Panel title")
    )
    {
        imtui.Text("Hello from .NET!");
        imtui.HRule("Layout");

        using (
            imtui.Panel(
                title: "Two columns",
                borderStyle: BorderStyle.Round,
                style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() },
                direction: Direction.Horizontal
            )
        )
        {
            imtui.Text(
                "Text widgets report their minimum and preferred widths, then"
                    + " wraps the text after the layout algorithm allocates"
                    + " horizontal space."
            );

            imtui.VRule();

            imtui.Text(
                "This column truncates each source line to its final width.",
                new LayoutStyle { Width = LayoutLength.Fixed(22), Height = LayoutLength.Grow() },
                TextWrappingMode.Truncate,
                backgroundColor: TerminalColor.Palette(52)
            );
        }
    }

    imtui.Text(
        "Force wrap: abcdefghijklmnopqrstuvwxyz",
        new LayoutStyle { Width = LayoutLength.Fixed(20), Height = LayoutLength.Fit() },
        TextWrappingMode.Force,
        backgroundColor: TerminalColor.Palette(17)
    );
}

LayoutNode root = imtui.EndLayout();
TerminalCanvas canvas = new(dimensions);
LayoutRenderer.Render(root, canvas);
new TerminalPresenter(Console.Out).Present(canvas);
