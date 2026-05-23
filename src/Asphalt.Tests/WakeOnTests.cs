// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class WakeOnTests
{
    [TestMethod]
    public void NullTask_IsNoOp()
    {
        AsphaltContext context = new AsphaltContext();
        int wakeCount = 0;
        context.SetWakeHandler(() => Interlocked.Increment(ref wakeCount));

        context.WakeOn(null);

        Assert.AreEqual(0, wakeCount);
    }

    [TestMethod]
    public void AlreadyCompletedTask_DoesNotInvokeWakeHandler()
    {
        AsphaltContext context = new AsphaltContext();
        int wakeCount = 0;
        context.SetWakeHandler(() => Interlocked.Increment(ref wakeCount));

        context.WakeOn(Task.CompletedTask);

        Assert.AreEqual(0, wakeCount);
    }

    [TestMethod]
    public async Task IncompleteTask_InvokesWakeHandlerOnceOnCompletion()
    {
        AsphaltContext context = new AsphaltContext();
        int wakeCount = 0;
        ManualResetEventSlim woke = new ManualResetEventSlim();
        context.SetWakeHandler(() =>
        {
            Interlocked.Increment(ref wakeCount);
            woke.Set();
        });

        TaskCompletionSource tcs = new TaskCompletionSource();
        context.WakeOn(tcs.Task);

        Assert.AreEqual(0, wakeCount);
        tcs.SetResult();

        Assert.IsTrue(woke.Wait(TimeSpan.FromSeconds(2)));
        Assert.AreEqual(1, wakeCount);
    }

    [TestMethod]
    public async Task SameTaskRegisteredMultipleTimes_InvokesWakeHandlerOnce()
    {
        AsphaltContext context = new AsphaltContext();
        int wakeCount = 0;
        ManualResetEventSlim woke = new ManualResetEventSlim();
        context.SetWakeHandler(() =>
        {
            Interlocked.Increment(ref wakeCount);
            woke.Set();
        });

        TaskCompletionSource tcs = new TaskCompletionSource();
        // Simulate many frames registering the same in-flight task.
        for (int i = 0; i < 10; i++)
            context.WakeOn(tcs.Task);

        tcs.SetResult();

        Assert.IsTrue(woke.Wait(TimeSpan.FromSeconds(2)));
        // Give any spurious continuations time to fire.
        await Task.Delay(50);
        Assert.AreEqual(1, wakeCount);
    }

    [TestMethod]
    public async Task TaskFailsWithException_StillInvokesWakeHandler()
    {
        AsphaltContext context = new AsphaltContext();
        ManualResetEventSlim woke = new ManualResetEventSlim();
        context.SetWakeHandler(() => woke.Set());

        TaskCompletionSource tcs = new TaskCompletionSource();
        context.WakeOn(tcs.Task);
        tcs.SetException(new InvalidOperationException("boom"));

        Assert.IsTrue(woke.Wait(TimeSpan.FromSeconds(2)));
        // Observe the exception so it doesn't become unobserved.
        try
        {
            await tcs.Task;
        }
        catch (InvalidOperationException) { }
    }

    [TestMethod]
    public void NoWakeHandlerAttached_DoesNotThrow()
    {
        AsphaltContext context = new AsphaltContext();
        TaskCompletionSource tcs = new TaskCompletionSource();
        context.WakeOn(tcs.Task);
        tcs.SetResult();
        // Completion should run cleanly with no handler.
    }
}
