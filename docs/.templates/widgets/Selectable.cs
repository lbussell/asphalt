#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

string[] items = ["Apples", "Bananas", "Cherries", "Dragonfruit"];
int chosen = 1;

AsphaltApplication.RunAltScreen(asphalt =>
{
    #region Example
    using (asphalt.Panel("Fruit"))
    {
        asphalt.Text("Up/Down to move, Enter to choose");
        for (int i = 0; i < items.Length; i++)
        {
            TextStyle textStyle = i == chosen ? TextStyle.Reverse : TextStyle.None;
            if (asphalt.Selectable(items[i], textStyle: textStyle, uniqueKey: i.ToString()))
                chosen = i;
        }
        asphalt.Text($"Chosen: {items[chosen]}");
    }
    #endregion Example
});
