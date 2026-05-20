#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

ImtuiApplication.Run(imtui =>
{
    #region Example
    imtui.Text("Before");
    imtui.HRule();
    imtui.HRule("Section");
    imtui.HRule("Fixed width", style: new LayoutStyle { Width = LayoutLength.Fixed(20) });
    imtui.Text("After");
    #endregion Example
}, altScreen: true);
