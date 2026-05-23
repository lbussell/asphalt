#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

int volume = 25;
double gain = 0.0;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    using (asphalt.Panel("Slider Example"))
    {
        asphalt.Text("Tab: switch fields, Left/Down: decrease, Right/Up: increase");
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Volume");
            asphalt.Slider(ref volume, min: 0, max: 100, step: 5);
            asphalt.Text(volume.ToString());
        }
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Gain");
            asphalt.Slider(ref gain, min: -1.0, max: 1.0, step: 0.05);
            asphalt.Text($"{gain:+0.00;-0.00;0.00}");
        }
        if (asphalt.Button("Quit"))
        {
            asphalt.QuitAfterThisFrame();
        }
    }
    #endregion
});
