#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

Dimensions dimensions = new(60, 16);
TerminalCanvas canvas = new(dimensions);
ImtuiContext imtui = new();

ConsoleKeyInfo? key = null;
System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

while (true)
{
    FrameInput frameInput = key is { } singleKey
        ? new FrameInput(singleKey, stopwatch.Elapsed)
        : new FrameInput(Time: stopwatch.Elapsed);
    imtui.BeginLayout(dimensions, frameInput);

    using (imtui.FocusScope("panels", FocusNavigation.VimHorizontal))
    using (imtui.HStack(gap: 1))
    {
        using (imtui.FocusScope("left-panel", FocusNavigation.VimVertical))
        using (
            imtui.BorderPanel(
                "Left",
                style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
            )
        )
        {
            imtui.Button("Left item 1");
            imtui.Button("Left item 2");
            imtui.Button("Left item 3");
        }

        using (imtui.FocusScope("right-panel", FocusNavigation.VimVertical))
        using (
            imtui.BorderPanel(
                "Right",
                style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
            )
        )
        {
            imtui.Button("Right item 1");
            imtui.Button("Right item 2");
            imtui.Button("Right item 3");
        }
    }

    LayoutNode root = imtui.EndLayout();
    LayoutRenderer.Render(root, canvas);
    canvas.Present(Console.Out);
    imtui.EndFrame();

    key = Console.ReadKey(intercept: true);
}
