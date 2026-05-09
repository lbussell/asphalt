// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed class ColorBlock() : IWidget
{
    public void Render(Rect bounds, ICanvas canvas) =>
        canvas.Fill(bounds, TerminalColorRgb.Random());
}

public static class ColorBlockWidgetExtensions
{
    public static void ColorBlock(this ImtuiContext context) => context.AddWidget(new ColorBlock());
}
