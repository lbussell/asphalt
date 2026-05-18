#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

int volume = 25;
double gain = 0.0;

ImtuiApplication.Run(imtui =>
{
    #region Example
    using (imtui.BorderPanel("ScalarInput Example"))
    {
        imtui.Text("Tab: switch fields, Left/Down: decrease, Right/Up: increase");
        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Volume");
            imtui.ScalarInput(ref volume, min: 0, max: 100, step: 5);
        }
        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Gain");
            imtui.ScalarInput(ref gain, min: -1.0, max: 1.0, step: 0.05, format: "+0.00;-0.00;0.00");
        }
        if (imtui.Button("Quit"))
        {
            imtui.QuitAfterThisFrame();
        }
    }
    #endregion
});
