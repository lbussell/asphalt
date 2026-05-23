#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using static System.Console;
using static Asphalt.TextWrapper;

int wrapAt = 40;
string lorem =
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do"
    + "eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad"
    + "minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip"
    + "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in"
    + "voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur"
    + "sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt"
    + "mollit anim id est laborum.";
string[] truncated = WrapText(lorem, wrapAt, TextWrappingMode.Truncate);
string[] forceWrapped = WrapText(lorem, wrapAt, TextWrappingMode.Force);
string[] wordWrapped = WrapText(lorem, wrapAt, TextWrappingMode.Wrap);

Write(
    $"""

    {Header($"Text truncated at {wrapAt} characters")}
    {Text(truncated)}
    {Dim($"(height: {truncated.Length} lines)")}

    {Header($"Text hard wrapped at {wrapAt} characters")}
    {Text(forceWrapped)}
    {Dim($"(height: {forceWrapped.Length} lines)")}

    {Header($"Text word-boundary wrapped at {wrapAt} characters")}
    {Text(wordWrapped)}
    {Dim($"(height: {wordWrapped.Length} lines)")}


    """
);

static string Header(string text) => $"\u001b[1;4;34m{text}\u001b[0m";
static string Dim(string text) => $"\u001b[2m{text}\u001b[0m";
static string Text(string[] lines) => string.Join('\n', lines);
