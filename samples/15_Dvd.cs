#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;

string[] logo =
[
    "⠀⠀⣶⣶⣶⣶⣶⣶⣶⣶⣶⠀⠀⠀⢀⣴⣶⣶⣶⣶⣶⣦⣄⡀",
    "⠀⢰⣶⡆⠀⠀⠉⣿⣿⠹⣿⣇⠀⣰⣿⠟⣱⣶⡆⠀⠀⢙⣿⣿",
    "⠀⣾⣿⣁⣀⣠⣾⡿⠋⠀⢻⣿⣾⡟⠁⠀⣿⣿⣁⣀⣠⣾⡿⠋",
    "⠀⠉⠉⠉⠉⠉⠁⠀⠀⠀⠈⡿⠋⠀⠀⠈⠉⠉⠉⠉⠉⠀⠀⠀",
    "⣀⣠⣤⣤⣶⣶⣶⣶⡶⠶⠶⠶⠶⢶⣶⣶⣶⣶⣶⣤⣤⣀⡀⠀",
    "⠉⠙⠛⠛⠛⠿⠿⠿⠷⠶⠶⠶⠶⠾⠿⠿⠿⠟⠛⠛⠛⠉⠁⠀",
];
int logoWidth = logo.Max(line => line.Length);
int logoHeight = logo.Length;

int x = 0;
int y = 0;
int vx = 2;
int vy = 1;

// Classic DVD-screensaver palette. Cycled to the next entry on every bounce.
TerminalColor[] palette =
[
    TerminalColor.Red,
    TerminalColor.Yellow,
    TerminalColor.Green,
    TerminalColor.Cyan,
    TerminalColor.Blue,
    TerminalColor.Magenta,
];
int colorIndex = 0;

AsphaltApplication.Run(
    asphalt =>
    {
        // Boring math
        int maxX = Math.Max(0, asphalt.Dimensions.Width - logoWidth);
        int maxY = Math.Max(0, asphalt.Dimensions.Height - logoHeight);

        // Bounce off the left/right edges
        if (x + vx < 0 || x + vx > maxX)
        {
            vx = -vx;
            colorIndex = (colorIndex + 1) % palette.Length;
        }

        // Bounce off the top/bottom edges
        if (y + vy < 0 || y + vy > maxY)
        {
            vy = -vy;
            colorIndex = (colorIndex + 1) % palette.Length;
        }

        // Don't draw outside the screen
        x = Math.Clamp(x + vx, 0, maxX);
        y = Math.Clamp(y + vy, 0, maxY);

        // Tell Asphalt about our widget
        asphalt.OpenElement(new DvdWidget(x, y, logo, palette[colorIndex]), style: Layout.Grow);
        asphalt.CloseElement();

        // Every time a frame is rendered, manually request the next one.
        // This starts animation.
        asphalt.RequestRedrawIn(TimeSpan.FromMilliseconds(80));
    },
    altScreen: true
);

sealed class DvdWidget(int x, int y, string[] logo, TerminalColor color) : IWidget
{
    // Braille blank — drawn as transparent so the bounding box isn't filled
    // with the foreground color.
    const char BrailleBlank = '⠀';

    public void Render(Rect bounds, ICanvas canvas)
    {
        for (int row = 0; row < logo.Length; row++)
        {
            string line = logo[row];
            for (int col = 0; col < line.Length; col++)
            {
                if (line[col] == BrailleBlank)
                    continue;

                canvas.Draw(
                    new Position(
                        X: bounds.Position.X + x + col,
                        Y: bounds.Position.Y + y + row
                    ),
                    line[col],
                    foregroundColor: color
                );
            }
        }
    }
}
