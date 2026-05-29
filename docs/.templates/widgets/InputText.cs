#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

string name = "Ada";
string email = "";
string search = "widgets";

AsphaltApplication.RunAltScreen(asphalt =>
{
    #region Example
    asphalt.Theme = asphalt.Theme with { Placeholder = TerminalColor.White };

    using (asphalt.VStack(gap: 1))
    {
        asphalt.Text("Profile");
        asphalt.InputText(ref name, placeholder: "Name");
        asphalt.InputText(ref email, placeholder: "Email");

        asphalt.Text("Search");
        asphalt.InputText(ref search, placeholder: "Search widgets");
    }
    #endregion Example
});
