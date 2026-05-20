#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

int volume = 10;
int brightness = 50;
int zoom = 90;

ImtuiApplication.Run(imtui =>
{
    #region Example
    using (imtui.VStack(gap: 1))
    {
        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Volume");
            imtui.Slider(
                ref volume,
                min: 0,
                max: 100,
                style: new LayoutStyle { Width = LayoutLength.Fixed(18) }
            );
            imtui.Text($"{volume}%");
        }

        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Brightness");
            imtui.Slider(
                ref brightness,
                min: 0,
                max: 100,
                style: new LayoutStyle { Width = LayoutLength.Fixed(18) }
            );
            imtui.Text($"{brightness}%");
        }

        using (imtui.HStack(gap: 1))
        {
            imtui.Text("Zoom");
            imtui.Slider(
                ref zoom,
                min: 0,
                max: 100,
                style: new LayoutStyle { Width = LayoutLength.Fixed(18) }
            );
            imtui.Text($"{zoom}%");
        }
    }
    #endregion Example
}, altScreen: true);
