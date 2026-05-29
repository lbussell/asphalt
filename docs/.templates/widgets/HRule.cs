#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    asphalt.Text("Before");
    asphalt.HRule();
    asphalt.HRule("Section");
    asphalt.HRule("Fixed width", style: new Layout { Width = LayoutLength.Fixed(20) });
    asphalt.Text("After");
    #endregion Example
}, altScreen: true);
