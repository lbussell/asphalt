#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltApplication.Run(asphalt =>
{
    using (asphalt.Panel("Spinner Example"))
    {
        asphalt.Text("Multiple spinners animate in lockstep because they share a frame duration.");
        asphalt.Text("The run loop wakes once per glyph boundary, not once per spinner.");

        #region Example
        asphalt.HRule("Default (80ms)");
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Spinner();
            asphalt.Spinner();
            asphalt.Spinner();
            asphalt.Text("loading...");
        }

        asphalt.HRule("Slower (250ms)");
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Spinner(frameDuration: TimeSpan.FromMilliseconds(250));
            asphalt.Text("thinking...");
        }
        #endregion Example
        asphalt.HRule();
        asphalt.DebugText();
        using (asphalt.Button("Quit"))
        {
            if (asphalt.KeyDown(ConsoleKey.Enter))
                asphalt.QuitAfterThisFrame();
        }
    }
});
