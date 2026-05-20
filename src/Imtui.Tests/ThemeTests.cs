// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests;

using Imtui.Rendering;

[TestClass]
public class ThemeTests
{
    private static readonly Dimensions s_dimensions = new Dimensions(40, 5);

    [TestMethod]
    public void ThemeAppliesToRules()
    {
        TerminalColor borderColor = TerminalColor.Rgb(0x77, 0x88, 0x99);

        LayoutNode root = ImtuiTestHarness.RunFrame(
            context =>
            {
                context.Theme = Theme.Default with { Border = borderColor };
                context.HRule();
                context.VRule();
            },
            s_dimensions
        );

        HRuleWidget.Implementation hRule = (HRuleWidget.Implementation)
            root.SingleNodeWithWidget<HRuleWidget.Implementation>().Widget!;
        VRuleWidget.Implementation vRule = (VRuleWidget.Implementation)
            root.SingleNodeWithWidget<VRuleWidget.Implementation>().Widget!;

        Assert.AreEqual(borderColor, hRule.Color);
        Assert.AreEqual(borderColor, vRule.Color);
    }

    [TestMethod]
    public void ThemeAppliesToSliderHandleColors()
    {
        TerminalColor handle = TerminalColor.Rgb(50, 50, 50);
        TerminalColor accent = TerminalColor.Rgb(100, 150, 200);
        TerminalColor border = TerminalColor.Rgb(33, 33, 33);

        int value = 0;
        LayoutNode root = ImtuiTestHarness.RunFrame(
            context =>
            {
                context.Theme = Theme.Default with
                {
                    Placeholder = handle,
                    Accent = accent,
                    Border = border,
                };
                context.Slider(ref value, 0, 10);
            },
            s_dimensions
        );

        SliderWidget.Implementation slider = (SliderWidget.Implementation)
            root.SingleNodeWithWidget<SliderWidget.Implementation>().Widget!;
        Assert.AreEqual(border, slider.BarColor);
        Assert.AreEqual(handle, slider.HandleColor);
        Assert.AreEqual(accent, slider.FocusedHandleColor);
    }
}
