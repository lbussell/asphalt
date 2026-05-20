#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

string name = "Ada";
string email = "";
string search = "widgets";

ImtuiApplication.Run(imtui =>
{
    #region Example
    imtui.Theme = imtui.Theme with { Placeholder = TerminalColor.White };

    using (imtui.VStack(gap: 1))
    {
        imtui.Text("Profile");
        imtui.InputText(ref name, placeholder: "Name");
        imtui.InputText(ref email, placeholder: "Email");

        imtui.Text("Search");
        imtui.InputText(ref search, placeholder: "Search widgets");
    }
    #endregion Example
}, altScreen: true);
