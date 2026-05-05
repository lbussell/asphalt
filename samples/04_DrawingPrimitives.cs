#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

#:project ../src/Imtui/Imtui.csproj
#:property PublishAot=false

using System.Runtime.CompilerServices;
using Imtui;
using Imtui.Rendering;

ImtuiContext imtui = new ImtuiContext();

Color cyan = Color.Palette256(31);
Color white = Color.Palette256(231);
Color darkGray = Color.Palette256(235);

CellStyle whiteOnBlue = new(Foreground: white, Background: cyan);
CellStyle whiteOnGray = new(Foreground: white, Background: darkGray);

int row = 0;

void DrawDemoWidget(Action draw, int height = 1, [CallerArgumentExpression(nameof(draw))] string expression = "")
{
    row += 1;
    expression = expression.Replace("() => ", "").Replace(nameof(row), row.ToString());
    imtui.WriteText(new CellPosition(1, row - 1), expression);
    draw();
    row += height + 1;
}

Console.CursorVisible = false;
Console.Clear();

try
{
    Size size = new(Console.WindowWidth, 20);
    imtui.NewFrame(size);

    imtui.WriteText(new CellPosition(1, row), "Drawing Primitives Demo");
    row += 2;

    DrawDemoWidget(() => imtui.FillRect(new Rect(X: 1, Y: row, Width: 20, Height: 3), style: whiteOnBlue), height: 3);
    DrawDemoWidget(() => imtui.DrawBox(new Rect(X: 1, Y: row, Width: 20, Height: 4)), height: 4);
    DrawDemoWidget(() => imtui.DrawHorizontalLine(new CellPosition(X: 1, Y: row), length: 30));

    string output = imtui.RenderFrame();
    Console.Out.Write(output);
    Console.Out.Flush();
}
finally
{
    Console.ResetColor();
    Console.CursorVisible = true;
}
