#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../../../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

AsphaltApplication.RunAltScreen(asphalt =>
{
    #region Example
    asphalt.DebugText();
    #endregion Example
});
