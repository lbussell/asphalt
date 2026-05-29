#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

int volume = 10;
int brightness = 50;
int zoom = 90;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    using (asphalt.VStack(gap: 1))
    {
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Volume");
            asphalt.Slider(
                ref volume,
                min: 0,
                max: 100,
                style: new Layout { Width = LayoutLength.Fixed(18) }
            );
            asphalt.Text($"{volume}%");
        }

        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Brightness");
            asphalt.Slider(
                ref brightness,
                min: 0,
                max: 100,
                style: new Layout { Width = LayoutLength.Fixed(18) }
            );
            asphalt.Text($"{brightness}%");
        }

        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Zoom");
            asphalt.Slider(
                ref zoom,
                min: 0,
                max: 100,
                style: new Layout { Width = LayoutLength.Fixed(18) }
            );
            asphalt.Text($"{zoom}%");
        }
    }
    #endregion Example
}, altScreen: true);
