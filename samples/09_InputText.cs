#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

string name = "";
string email = "";

AsphaltApplication.Run(asphalt =>
{
    #region Example
    using (asphalt.Panel("InputText Example"))
    {
        asphalt.Text("Tab to move between fields. Type to edit. Enter on Quit to exit.");
        asphalt.HRule("Form");

        asphalt.Text("Name:");
        using var _name = asphalt.InputText(ref name, placeholder: "your name");

        asphalt.Text("Email:");
        using var _email = asphalt.InputText(ref email, placeholder: "you@example.com");

        asphalt.HRule("Live preview");
        asphalt.Text($"Hello, {(name.Length == 0 ? "stranger" : name)}!");
        if (email.Length > 0)
            asphalt.Text($"We'll reach you at {email}.");

        using (asphalt.Button("Quit"))
        {
            if (asphalt.KeyDown(ConsoleKey.Enter))
                asphalt.QuitAfterThisFrame();
        }
    }
    #endregion
});
