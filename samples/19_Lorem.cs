#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

int applicationWidth = Math.Min(Console.WindowWidth, 60);
int applicationHeight = Math.Min(Console.WindowHeight, 10);
Dimensions applicationSize = new Dimensions(applicationWidth, applicationHeight);

AsphaltApplication.Run(
    context =>
    {
        if (context.KeyDown(ConsoleKey.Q))
            context.QuitAfterThisFrame();

        using (context.Container(applicationSize))
        using (context.Panel("Long text", style: LayoutStyle.Grow))
        {
            context.Text(
                """
                Repellat sit voluptatem cupiditate molestias recusandae. Numquam
                nobis magnam sunt. Quod est aperiam nihil nihil. Ab voluptatem eum
                dolorem ullam tempora possimus asperiores nemo. Maxime ratione
                beatae ipsa eum officia. Et laudantium reprehenderit consequatur
                debitis eius rem soluta.

                Excepturi ea necessitatibus earum soluta recusandae enim.Soluta
                modi libero adipisci qui quasi quos voluptates.Totam atque dolorem
                labore.Qui nam commodi labore veritatis. Quam et dolorem qui error.
                """,
                wrappingMode: TextWrappingMode.Truncate
            );
        }
    },
    altScreen: false
);
