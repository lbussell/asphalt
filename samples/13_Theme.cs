#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

int red = 0x4A;
int green = 0x90;
int blue = 0xE2;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    asphalt.Theme = asphalt.Theme with
    {
        Accent = TerminalColor.Rgb((byte)red, (byte)green, (byte)blue),
    };

    using (asphalt.Panel("Theme Example"))
    {
        asphalt.Text("Tab: switch fields, Left/Down: decrease, Right/Up: increase");
        asphalt.Text("Focus a slider to preview the new accent on its handle.");
        asphalt.HRule("Accent color");
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("R");
            asphalt.Slider(ref red, min: 0, max: 255, step: 5);
            asphalt.Text($"{red,3}");
        }
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("G");
            asphalt.Slider(ref green, min: 0, max: 255, step: 5);
            asphalt.Text($"{green,3}");
        }
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("B");
            asphalt.Slider(ref blue, min: 0, max: 255, step: 5);
            asphalt.Text($"{blue,3}");
        }

        if (asphalt.Button("Quit"))
            asphalt.QuitAfterThisFrame();
    }
    #endregion
});
