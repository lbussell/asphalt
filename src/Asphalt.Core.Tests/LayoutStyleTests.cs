// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class LayoutStyleTests
{
    [TestMethod]
    public void Default_IsEquivalentToFit()
    {
        LayoutStyle defaulted = default;
        Assert.AreEqual(LayoutLengthKind.Fit, defaulted.Width.Kind);
        Assert.AreEqual(LayoutLengthKind.Fit, defaulted.Height.Kind);
        Assert.AreEqual(int.MaxValue, defaulted.Width.Maximum);
        Assert.AreEqual(int.MaxValue, defaulted.Height.Maximum);
        Assert.AreEqual(0, defaulted.Width.Minimum);
        Assert.AreEqual(0, defaulted.ChildGap);
        Assert.AreEqual(Direction.Vertical, defaulted.Direction);
        Assert.AreEqual(Padding.Zero, defaulted.Padding);
    }

    [TestMethod]
    public void DefaultLayoutLength_EqualsFitParameterless()
    {
        // Critical invariant: default(LayoutLength) and LayoutLength.Fit()
        // must be value-equal so that default(LayoutStyle) is interchangeable
        // with the explicit "fit" style.
        Assert.AreEqual(LayoutLength.Fit(), default(LayoutLength));
    }

    [TestMethod]
    public void FluentChain_DoesNotAllocate()
    {
        // Warm up — JIT, statics, anything that allocates on first touch.
        _ = BuildChain();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        long before = GC.GetAllocatedBytesForCurrentThread();

        // Hammer it to amplify any per-call allocation past measurement noise.
        LayoutStyle sink = default;
        for (int i = 0; i < 10_000; i++)
            sink = BuildChain();

        long after = GC.GetAllocatedBytesForCurrentThread();

        Assert.AreEqual(
            0L,
            after - before,
            $"Expected zero allocations in fluent chain; observed {after - before} bytes. "
                + $"Sink: {sink}"
        );
    }

    private static LayoutStyle BuildChain() =>
        LayoutStyle.Grow.WithPadding(2).WithGap(1).WithDirection(Direction.Horizontal);
}
