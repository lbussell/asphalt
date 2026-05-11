// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class HRuleWidget(string? text = null) : IWidget
{
    private const char RuleCharacter = '─';
    private const int TextOffset = 2;
    private static readonly TerminalColor s_ruleColor = TerminalColor.Rgb(0x3F, 0x3F, 0x48);

    public string Text { get; } = text ?? string.Empty;

    public WidgetLayout Measure()
    {
        int preferredWidth = Text.Length == 0 ? 1 : Text.Length + TextOffset + 2;
        return new WidgetLayout(new Dimensions(1, 1), new Dimensions(preferredWidth, 1));
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        for (int x = 0; x < bounds.Dimensions.Width; x++)
        {
            canvas.Draw(
                new Position(bounds.Position.X + x, bounds.Position.Y),
                RuleCharacter,
                s_ruleColor
            );
        }

        DrawText(bounds, canvas);
    }

    private void DrawText(Rect bounds, ICanvas canvas)
    {
        if (Text.Length == 0)
            return;

        int width = Math.Min(Text.Length + 2, Math.Max(0, bounds.Dimensions.Width - TextOffset));

        for (int x = 0; x < width; x++)
        {
            char character = x == 0 || x == Text.Length + 1 ? ' ' : Text[x - 1];
            canvas.Draw(
                new Position(bounds.Position.X + TextOffset + x, bounds.Position.Y),
                character
            );
        }
    }
}
