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
    using (imtui.Panel("Alt-Screen Example"))
    {
        imtui.Text("This sample takes over the full terminal using the alternate screen buffer.");

        imtui.HRule("Buttons");
        if (imtui.Button("Increment"))
            counter += 1;

        imtui.Text($"Count: {counter}");

        if (imtui.Button("Quit"))
            imtui.QuitAfterThisFrame();
    }
}, altScreen: true);
#endregion Example
