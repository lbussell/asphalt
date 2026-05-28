#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

LayoutStyle grow = new() { Height = LayoutLength.Grow(), Width = LayoutLength.Grow() };

AsphaltApplication.Run(asphalt =>
{
    asphalt.Button("top level button");

    // Application container
    using (asphalt.HStack(grow: true))
    {
        // Left sidebar
        using (asphalt.VStack(grow: true))
        {
            using (asphalt.Panel("Files", style: grow))
            {
                asphalt.Button("item 1");
                asphalt.Button("item 2");
                asphalt.Button("item 3");
            }
            using (asphalt.Panel("Branches", style: grow))
            {
                asphalt.Button("item 1");
                asphalt.Button("item 2");
                asphalt.Button("item 3");
            }
            using (asphalt.Panel("Commits", style: grow))
            {
                asphalt.Text("Hello world.");
                asphalt.Button("item 1");
                asphalt.Button("item 2");
                asphalt.Button("item 3");
            }
        }

        // Right content
        using (asphalt.Panel("Content", style: grow ))
        {
            asphalt.Text("Hello world.");
            asphalt.Button("item 1");
            asphalt.Button("item 2");
            asphalt.Button("item 3");
        }
    }
}, altScreen: true);
