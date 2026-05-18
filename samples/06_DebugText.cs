#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

ImtuiApplication.Run(imtui =>
{
    bool keepRunning = true;

    #region Example
    using (imtui.BorderPanel("DebugText Example"))
    {
        imtui.Text("Press any key to advance a frame.");
        imtui.HRule("imtui.DebugText()");
        imtui.DebugText();
        if (imtui.Button("Quit"))
            keepRunning = false;
    }
    #endregion

    return keepRunning;
});
