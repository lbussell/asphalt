#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

Dimensions dimensions = new(72, 18);
AsphaltContext asphalt = new();
asphalt.BeginLayout(dimensions);

using (
    asphalt.Panel(
        "Asphalt text layout",
        style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
    )
)
{
    using (
        asphalt.Panel("Panel title")
    )
    {
        asphalt.Text("Hello from .NET!");
        asphalt.HRule("Layout");

        using (
            asphalt.Panel(
                title: "Two columns",
                borderStyle: BorderStyle.Round,
                style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() },
                direction: Direction.Horizontal
            )
        )
        {
            asphalt.Text(
                "Text widgets report their minimum and preferred widths, then"
                    + " wraps the text after the layout algorithm allocates"
                    + " horizontal space."
            );

            asphalt.VRule();

            asphalt.Text(
                "This column truncates each source line to its final width.",
                new LayoutStyle { Width = LayoutLength.Fixed(22), Height = LayoutLength.Grow() },
                TextWrappingMode.Truncate,
                backgroundColor: TerminalColor.Palette(52)
            );
        }
    }

    asphalt.Text(
        "Force wrap: abcdefghijklmnopqrstuvwxyz",
        new LayoutStyle { Width = LayoutLength.Fixed(20), Height = LayoutLength.Fit() },
        TextWrappingMode.Force,
        backgroundColor: TerminalColor.Palette(17)
    );
}

LayoutNode root = asphalt.EndLayout();
TerminalCanvas canvas = new(dimensions);
LayoutRenderer.Render(root, canvas);
new TerminalPresenter(Console.Out).Present(canvas);
