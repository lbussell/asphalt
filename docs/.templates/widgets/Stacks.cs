#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    using (asphalt.HStack(gap: 4))
    {
        using (asphalt.VStack(gap: 1))
        {
            asphalt.Text("VStack");
            asphalt.Text("A");
            asphalt.Text("B");
        }

        using (asphalt.HStack(padding: new Padding(1), gap: 1))
        {
            asphalt.Text("HStack");
            asphalt.Text("with");
            asphalt.Text("padding");
        }
    }
    #endregion Example
}, altScreen: true);
