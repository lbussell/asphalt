#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

string name = "";
string email = "";

ImtuiApplication.Run(imtui =>
{
    #region Example
    using (imtui.BorderPanel("InputText Example"))
    {
        imtui.Text("Tab to move between fields. Type to edit. Enter on Quit to exit.");
        imtui.HRule("Form");

        imtui.Text("Name:");
        imtui.InputText(ref name, placeholder: "your name");

        imtui.Text("Email:");
        imtui.InputText(ref email, placeholder: "you@example.com");

        imtui.HRule("Live preview");
        imtui.Text($"Hello, {(name.Length == 0 ? "stranger" : name)}!");
        if (email.Length > 0)
            imtui.Text($"We'll reach you at {email}.");

        if (imtui.Button("Quit"))
            imtui.QuitAfterThisFrame();
    }
    #endregion
});
