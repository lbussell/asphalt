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
        using (asphalt.VStack())
        {
            asphalt.Text("Left");
            asphalt.Text("side");
        }
        asphalt.VRule(style: new LayoutStyle { Height = LayoutLength.Fixed(2) });
        asphalt.VRule(style: new LayoutStyle { Height = LayoutLength.Fixed(4) });
        using (asphalt.VStack())
        {
            asphalt.Text("Right");
            asphalt.Text("side");
            asphalt.Text("taller");
            asphalt.Text("rule");
        }
    }
    #endregion Example
}, altScreen: true);
