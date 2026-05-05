#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui
#:property PublishAot=false

using Imtui;

ImtuiContext imtui = new ImtuiContext();
bool enabled = true;
string name = "imtui";

imtui.NewFrame();
imtui.Text("dotnet-imtui");
imtui.Checkbox("Enabled", ref enabled);
imtui.TextField("Name", ref name);
imtui.Button("OK");
string output = imtui.RenderFrame();

Console.Out.Write(output);
Console.Out.Flush();
