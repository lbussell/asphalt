// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests.Widgets;

using Asphalt.Rendering;
using Asphalt.Widgets;

[TestClass]
public class SelectableTests
{
    private static readonly Dimensions s_terminalDimensions = new Dimensions(20, 5);

    private static FrameInput Frame(params ConsoleKeyInfo[] keys) => new FrameInput(keys);

    private static ConsoleKeyInfo Enter() =>
        new ConsoleKeyInfo('\r', ConsoleKey.Enter, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Down() =>
        new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, shift: false, alt: false, control: false);

    // All helpers below call Selectable from the same source line so the
    // widget id (built from CallerFilePath + CallerLineNumber) is stable
    // across frames within a single test.
    private static bool RunOne(
        AsphaltContext context,
        FrameInput input,
        TextStyle textStyle = TextStyle.None
    )
    {
        context.BeginLayout(s_terminalDimensions, input);
        bool activated = context.Selectable("Item", textStyle);
        context.EndLayout();
        return activated;
    }

    private static (bool A, bool B) RunPair(
        AsphaltContext context,
        TerminalCanvas? canvas,
        FrameInput input,
        TextStyle firstStyle = TextStyle.None,
        TextStyle secondStyle = TextStyle.None
    )
    {
        context.BeginLayout(canvas?.Dimensions ?? s_terminalDimensions, input);
        bool aActivated = context.Selectable("A", firstStyle);
        bool bActivated = context.Selectable("B", secondStyle);
        LayoutNode root = context.EndLayout();
        if (canvas is not null)
            LayoutRenderer.Render(root, canvas);
        return (aActivated, bActivated);
    }

    [TestMethod]
    public void Unfocused_EnterPress_IsIgnored()
    {
        AsphaltContext context = new AsphaltContext();

        // Two selectables: first is focused by default. Move focus to the
        // second, then press Enter and verify only the focused one activates.
        RunPair(context, canvas: null, Frame());
        RunPair(context, canvas: null, Frame(Down()));
        (bool a, bool b) = RunPair(context, canvas: null, Frame(Enter()));

        Assert.IsFalse(a, "unfocused widget must not activate");
        Assert.IsTrue(b, "focused widget must activate on Enter");
    }

    [TestMethod]
    public void Focused_EnterPress_ReturnsTrueOnce()
    {
        AsphaltContext context = new AsphaltContext();

        // Register focusable on frame 1, press Enter on frame 2, then no
        // input on frame 3. Activation should be true exactly on frame 2.
        bool first = RunOne(context, Frame());
        bool second = RunOne(context, Frame(Enter()));
        bool third = RunOne(context, Frame());

        Assert.IsFalse(first);
        Assert.IsTrue(second);
        Assert.IsFalse(third);
    }

    [TestMethod]
    public void Measure_ReportsHeightOneAndLabelWidth()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_terminalDimensions);
        context.Selectable("Hello");
        LayoutNode root = context.EndLayout();

        SelectableWidget.Implementation widget = (SelectableWidget.Implementation)
            root.NodesWithWidget<SelectableWidget.Implementation>().Single().Widget!;

        WidgetLayout layout = widget.Measure();
        Assert.AreEqual(new Dimensions(5, 1), layout.Minimum);
        Assert.AreEqual(new Dimensions(5, 1), layout.Preferred);
    }

    [TestMethod]
    public void Render_FocusedRow_AppliesReverse()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_terminalDimensions);

        // First selectable is focused by default; second is unfocused and
        // has no caller-supplied style.
        RunPair(context, canvas, Frame());

        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.Reverse);
        AssertRowStyle(canvas, row: 1, expectedStyle: TextStyle.None);
    }

    [TestMethod]
    public void Render_CallerSuppliedStyle_IsApplied()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_terminalDimensions);

        // Move focus to the second row (Down's effect lands on the
        // following frame), then paint the unfocused first row Reverse.
        RunPair(context, canvas: null, Frame());
        RunPair(context, canvas: null, Frame(Down()));
        RunPair(context, canvas, Frame(), firstStyle: TextStyle.Reverse);

        // First row is unfocused but caller-styled Reverse.
        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.Reverse);
        // Second row is focused, no caller style.
        AssertRowStyle(canvas, row: 1, expectedStyle: TextStyle.Reverse);
    }

    [TestMethod]
    public void Render_FocusOrsWithCallerStyle()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_terminalDimensions);

        // Focused row with caller-supplied Bold should render Bold|Reverse.
        RunPair(context, canvas, Frame(), firstStyle: TextStyle.Bold);

        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.Bold | TextStyle.Reverse);
        AssertRowStyle(canvas, row: 1, expectedStyle: TextStyle.None);
    }

    [TestMethod]
    public void Render_FillsAvailableWidth()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(new Dimensions(10, 2));

        context.BeginLayout(canvas.Dimensions);
        context.Selectable("Hi");
        LayoutNode root = context.EndLayout();
        LayoutRenderer.Render(root, canvas);

        Assert.AreEqual('H', canvas.GetCell(0, 0).CharacterOrSpace);
        Assert.AreEqual('i', canvas.GetCell(1, 0).CharacterOrSpace);
        for (int x = 2; x < canvas.Dimensions.Width; x++)
            Assert.AreEqual(' ', canvas.GetCell(x, 0).CharacterOrSpace, $"column {x}");

        // The single focusable is focused → reverse across the whole row.
        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.Reverse);
    }

    [TestMethod]
    public void UniqueKey_AllowsRepeatedCallSite()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(s_terminalDimensions);
        for (int index = 0; index < 3; index++)
            context.Selectable($"Item {index}", uniqueKey: index.ToString());
        LayoutNode root = context.EndLayout();

        Assert.AreEqual(3, root.NodesWithWidget<SelectableWidget.Implementation>().Count());
    }

    private static void AssertRowStyle(TerminalCanvas canvas, int row, TextStyle expectedStyle)
    {
        for (int x = 0; x < canvas.Dimensions.Width; x++)
        {
            TerminalCell cell = canvas.GetCell(x, row);
            Assert.AreEqual(expectedStyle, cell.Style, $"row {row}, column {x}");
        }
    }
}
