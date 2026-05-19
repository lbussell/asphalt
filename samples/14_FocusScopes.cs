#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

ImtuiApplication.Run(imtui =>
{
    using (imtui.Panel("Focus Scopes"))
    {
        imtui.Text("Use Tab and Shift+Tab to move through focusable widgets.");
        imtui.HRule();

        using (imtui.BeginFocusScope("panels"))
        using (imtui.Panel(gap: 1, direction: Direction.Horizontal))
        {
            using (imtui.BeginFocusScope("left-panel"))
            using (imtui.BorderPanel("Left"))
            {
                imtui.Button("Left item 1");
                imtui.Button("Left item 2");
                imtui.Button("Left item 3");
            }

            using (imtui.BeginFocusScope("right-panel"))
            using (imtui.BorderPanel("Right"))
            {
                imtui.Button("Right item 1");
                imtui.Button("Right item 2");
                imtui.Button("Right item 3");
            }
        }

        if (imtui.Button("Quit"))
            imtui.QuitAfterThisFrame();
    }
});
