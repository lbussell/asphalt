#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

string[] items =
[
    "Apple",
    "Apricot",
    "Banana",
    "Blackberry",
    "Blueberry",
    "Cantaloupe",
    "Cherry",
    "Coconut",
    "Cranberry",
    "Date",
    "Dragonfruit",
    "Durian",
    "Elderberry",
    "Fig",
    "Gooseberry",
    "Grape",
    "Grapefruit",
    "Guava",
    "Honeydew",
    "Jackfruit",
    "Kiwi",
    "Kumquat",
    "Lemon",
    "Lime",
    "Lychee",
    "Mango",
    "Mulberry",
    "Nectarine",
    "Orange",
    "Papaya",
    "Passionfruit",
    "Peach",
    "Pear",
    "Persimmon",
    "Pineapple",
    "Plum",
    "Pomegranate",
    "Raspberry",
    "Strawberry",
    "Tangerine",
    "Watermelon",
];

int selected = 0;
string lastPicked = "(none yet)";

AsphaltApplication.Run(asphalt =>
{
    asphalt.OpenElement(
        style: new LayoutStyle
        {
            Width = LayoutLength.Fixed(80),
            Height = LayoutLength.Fixed(24),
            Direction = Direction.Horizontal,
            ChildGap = 1,
        }
    );

    using (asphalt.Panel("Fruit", style: LayoutStyle.Grow))
    {
        if (asphalt.SelectableList<string>(items, fruit => fruit, ref selected))
            lastPicked = items[selected];
    }

    using (asphalt.Panel("Details", style: LayoutStyle.Grow))
    {
        asphalt.Text($"Highlighted: {items[selected]}");
        asphalt.Text($"Last picked: {lastPicked}");
        asphalt.HRule();
        asphalt.Text("↑/↓ or j/k to move");
        asphalt.Text("PageUp/PageDown to page");
        asphalt.Text("Home/g to top, End/G to bottom");
        asphalt.Text("Enter to pick");
        asphalt.Text("Tab to switch panels");
        asphalt.DebugText();
    }

    asphalt.CloseElement();
});
