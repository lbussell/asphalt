#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

#region Example
int counter = 0;
ImtuiApplication.Run(imtui =>
{
    bool keepRunning = true;
    using (imtui.Panel("Alt-Screen Example", style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }))
    {
        imtui.Text("This sample takes over the full terminal using the alternate screen buffer.");
        imtui.Text("Resize the window — the layout adjusts on the next keypress.");
        imtui.HRule("Buttons");
        if (imtui.Button("Increment"))
            counter += 1;
        imtui.Text($"Count: {counter}");
        if (imtui.Button("Quit"))
            keepRunning = false;
    }
    return keepRunning;
}, altScreen: true);
#endregion Example
