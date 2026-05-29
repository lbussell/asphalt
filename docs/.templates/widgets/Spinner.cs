#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

AsphaltApplication.RunAltScreen(asphalt =>
{
    #region Example
    using (asphalt.VStack(gap: 1))
    {
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Spinner();
            asphalt.Text("loading");
        }

        using (asphalt.HStack(gap: 1))
        {
            asphalt.Spinner(
                foregroundColor: TerminalColor.Cyan,
                glyphs: ['◐', '◓', '◑', '◒'],
                frameDuration: TimeSpan.FromMilliseconds(120)
            );
            asphalt.Text("custom");
        }
    }
    #endregion Example
});
