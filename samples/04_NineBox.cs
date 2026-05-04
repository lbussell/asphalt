#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui
#:property PublishAot=false

using Imtui;
using Imtui.Widgets;

ImtuiContext imtui = new ImtuiContext();

imtui.NewFrame();
imtui.NineBox(2, 1, 20, 8);
imtui.NineBox(10, 4, 24, 10);
string output = imtui.RenderFrame();

Console.Out.Write(output);
Console.Out.Flush();
