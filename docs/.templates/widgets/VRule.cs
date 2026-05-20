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
        using (imtui.VStack())
        {
            imtui.Text("Left");
            imtui.Text("side");
        }
        imtui.VRule(style: new LayoutStyle { Height = LayoutLength.Fixed(2) });
        imtui.VRule(style: new LayoutStyle { Height = LayoutLength.Fixed(4) });
        using (imtui.VStack())
        {
            imtui.Text("Right");
            imtui.Text("side");
            imtui.Text("taller");
            imtui.Text("rule");
        }
    }
    #endregion Example
}, altScreen: true);
