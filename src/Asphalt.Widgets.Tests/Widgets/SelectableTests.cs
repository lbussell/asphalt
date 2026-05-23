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
    private static (bool Activated, LayoutNode Root) RunOne(
        AsphaltContext context,
        FrameInput input,
        bool selected = false
    )
    {
        context.BeginLayout(s_terminalDimensions, input);
        bool activated = context.Selectable("Item", selected);
        LayoutNode root = context.EndLayout();
        return (activated, root);
    }

    private static (bool Activated, LayoutNode Root) RunOneRef(
        AsphaltContext context,
        FrameInput input,
        ref bool selected
    )
    {
        context.BeginLayout(s_terminalDimensions, input);
        bool activated = context.Selectable("Item", ref selected);
        LayoutNode root = context.EndLayout();
        return (activated, root);
    }

    private static (bool ActivatedA, bool ActivatedB) RunPair(
        AsphaltContext context,
        TerminalCanvas? canvas,
        FrameInput input,
        bool firstSelected,
        bool secondSelected
    )
    {
        context.BeginLayout(canvas?.Dimensions ?? s_terminalDimensions, input);
        bool activatedA = context.Selectable("A", firstSelected);
        bool activatedB = context.Selectable("B", secondSelected);
        LayoutNode root = context.EndLayout();
        if (canvas is not null)
            LayoutRenderer.Render(root, canvas);
        return (activatedA, activatedB);
    }

    [TestMethod]
    public void Unfocused_EnterPress_IsIgnored()
    {
        AsphaltContext context = new AsphaltContext();

        // Two selectables: first is focused by default. Move focus to the
        // second, then press Enter and verify only the focused one activates.
        RunPair(context, canvas: null, Frame(), false, false);
        RunPair(context, canvas: null, Frame(Down()), false, false);
        (bool a, bool b) = RunPair(context, canvas: null, Frame(Enter()), false, false);

        Assert.IsFalse(a, "unfocused widget must not activate");
        Assert.IsTrue(b, "focused widget must activate on Enter");
    }

    [TestMethod]
    public void Focused_EnterPress_ReturnsTrueOnce()
    {
        AsphaltContext context = new AsphaltContext();

        // Register focusable on frame 1, press Enter on frame 2, then no
        // input on frame 3. Activation should be true exactly on frame 2.
        bool first = RunOne(context, Frame()).Activated;
        bool second = RunOne(context, Frame(Enter())).Activated;
        bool third = RunOne(context, Frame()).Activated;

        Assert.IsFalse(first);
        Assert.IsTrue(second);
        Assert.IsFalse(third);
    }

    [TestMethod]
    public void RefOverload_TogglesSelectedOnEnter()
    {
        AsphaltContext context = new AsphaltContext();
        bool selected = false;

        RunOneRef(context, Frame(), ref selected); // register focus
        Assert.IsFalse(selected);

        bool toggled1 = RunOneRef(context, Frame(Enter()), ref selected).Activated;
        Assert.IsTrue(toggled1);
        Assert.IsTrue(selected);

        bool toggled2 = RunOneRef(context, Frame(Enter()), ref selected).Activated;
        Assert.IsTrue(toggled2);
        Assert.IsFalse(selected);

        bool toggled3 = RunOneRef(context, Frame(), ref selected).Activated;
        Assert.IsFalse(toggled3);
        Assert.IsFalse(selected);
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
    public void Render_FocusedRow_UsesReverseAndBold()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_terminalDimensions);

        // First selectable is focused by default; second is unfocused and
        // unselected.
        RunPair(context, canvas, Frame(), firstSelected: false, secondSelected: false);

        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.Reverse | TextStyle.Bold);
        AssertRowStyle(canvas, row: 1, expectedStyle: TextStyle.None);
    }

    [TestMethod]
    public void Render_SelectedUnfocused_UsesReverseOnly()
    {
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_terminalDimensions);

        // Mark the second selectable selected. Focus stays on the first.
        RunPair(context, canvas, Frame(), firstSelected: false, secondSelected: true);

        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.Reverse | TextStyle.Bold);
        AssertRowStyle(canvas, row: 1, expectedStyle: TextStyle.Reverse);
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

        // The single focusable is focused → reverse + bold across the row.
        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.Reverse | TextStyle.Bold);
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

    [TestMethod]
    public void ClosureOverload_EvaluatesSelectedAtRenderTime()
    {
        // Repro of the staleness problem from the sample: a single `chosen`
        // index drives every row's selected state. With the closure overload,
        // a row whose Selectable runs LATER in the frame can update `chosen`
        // and have the earlier-declared rows render with the new value,
        // because the closure is invoked during the render pass.
        AsphaltContext context = new AsphaltContext();
        TerminalCanvas canvas = new TerminalCanvas(s_terminalDimensions);

        int chosen = 0;

        void RunChooseFrame(FrameInput input)
        {
            context.BeginLayout(canvas.Dimensions, input);
            for (int i = 0; i < 2; i++)
            {
                int index = i;
                if (context.Selectable($"row{i}", () => chosen == index, uniqueKey: i.ToString()))
                    chosen = i;
            }
            LayoutNode node = context.EndLayout();
            LayoutRenderer.Render(node, canvas);
        }

        RunChooseFrame(Frame()); // register focus on row 0
        RunChooseFrame(Frame(Down())); // move focus to row 1
        canvas.Clear();
        RunChooseFrame(Frame(Enter())); // activate row 1

        Assert.AreEqual(1, chosen);
        // Row 0 was declared with chosen still 0 (before row 1 activated);
        // closure must re-evaluate at render time and report it unselected.
        AssertRowStyle(canvas, row: 0, expectedStyle: TextStyle.None);
        // Row 1 is focused (which implies selected) → reverse + bold.
        AssertRowStyle(canvas, row: 1, expectedStyle: TextStyle.Reverse | TextStyle.Bold);
    }

    [TestMethod]
    public void ClosureOverload_NullPredicate_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_terminalDimensions);
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            context.Selectable("Item", isSelected: null!)
        );
        context.EndLayout();
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
