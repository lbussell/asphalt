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
        asphalt.Button("Focused");
        asphalt.Button("Unfocused");
    }
    #endregion Example
}, altScreen: true);
