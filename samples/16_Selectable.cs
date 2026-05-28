#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

string[] items =
[
    "New File",
    "Open...",
    "Save",
    "Save As...",
    "Quit",
];
int chosen = 0;
string lastAction = "(nothing yet)";

AsphaltApplication.Run(asphalt =>
{
    using (asphalt.Panel("Menu"))
    {
        asphalt.Text("Up/Down to move focus, Enter to pick an item.");
        asphalt.HRule();

        for (int i = 0; i < items.Length; i++)
        {
            TextStyle textStyle = i == chosen ? TextStyle.Reverse : TextStyle.None;
            if (asphalt.Selectable(items[i], textStyle: textStyle, uniqueKey: i.ToString()))
            {
                chosen = i;
                lastAction = items[i];
                if (items[i] == "Quit")
                    asphalt.QuitAfterThisFrame();
            }
        }

        asphalt.HRule();
        asphalt.Text($"Last action: {lastAction}");
    }
});
