// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests;

using Imtui.Rendering;

[TestClass]
public class ThemeTests
{
    private static readonly Dimensions s_dimensions = new Dimensions(40, 5);

    [TestMethod]
    public void DefaultThemeIsAppliedToButtonBackground()
    {
        LayoutNode root = ImtuiTestHarness.RunFrame(context => context.Button("Ok"), s_dimensions);

        ButtonWidget button = (ButtonWidget)root.SingleNodeWithWidget<ButtonWidget>().Widget!;
        Assert.AreEqual(Theme.Default.Surface, button.BackgroundColor);
        Assert.AreEqual(Theme.Default.SurfaceFocused, button.FocusedBackgroundColor);
    }

    [TestMethod]
    public void CustomThemeFlowsToButtonBackground()
    {
        TerminalColor customSurface = TerminalColor.Rgb(0x11, 0x22, 0x33);
        TerminalColor customFocused = TerminalColor.Rgb(0xAA, 0xBB, 0xCC);

        LayoutNode root = ImtuiTestHarness.RunFrame(
            context =>
            {
                context.Theme = Theme.Default with
                {
                    Surface = customSurface,
                    SurfaceFocused = customFocused,
                };
                context.Button("Ok");
            },
            s_dimensions
        );

        ButtonWidget button = (ButtonWidget)root.SingleNodeWithWidget<ButtonWidget>().Widget!;
        Assert.AreEqual(customSurface, button.BackgroundColor);
        Assert.AreEqual(customFocused, button.FocusedBackgroundColor);
    }

    [TestMethod]
    public void ThemeIsMutableAcrossFrames()
    {
        ImtuiContext context = new ImtuiContext();
        TerminalColor firstColor = TerminalColor.Rgb(10, 10, 10);
        TerminalColor secondColor = TerminalColor.Rgb(20, 20, 20);

        context.BeginLayout(s_dimensions);
        context.Theme = Theme.Default with { Surface = firstColor };
        context.Button("A");
        LayoutNode firstRoot = context.EndLayout();

        context.BeginLayout(s_dimensions);
        context.Theme = context.Theme with { Surface = secondColor };
        context.Button("A");
        LayoutNode secondRoot = context.EndLayout();

        ButtonWidget firstButton = (ButtonWidget)
            firstRoot.SingleNodeWithWidget<ButtonWidget>().Widget!;
        ButtonWidget secondButton = (ButtonWidget)
            secondRoot.SingleNodeWithWidget<ButtonWidget>().Widget!;

        Assert.AreEqual(firstColor, firstButton.BackgroundColor);
        Assert.AreEqual(secondColor, secondButton.BackgroundColor);
    }

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

        HRuleWidget hRule = (HRuleWidget)root.SingleNodeWithWidget<HRuleWidget>().Widget!;
        VRuleWidget vRule = (VRuleWidget)root.SingleNodeWithWidget<VRuleWidget>().Widget!;

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

        SliderWidget slider = (SliderWidget)root.SingleNodeWithWidget<SliderWidget>().Widget!;
        Assert.AreEqual(border, slider.BarColor);
        Assert.AreEqual(handle, slider.HandleColor);
        Assert.AreEqual(accent, slider.FocusedHandleColor);
    }
}
