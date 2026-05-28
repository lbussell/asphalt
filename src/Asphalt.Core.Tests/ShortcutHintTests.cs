// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class ShortcutHintTests
{
    private static readonly Dimensions s_dimensions = new Dimensions(20, 5);

    [TestMethod]
    public void OutsideWidgetScope_AlwaysRegisters()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        context.AddShortcutHint("Q", "Quit");

        Assert.AreEqual(1, context.ShortcutHints.Count);
        Assert.AreEqual(new ShortcutHint("Q", "Quit"), context.ShortcutHints[0]);
    }

    [TestMethod]
    public void InsideFocusedWidgetScope_Registers()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        context.PushWidgetInputScope(focused: true);
        context.AddShortcutHint("Enter", "Select");
        context.PopWidgetInputScope();

        Assert.AreEqual(1, context.ShortcutHints.Count);
        Assert.AreEqual(new ShortcutHint("Enter", "Select"), context.ShortcutHints[0]);
    }

    [TestMethod]
    public void InsideUnfocusedWidgetScope_Skipped()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        context.PushWidgetInputScope(focused: false);
        context.AddShortcutHint("Enter", "Select");
        context.PopWidgetInputScope();

        Assert.AreEqual(0, context.ShortcutHints.Count);
    }

    [TestMethod]
    public void PreservesRegistrationOrder()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        context.AddShortcutHint("Q", "Quit");
        context.PushWidgetInputScope(focused: true);
        context.AddShortcutHint("Enter", "Select");
        context.AddShortcutHint("D", "Delete");
        context.PopWidgetInputScope();

        CollectionAssert.AreEqual(
            new[]
            {
                new ShortcutHint("Q", "Quit"),
                new ShortcutHint("Enter", "Select"),
                new ShortcutHint("D", "Delete"),
            },
            context.ShortcutHints.ToArray()
        );
    }

    [TestMethod]
    public void ClearedEachFrame()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(s_dimensions);
        context.AddShortcutHint("Q", "Quit");
        Assert.AreEqual(1, context.ShortcutHints.Count);
        context.EndLayout();

        context.BeginLayout(s_dimensions);
        Assert.AreEqual(0, context.ShortcutHints.Count);
        context.AddShortcutHint("N", "New");
        Assert.AreEqual(1, context.ShortcutHints.Count);
        Assert.AreEqual(new ShortcutHint("N", "New"), context.ShortcutHints[0]);
        context.EndLayout();
    }

    [TestMethod]
    public void NestedScopes_InnermostFocusGoverns()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        // Outer scope focused, inner unfocused: hint added inside inner is dropped.
        context.PushWidgetInputScope(focused: true);
        context.AddShortcutHint("Outer", "ok");

        context.PushWidgetInputScope(focused: false);
        context.AddShortcutHint("Inner", "dropped");
        context.PopWidgetInputScope();

        // Back at outer (focused): subsequent adds work again.
        context.AddShortcutHint("OuterAgain", "ok");
        context.PopWidgetInputScope();

        CollectionAssert.AreEqual(
            new[] { new ShortcutHint("Outer", "ok"), new ShortcutHint("OuterAgain", "ok") },
            context.ShortcutHints.ToArray()
        );
    }
}
