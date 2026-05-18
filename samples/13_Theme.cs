#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

int red = 0x4A;
int green = 0x90;
int blue = 0xE2;

ImtuiApplication.Run(imtui =>
{
    #region Example
    imtui.Theme = imtui.Theme with
    {
        Accent = TerminalColor.Rgb((byte)red, (byte)green, (byte)blue),
    };

    using (imtui.Panel("Theme Example"))
    {
        imtui.Text("Tab: switch fields, Left/Down: decrease, Right/Up: increase");
        imtui.Text("Focus a slider to preview the new accent on its handle.");
        imtui.HRule("Accent color");
        using (imtui.HStack(gap: 1))
        {
            imtui.Text("R");
            imtui.Slider(ref red, min: 0, max: 255, step: 5);
            imtui.Text($"{red,3}");
        }
        using (imtui.HStack(gap: 1))
        {
            imtui.Text("G");
            imtui.Slider(ref green, min: 0, max: 255, step: 5);
            imtui.Text($"{green,3}");
        }
        using (imtui.HStack(gap: 1))
        {
            imtui.Text("B");
            imtui.Slider(ref blue, min: 0, max: 255, step: 5);
            imtui.Text($"{blue,3}");
        }

        if (imtui.Button("Quit"))
            imtui.QuitAfterThisFrame();
    }
    #endregion
});
