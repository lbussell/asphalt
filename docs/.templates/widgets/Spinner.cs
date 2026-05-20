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
    using (imtui.VStack(gap: 1))
    {
        using (imtui.HStack(gap: 1))
        {
            imtui.Spinner();
            imtui.Text("loading");
        }

        using (imtui.HStack(gap: 1))
        {
            imtui.Spinner(
                foregroundColor: TerminalColor.Cyan,
                glyphs: ['◐', '◓', '◑', '◒'],
                frameDuration: TimeSpan.FromMilliseconds(120)
            );
            imtui.Text("custom");
        }
    }
    #endregion Example
}, altScreen: true);
