// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public static class RuleExtensions
{
    extension(ImtuiContext context)
    {
        public void HRule(string? text = null, LayoutStyle? style = null)
        {
            context.OpenElement(
                new HRuleWidget(text),
                style
                    ?? new LayoutStyle
                    {
                        Width = LayoutLength.Grow(),
                        Height = LayoutLength.Fixed(1),
                    }
            );
            context.CloseElement();
        }

        public void VRule(LayoutStyle? style = null)
        {
            context.OpenElement(
                new VRuleWidget(),
                style
                    ?? new LayoutStyle
                    {
                        Width = LayoutLength.Fixed(1),
                        Height = LayoutLength.Grow(),
                    }
            );
            context.CloseElement();
        }
    }
}
