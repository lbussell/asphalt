// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests.Widgets;

using Asphalt.Rendering;
using Asphalt.Widgets;

[TestClass]
public class SelectableListTests
{
    private static readonly Dimensions s_dimensions = new(20, 5);

    private static FrameInput Frame(params ConsoleKeyInfo[] keys) => new(keys);

    private static ConsoleKeyInfo Key(ConsoleKey key, bool shift = false) =>
        new('\0', key, shift, alt: false, control: false);

    private static readonly string[] s_short = ["A", "B", "C"];
    private static readonly string[] s_long = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

    // All runs go through the same call site so the widget id is stable
    // across frames within a single test.
    private static bool Run(
        AsphaltContext context,
        IReadOnlyList<string> items,
        ref int selected,
        FrameInput input
    )
    {
        context.BeginLayout(s_dimensions, input);
        bool activated;
        using (context.SelectableList<string>(items, x => x, ref selected))
        {
            activated = context.KeyDown(ConsoleKey.Enter);
        }
        context.EndLayout();
        return activated;
    }

    private static bool RunSpan(
        AsphaltContext context,
        ReadOnlySpan<string> items,
        ref int selected,
        FrameInput input
    )
    {
        context.BeginLayout(s_dimensions, input);
        bool activated;
        using (context.SelectableList<string>(items, x => x, ref selected))
        {
            activated = context.KeyDown(ConsoleKey.Enter);
        }
        context.EndLayout();
        return activated;
    }

    [TestMethod]
    public void DownArrow_MovesSelection()
    {
        AsphaltContext context = new();
        int selected = 0;

        Run(context, s_short, ref selected, Frame());
        Run(context, s_short, ref selected, Frame(Key(ConsoleKey.DownArrow)));

        Assert.AreEqual(1, selected);
    }

    [TestMethod]
    public void UpArrow_AtTop_DoesNotMutateAndBubbles()
    {
        AsphaltContext context = new();
        int selected = 0;

        Run(context, s_short, ref selected, Frame());
        Run(context, s_short, ref selected, Frame(Key(ConsoleKey.UpArrow)));

        Assert.AreEqual(0, selected);
    }

    [TestMethod]
    public void Enter_ReturnsTrueOnlyOnTheFrameItIsPressed()
    {
        AsphaltContext context = new();
        int selected = 0;

        bool first = Run(context, s_short, ref selected, Frame());
        bool second = Run(context, s_short, ref selected, Frame(Key(ConsoleKey.Enter)));
        bool third = Run(context, s_short, ref selected, Frame());

        Assert.IsFalse(first);
        Assert.IsTrue(second);
        Assert.IsFalse(third);
    }

    [TestMethod]
    public void Home_JumpsToTop_End_JumpsToBottom()
    {
        AsphaltContext context = new();
        int selected = 0;

        Run(context, s_long, ref selected, Frame());
        Run(context, s_long, ref selected, Frame(Key(ConsoleKey.End)));
        Assert.AreEqual(s_long.Length - 1, selected);

        Run(context, s_long, ref selected, Frame(Key(ConsoleKey.Home)));
        Assert.AreEqual(0, selected);
    }

    [TestMethod]
    public void ShiftG_JumpsToBottom_LowercaseG_JumpsToTop()
    {
        AsphaltContext context = new();
        int selected = 0;

        Run(context, s_long, ref selected, Frame());
        Run(context, s_long, ref selected, Frame(Key(ConsoleKey.G, shift: true)));
        Assert.AreEqual(s_long.Length - 1, selected);

        Run(context, s_long, ref selected, Frame(Key(ConsoleKey.G)));
        Assert.AreEqual(0, selected);
    }

    [TestMethod]
    public void J_MovesDown_K_MovesUp()
    {
        AsphaltContext context = new();
        int selected = 0;

        Run(context, s_long, ref selected, Frame());
        Run(context, s_long, ref selected, Frame(Key(ConsoleKey.J)));
        Run(context, s_long, ref selected, Frame(Key(ConsoleKey.J)));
        Assert.AreEqual(2, selected);

        Run(context, s_long, ref selected, Frame(Key(ConsoleKey.K)));
        Assert.AreEqual(1, selected);
    }

    [TestMethod]
    public void OutOfRangeSelected_IsClampedAndWrittenBack()
    {
        AsphaltContext context = new();
        int selected = 999;

        Run(context, s_short, ref selected, Frame());

        Assert.AreEqual(2, selected);
    }

    [TestMethod]
    public void NegativeSelected_IsClampedToZero()
    {
        AsphaltContext context = new();
        int selected = -5;

        Run(context, s_short, ref selected, Frame());

        Assert.AreEqual(0, selected);
    }

    [TestMethod]
    public void EmptyList_DoesNotMutateSelected_AndEnterReturnsFalse()
    {
        AsphaltContext context = new();
        int selected = 42;

        bool activated = Run(context, [], ref selected, Frame(Key(ConsoleKey.Enter)));

        Assert.IsFalse(activated);
        Assert.AreEqual(42, selected);
    }

    [TestMethod]
    public void SpanOverload_MovesSelection()
    {
        AsphaltContext context = new();
        int selected = 0;

        RunSpan(context, s_short, ref selected, Frame());
        RunSpan(context, s_short, ref selected, Frame(Key(ConsoleKey.DownArrow)));

        Assert.AreEqual(1, selected);
    }

    [TestMethod]
    public void ReadOnlyListOverload_DoesNotEnumerateItems()
    {
        AsphaltContext context = new();
        int selected = 0;
        IReadOnlyList<string> items = new IndexOnlyList(["A", "B", "C"]);

        Run(context, items, ref selected, Frame());

        Assert.AreEqual(0, selected);
    }

    [TestMethod]
    public void DownArrow_AtBottom_DoesNotMoveAndDoesNotConsume()
    {
        AsphaltContext context = new();
        int selected = s_short.Length - 1;

        Run(context, s_short, ref selected, Frame());
        Run(context, s_short, ref selected, Frame(Key(ConsoleKey.DownArrow)));

        Assert.AreEqual(s_short.Length - 1, selected);
    }

    private sealed class IndexOnlyList(string[] items) : IReadOnlyList<string>
    {
        public string this[int index] => items[index];

        public int Count => items.Length;

        public IEnumerator<string> GetEnumerator() =>
            throw new InvalidOperationException("SelectableList should not enumerate items.");

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
