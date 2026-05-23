#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

int temperature = -5;
int retries = 0;
decimal opacity = 0.75m;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    using (asphalt.VStack(gap: 1))
    {
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Temperature");
            asphalt.ScalarInput(ref temperature, min: -10, max: 10, width: 5);
        }

        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Retries");
            asphalt.ScalarInput(ref retries, min: 0, max: 10, width: 5);
        }

        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Opacity");
            asphalt.ScalarInput(ref opacity, min: 0m, max: 1m, step: 0.05m, format: "P0", width: 6);
        }
    }
    #endregion Example
}, altScreen: true);
