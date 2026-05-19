#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

LayoutStyle grow = new() { Height = LayoutLength.Grow(), Width = LayoutLength.Grow() };

ImtuiApplication.Run(imtui =>
{
    imtui.Button("top level button");

    // Application container
    using (imtui.HStack(grow: true))
    {
        // Left sidebar
        using (imtui.VStack(grow: true))
        {
            using (imtui.BorderPanel("Files", style: grow))
            {
                imtui.Button("item 1");
                imtui.Button("item 2");
                imtui.Button("item 3");
            }
            using (imtui.BorderPanel("Branches", style: grow))
            {
                imtui.Button("item 1");
                imtui.Button("item 2");
                imtui.Button("item 3");
            }
            using (imtui.BorderPanel("Commits", style: grow))
            {
                imtui.Text("Hello world.");
                imtui.Button("item 1");
                imtui.Button("item 2");
                imtui.Button("item 3");
            }
        }

        // Right content
        using (imtui.BorderPanel("Content", style: grow ))
        {
            imtui.Text("Hello world.");
            imtui.Button("item 1");
            imtui.Button("item 2");
            imtui.Button("item 3");
        }
    }
}, altScreen: true);
