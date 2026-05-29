#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltContext asphalt = new AsphaltContext();
asphalt.BeginLayout(new Dimensions(80, 24));

asphalt.OpenElement(style: LayoutStyle.Grow.WithGap(1).WithPadding(1));

using (asphalt.HStack(padding: 1, gap: 1))
{
    asphalt.OpenElement(style: LayoutStyle.Fixed(5, 1));
    asphalt.CloseElement();
    asphalt.OpenElement(style: LayoutStyle.Fixed(5, 1));
    asphalt.CloseElement();
}

// Row 2: three columns
asphalt.OpenElement(
    style: LayoutStyle.Grow.WithDirection(Direction.Horizontal).WithGap(1).WithPadding(1)
);
asphalt.OpenElement(style: LayoutStyle.Grow);
asphalt.CloseElement();
asphalt.OpenElement(style: LayoutStyle.Grow);
asphalt.CloseElement();
asphalt.OpenElement(style: LayoutStyle.Grow);
asphalt.CloseElement();
asphalt.CloseElement();

// Row 3: vertical with two nested grow children
asphalt.OpenElement(style: LayoutStyle.Grow.WithGap(1).WithPadding(1));
asphalt.OpenElement(style: LayoutStyle.Grow);
asphalt.CloseElement();
asphalt.OpenElement(style: LayoutStyle.Grow);
asphalt.CloseElement();
asphalt.CloseElement();

asphalt.CloseElement();

LayoutNode root = asphalt.EndLayout();
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
