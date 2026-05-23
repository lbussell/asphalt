#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    using (asphalt.HStack(gap: 1))
    {
        using (
            asphalt.Panel(
                "No pad",
                style: new LayoutStyle
                {
                    Width = LayoutLength.Fixed(10),
                    Height = LayoutLength.Fixed(3),
                }
            )
        )
        {
            asphalt.Text("Body");
        }

        using (
            asphalt.Panel(
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
            asphalt.Text("Body");
        }

        using (
            asphalt.Panel(
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
            asphalt.Text("ASCII");
        }
    }
    #endregion Example
}, altScreen: true);
