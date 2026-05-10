#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;

ImtuiContext imtui = new ImtuiContext();
imtui.BeginLayout(new Dimensions(80, 25));

imtui.OpenElement(
    style: new LayoutStyle
    {
        Direction = Direction.Vertical,
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Grow(),
        ChildGap = 1,
        Padding = new Padding(1),
    }
);

// Row 1: two equal columns
imtui.OpenElement(
    style: new LayoutStyle
    {
        Direction = Direction.Horizontal,
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Grow(),
        ChildGap = 1,
        Padding = new Padding(1),
    }
);
imtui.OpenElement(
    style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
);
imtui.CloseElement();
imtui.OpenElement(
    style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
);
imtui.CloseElement();
imtui.CloseElement();

// Row 2: three columns
imtui.OpenElement(
    style: new LayoutStyle
    {
        Direction = Direction.Horizontal,
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Grow(),
        ChildGap = 1,
        Padding = new Padding(1),
    }
);
imtui.OpenElement(
    style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
);
imtui.CloseElement();
imtui.OpenElement(
    style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
);
imtui.CloseElement();
imtui.OpenElement(
    style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
);
imtui.CloseElement();
imtui.CloseElement();

// Row 3: vertical with two nested grow children
imtui.OpenElement(
    style: new LayoutStyle
    {
        Direction = Direction.Vertical,
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Grow(),
        ChildGap = 1,
        Padding = new Padding(1),
    }
);
imtui.OpenElement(
    style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
);
imtui.CloseElement();
imtui.OpenElement(
    style: new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() }
);
imtui.CloseElement();
imtui.CloseElement();

imtui.CloseElement();

LayoutNode root = imtui.EndLayout();
PrintLayout(root);
Console.WriteLine();
LayoutDebug.PrintLayout(root, Console.Out);

#region Helpers
static string LayoutKind(LayoutLength l) =>
    l.Kind switch
    {
        LayoutLengthKind.Fit => "fit",
        LayoutLengthKind.Fixed => $"fixed({l.Value})",
        LayoutLengthKind.Grow => "grow",
        _ => "?",
    };

static void PrintLayout(LayoutNode node, int indent = 0)
{
    string prefix = new string(' ', indent);
    string direction = node.Direction == Direction.Horizontal ? "H" : "V";
    string widthKind = LayoutKind(node.WidthLayout);
    string heightKind = LayoutKind(node.HeightLayout);
    int width = node.Dimensions.Width;
    int height = node.Dimensions.Height;
    Console.WriteLine(
        $"{prefix}{width}x{height}  (w={widthKind}, h={heightKind}, dir={direction})"
    );
    foreach (LayoutNode child in node.Children)
        PrintLayout(child, indent + 2);
}
#endregion
