#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

ImtuiApplication.Run(imtui =>
{
    using (imtui.Panel("Spinner Example"))
    {
        imtui.Text("Multiple spinners animate in lockstep because they share a frame duration.");
        imtui.Text("The run loop wakes once per glyph boundary, not once per spinner.");

        #region Example
        imtui.HRule("Default (80ms)");
        using (imtui.HStack(gap: 1))
        {
            imtui.Spinner();
            imtui.Spinner();
            imtui.Spinner();
            imtui.Text("loading...");
        }

        imtui.HRule("Slower (250ms)");
        using (imtui.HStack(gap: 1))
        {
            imtui.Spinner(frameDuration: TimeSpan.FromMilliseconds(250));
            imtui.Text("thinking...");
        }
        #endregion Example
        imtui.HRule();
        imtui.DebugText();
        if (imtui.Button("Quit"))
            imtui.QuitAfterThisFrame();
    }
});
