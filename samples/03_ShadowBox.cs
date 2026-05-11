#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

Dimensions dimensions = new(52, 14);
ImtuiContext imtui = new();
imtui.BeginLayout(dimensions);

using (
    imtui.Panel(
        style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() },
        padding: new Padding(2, 1),
        backgroundColor: TerminalColor.Rgb(0x24, 0x24, 0x2B)
    )
)
{
    using (
        imtui.ShadowBox(
            style: new LayoutStyle { Width = LayoutLength.Fixed(36), Height = LayoutLength.Fit() }
        )
    )
    {
        using (
            imtui.Panel(
                "ShadowBox",
                style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() }
            )
        )
        {
            imtui.Text("This panel is wrapped in a ShadowBox.");
            imtui.HRule("Shadow");
            imtui.Text("The shadow is reserved by layout down and to the right.");
        }
    }
}

LayoutNode root = imtui.EndLayout();
TerminalCanvas canvas = new(dimensions);
LayoutRenderer.Render(root, canvas);
canvas.Present(Console.Out);
