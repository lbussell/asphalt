#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

#region Example
int counter = 0;
AsphaltApplication.Run(asphalt =>
{
    using (asphalt.Panel("Alt-Screen Example"))
    {
        asphalt.Text("This sample takes over the full terminal using the alternate screen buffer.");

        asphalt.HRule("Buttons");
        if (asphalt.Button("Increment"))
            counter += 1;

        asphalt.Text($"Count: {counter}");

        if (asphalt.Button("Quit"))
            asphalt.QuitAfterThisFrame();
    }
}, altScreen: true);
#endregion Example
