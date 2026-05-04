#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui
#:property PublishAot=false

using Imtui;

ImtuiContext imtui = new ImtuiContext();
imtui.NewFrame();
imtui.Box(0, 0, 20, 20);
string output = imtui.Render();
Console.Out.Write(output);

// screen.WriteText(
//     new CellPosition(2, 1),
//     "dotnet-imtui",
//     new CellStyle(Color.Ansi(AnsiColor.BrightCyan), Color.Default)
// );
// screen.WriteText(
//     new CellPosition(2, 2),
//     "minimal terminal output",
//     new CellStyle(Color.Ansi(AnsiColor.White), Color.Ansi(AnsiColor.Blue))
// );

// Console.Write("\x1b[2J\x1b[H");
// Console.Write(Renderer.Render(new Screen(screen.Size), screen));
// Console.Out.Flush();
