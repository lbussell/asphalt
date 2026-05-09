#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj
#:property PublishAot=false

using Imtui;

ImtuiContext imtui = new ImtuiContext();

using (imtui.Container(Direction.Horizontal))
{
    imtui.ColorBlock();
    using (imtui.Container(Direction.Vertical))
    {
        imtui.ColorBlock();
        imtui.ColorBlock();
    }
    imtui.ColorBlock();
}

LayoutNode solved = imtui.Build(new Rect(0, 0, 80, 24));
TerminalCanvas canvas = new TerminalCanvas(width: 80, height: 24);
Renderer.Render(solved, canvas);
canvas.Present();
