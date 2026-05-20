#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

ImtuiApplication.Run(imtui =>
{
    #region Example
    using (imtui.HStack(gap: 4))
    {
        using (imtui.VStack(gap: 1))
        {
            imtui.Text("VStack");
            imtui.Text("A");
            imtui.Text("B");
        }

        using (imtui.HStack(padding: new Padding(1), gap: 1))
        {
            imtui.Text("HStack");
            imtui.Text("with");
            imtui.Text("padding");
        }
    }
    #endregion Example
}, altScreen: true);
