#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui
#:property PublishAot=false

using Imtui;
using Imtui.Rendering;

ImtuiContext imtui = new ImtuiContext(80, 24);

imtui.NewFrame();
imtui.Box(2, 2, 22, 14, AnsiColor.Red);
imtui.Box(8, 5, 28, 17, AnsiColor.Blue);
imtui.Box(14, 8, 34, 20, AnsiColor.Green);
string output = imtui.Render();

Console.Out.Write(output);
Console.Out.Flush();
