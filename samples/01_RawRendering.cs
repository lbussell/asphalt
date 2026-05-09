#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

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

Dimensions dimensions = new Dimensions(80, 20);
LayoutNode layout = imtui.Build(dimensions);
TerminalCanvas canvas = new TerminalCanvas(dimensions);
Renderer.Render(layout, canvas);
canvas.Present();
