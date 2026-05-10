#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;

// Setup
Dimensions dimensions = new Dimensions(80, 20);
TerminalCanvas canvas = new TerminalCanvas(dimensions);
ImtuiContext imtui = new ImtuiContext();

// using (imtui.Container(Direction.Horizontal))
// {
//     using (imtui.BorderPanel(style: new LayoutStyle { Width = LayoutLength.Fixed(18) }))
//     {
//         imtui.Text(
//             "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed"
//                 + "do eiusmod tempor incididunt ut labore et dolore magna"
//                 + "aliqua. Ut enim ad minim veniam, quis nostrud exercitation"
//                 + "ullamco laboris nisi ut aliquip ex ea commodo consequat."
//                 + "Duis aute irure dolor in reprehenderit in voluptate velit"
//                 + "esse cillum dolore eu fugiat nulla pariatur. Excepteur sint"
//                 + "occaecat cupidatat non proident, sunt in culpa qui officia"
//                 + "deserunt mollit anim id est laborum."
//         );
//     }

//     using (imtui.BorderPanel(direction: Direction.Vertical))
//     {
//         imtui.Text("Text can live inside bordered containers.");
//         using (imtui.BorderPanel())
//         {
//             imtui.Text("Nested panel");
//         }
//         imtui.Text("The middle panel grows to fill.");
//     }

//     using (imtui.BorderPanel(BorderStyle.Round, new LayoutStyle { Width = LayoutLength.Fixed(16) }))
//     {
//         imtui.Text("Right fixed");
//     }
// }

// Build the layout - calculates precise dimensions for all widgets/elements
// according to constraints.
LayoutNode layout = imtui.Build(dimensions);
