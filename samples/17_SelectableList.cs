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
        style: Layout.Fixed(80, 24).WithDirection(Direction.Horizontal).WithGap(1)
    );

    using (asphalt.Panel("Fruit", style: Layout.Grow))
    {
        using (asphalt.SelectableList<string>(items, fruit => fruit, ref selected))
        {
            if (asphalt.KeyDown(ConsoleKey.Enter))
                lastPicked = items[selected];
        }
    }

    using (asphalt.Panel("Details", style: Layout.Grow))
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
