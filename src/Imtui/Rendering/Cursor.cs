// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Rendering;

internal static class Cursor
{
    private const string HideCursor = "\x1b[?25l";
    private const string ShowCursor = "\x1b[?25h";

    public static void Hide(TextWriter output)
    {
        output.Write(HideCursor);
        output.Flush();
    }

    public static void Show(TextWriter output)
    {
        output.Write(ShowCursor);
        output.Flush();
    }
}
