// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

internal static class AltScreen
{
    private const string EnterAltScreen = "\x1b[?1049h";
    private const string ExitAltScreen = "\x1b[?1049l";
    private const string HideCursor = "\x1b[?25l";
    private const string ShowCursor = "\x1b[?25h";
    private const string ClearAndHome = "\x1b[2J\x1b[H";
    private const string ResetSgr = "\x1b[0m";

    public static void Enter(TextWriter output)
    {
        output.Write(EnterAltScreen);
        output.Write(HideCursor);
        output.Write(ClearAndHome);
        output.Flush();
    }

    public static void Exit(TextWriter output)
    {
        output.Write(ResetSgr);
        output.Write(ShowCursor);
        output.Write(ExitAltScreen);
        output.Flush();
    }
}
