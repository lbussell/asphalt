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
        imtui.Button("Focused");
        imtui.Button("Unfocused");
    }
    #endregion Example
}, altScreen: true);
