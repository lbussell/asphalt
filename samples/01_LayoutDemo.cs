#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using static System.Console;

ImtuiContext imtui = new ImtuiContext();
Dimensions dimensions = new Dimensions(80, 20);

imtui.BeginLayout(dimensions);
imtui.OpenElement();
imtui.CloseElement();
LayoutNode root = imtui.EndLayout();

PrintLayout(root);

#region Helpers
static void PrintLayout(LayoutNode node, int indent = 0)
{
    string indentStr = new string(' ', indent);
    WriteLine(
        $"{indentStr}Node: {node.Dimensions.Width}x{node.Dimensions.Height}, Position: ({node.Position.X}, {node.Position.Y})"
    );
    foreach (LayoutNode child in node.Children)
        PrintLayout(child, indent + 2);
}
#endregion
