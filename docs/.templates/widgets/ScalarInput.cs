#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

int temperature = -5;
int retries = 0;
decimal opacity = 0.75m;

ImtuiApplication.Run(imtui =>
{
    #region Example
    using (imtui.VStack(gap: 1))
    {
        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Temperature");
            imtui.ScalarInput(ref temperature, min: -10, max: 10, width: 5);
        }

        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Retries");
            imtui.ScalarInput(ref retries, min: 0, max: 10, width: 5);
        }

        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Opacity");
            imtui.ScalarInput(ref opacity, min: 0m, max: 1m, step: 0.05m, format: "P0", width: 6);
        }
    }
    #endregion Example
}, altScreen: true);
