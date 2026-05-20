#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Rendering;
using Imtui.Widgets;

ImtuiApplication.Run(imtui =>
{
    #region Example
    imtui.Text($"Frame {imtui.FrameCount}");
    imtui.Text("Status: connected");
    imtui.Text(
        "A longer message wraps to the available width.",
        style: new LayoutStyle { Width = LayoutLength.Fixed(24) }
    );
    imtui.Text(
        "Truncate this long line",
        style: new LayoutStyle { Width = LayoutLength.Fixed(14) },
        wrappingMode: TextWrappingMode.Truncate
    );
    imtui.Text("Foreground color", foregroundColor: TerminalColor.Cyan);
    imtui.Text(
        "Background color",
        foregroundColor: TerminalColor.Black,
        backgroundColor: TerminalColor.White
    );
    #endregion Example
}, altScreen: true);
