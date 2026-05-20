#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

ImtuiApplication.Run(imtui =>
{
    #region Example
    using (imtui.HStack(gap: 1))
    {
        using (
            imtui.Panel(
                "No pad",
                style: new LayoutStyle
                {
                    Width = LayoutLength.Fixed(10),
                    Height = LayoutLength.Fixed(3),
                }
            )
        )
        {
            imtui.Text("Body");
        }

        using (
            imtui.Panel(
                "Square",
                BorderStyle.Square,
                padding: new Padding(1),
                style: new LayoutStyle
                {
                    Width = LayoutLength.Fixed(12),
                    Height = LayoutLength.Fixed(5),
                }
            )
        )
        {
            imtui.Text("Body");
        }

        using (
            imtui.Panel(
                borderStyle: BorderStyle.Ascii,
                padding: new Padding(1),
                style: new LayoutStyle
                {
                    Width = LayoutLength.Fixed(12),
                    Height = LayoutLength.Fixed(5),
                }
            )
        )
        {
            imtui.Text("ASCII");
        }
    }
    #endregion Example
}, altScreen: true);
