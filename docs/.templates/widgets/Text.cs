#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

AsphaltApplication.Run(asphalt =>
{
    #region Example
    asphalt.Text($"Frame {asphalt.FrameCount}");
    asphalt.Text("Status: connected");
    asphalt.Text(
        "A longer message wraps to the available width.",
        style: new LayoutStyle { Width = LayoutLength.Fixed(24) }
    );
    asphalt.Text(
        "Truncate this long line",
        style: new LayoutStyle { Width = LayoutLength.Fixed(14) },
        wrappingMode: TextWrappingMode.Truncate
    );
    asphalt.Text("Foreground color", foregroundColor: TerminalColor.Cyan);
    asphalt.Text(
        "Background color",
        foregroundColor: TerminalColor.Black,
        backgroundColor: TerminalColor.White
    );
    #endregion Example
}, altScreen: true);
