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
        using (asphalt.Button("Increment"))
        {
            if (asphalt.KeyDown(ConsoleKey.Enter))
                counter += 1;
        }

        asphalt.Text($"Count: {counter}");

        using (asphalt.Button("Quit"))
        {
            if (asphalt.KeyDown(ConsoleKey.Enter))
                asphalt.QuitAfterThisFrame();
        }
    }
}, altScreen: true);
#endregion Example
