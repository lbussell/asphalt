#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

LayoutStyle grow = new() { Height = LayoutLength.Grow(), Width = LayoutLength.Grow() };

AsphaltApplication.Run(asphalt =>
{
    using var _top = asphalt.Button("top level button");

    // Application container
    using (asphalt.HStack(grow: true))
    {
        // Left sidebar
        using (asphalt.VStack(grow: true))
        {
            using (asphalt.Panel("Files", style: grow))
            {
                using var _f1 = asphalt.Button("item 1");
                using var _f2 = asphalt.Button("item 2");
                using var _f3 = asphalt.Button("item 3");
            }
            using (asphalt.Panel("Branches", style: grow))
            {
                using var _b1 = asphalt.Button("item 1");
                using var _b2 = asphalt.Button("item 2");
                using var _b3 = asphalt.Button("item 3");
            }
            using (asphalt.Panel("Commits", style: grow))
            {
                asphalt.Text("Hello world.");
                using var _c1 = asphalt.Button("item 1");
                using var _c2 = asphalt.Button("item 2");
                using var _c3 = asphalt.Button("item 3");
            }
        }

        // Right content
        using (asphalt.Panel("Content", style: grow ))
        {
            asphalt.Text("Hello world.");
            using var _r1 = asphalt.Button("item 1");
            using var _r2 = asphalt.Button("item 2");
            using var _r3 = asphalt.Button("item 3");
        }
    }
}, altScreen: true);
