#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltApplication.RunAltScreen(asphalt =>
{
    #region Example
    using (asphalt.VStack(gap: 1))
    {
        Bar("Download", 1.0f);
        Bar("Install", 0.62f);
        Bar("Verify", 0.18f);
    }

    void Bar(string label, float progress)
    {
        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text(label, style: new Layout { Width = LayoutLength.Fixed(9) });
            asphalt.ProgressBar(progress, style: new Layout { Width = LayoutLength.Fixed(20) });
            asphalt.Text($"{progress * 100:0}%");
        }
    }
    #endregion Example
});
