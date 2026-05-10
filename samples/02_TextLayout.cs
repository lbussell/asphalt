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

imtui.OpenElement(
    style: new LayoutStyle
    {
        Direction = Direction.Vertical,
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Grow(),
        Padding = new Padding(1),
        ChildGap = 1,
    }
);

imtui.Text(
    "Imtui text layout",
    new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() },
    foregroundColor: TerminalColor.BrightWhite,
    backgroundColor: TerminalColor.Blue
);

using (imtui.HStack(padding: new Padding(1), gap: 2))
{
    imtui.Text(
        "Text widgets measure their preferred size, then lay themselves out once the layout algorithm assigns a final width.",
        new LayoutStyle { Width = LayoutLength.Grow(12), Height = LayoutLength.Fit() },
        foregroundColor: TerminalColor.White,
        backgroundColor: TerminalColor.Palette(24)
    );

    imtui.Text(
        "This column truncates each source line to its final width.",
        new LayoutStyle { Width = LayoutLength.Fixed(22), Height = LayoutLength.Fit() },
        TextWrappingMode.Truncate,
        foregroundColor: TerminalColor.BrightYellow,
        backgroundColor: TerminalColor.Palette(52)
    );
}

imtui.Text(
    "Force wrap: abcdefghijklmnopqrstuvwxyz",
    new LayoutStyle { Width = LayoutLength.Fixed(20), Height = LayoutLength.Fit() },
    TextWrappingMode.Force,
    foregroundColor: TerminalColor.BrightCyan,
    backgroundColor: TerminalColor.Palette(17)
);

imtui.CloseElement();

LayoutNode root = imtui.EndLayout();
TerminalCanvas canvas = new(dimensions);
LayoutRenderer.Render(root, canvas);
canvas.Present(Console.Out);
