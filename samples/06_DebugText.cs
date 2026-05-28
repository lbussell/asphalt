#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    using (asphalt.Panel("DebugText Example"))
    {
        asphalt.Text("Press any key to advance a frame.");
        asphalt.HRule("asphalt.DebugText()");
        asphalt.DebugText();
        using (asphalt.Button("Quit"))
        {
            if (asphalt.KeyDown(ConsoleKey.Enter))
                asphalt.QuitAfterThisFrame();
        }
    }
    #endregion
});
