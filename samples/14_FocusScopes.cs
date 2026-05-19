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
        imtui.Text("Use h/l to move between panels.");
        imtui.Text("Use j/k to move between items in the focused panel.");
        imtui.HRule();

        using (imtui.BeginFocusScope("panels", FocusNavigation.VimHorizontal))
        using (imtui.Panel(gap: 1, direction: Direction.Horizontal))
        {
            using (imtui.BeginFocusScope("left-panel", FocusNavigation.VimVertical))
            using (imtui.BorderPanel("Left"))
            {
                imtui.Button("Left item 1");
                imtui.Button("Left item 2");
                imtui.Button("Left item 3");
            }

            using (imtui.BeginFocusScope("right-panel", FocusNavigation.VimVertical))
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
