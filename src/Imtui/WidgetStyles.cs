// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Imtui.Rendering;

namespace Imtui;

internal static class WidgetStyles
{
    public static CellStyle Normal => new(Color.Default, Color.Default);

    public static CellStyle Focused =>
        new(Color.Ansi(AnsiColor.Black), Color.Ansi(AnsiColor.White));

    public static CellStyle ForFocus(bool focused) => focused ? Focused : Normal;
}
