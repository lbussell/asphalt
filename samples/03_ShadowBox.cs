#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

ImtuiContext imtui = new();
Dimensions dimensions = new(30, 10);
TerminalCanvas canvas = new(dimensions);
imtui.BeginLayout(dimensions);

using (imtui.ShadowBox())
{
    using (imtui.Panel("ShadowBox Example"))
    {
        imtui.Text("This panel is wrapped in a ShadowBox.");
        imtui.HRule("Shadow");
    }
}

LayoutNode root = imtui.EndLayout();
LayoutRenderer.Render(root, canvas);
canvas.Present(Console.Out);
