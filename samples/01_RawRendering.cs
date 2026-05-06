#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

#:project ../src/Imtui/Imtui.csproj
#:property PublishAot=false

using Imtui.Rendering;

Screen screen = new(new Size(32, 5));

screen.WriteText(
    new CellPosition(2, 1),
    "dotnet-imtui",
    new CellStyle(Color.Ansi(AnsiColor.BrightCyan), Color.Default)
);
screen.WriteText(
    new CellPosition(2, 2),
    "minimal terminal output",
    new CellStyle(Color.Ansi(AnsiColor.White), Color.Ansi(AnsiColor.Blue))
);

Console.Write("\x1b[2J\x1b[H");
Console.Write(Renderer.Render(new Screen(screen.Size), screen));
Console.Out.Flush();
