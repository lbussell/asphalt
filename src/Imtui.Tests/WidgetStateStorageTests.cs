// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class WidgetStateStorageTests
{
    private sealed class SampleState
    {
        public int Counter { get; set; }
    }

    private sealed class OtherState
    {
        public string Name { get; set; } = "";
    }

    [TestMethod]
    public void GetOrCreate_NewId_CreatesDefaultState()
    {
        WidgetStateStorage storage = new();
        WidgetID id = new(1);

        SampleState state = storage.GetOrCreate<SampleState>(id);

        Assert.IsNotNull(state);
        Assert.AreEqual(0, state.Counter);
    }

    [TestMethod]
    public void GetOrCreate_SameId_ReturnsSameInstance()
    {
        WidgetStateStorage storage = new();
        WidgetID id = new(1);

        SampleState first = storage.GetOrCreate<SampleState>(id);
        first.Counter = 42;
        SampleState second = storage.GetOrCreate<SampleState>(id);

        Assert.AreSame(first, second);
        Assert.AreEqual(42, second.Counter);
    }

    [TestMethod]
    public void GetOrCreate_TypeMismatch_Throws()
    {
        WidgetStateStorage storage = new();
        WidgetID id = new(1);

        storage.GetOrCreate<SampleState>(id);

        Assert.ThrowsExactly<InvalidOperationException>(() => storage.GetOrCreate<OtherState>(id));
    }

    [TestMethod]
    public void Contains_ReturnsTrueForExistingId()
    {
        WidgetStateStorage storage = new();
        WidgetID id = new(1);

        storage.GetOrCreate<SampleState>(id);

        Assert.IsTrue(storage.Contains(id));
    }

    [TestMethod]
    public void Contains_ReturnsFalseForMissingId()
    {
        WidgetStateStorage storage = new();

        Assert.IsFalse(storage.Contains(new WidgetID(999)));
    }

    [TestMethod]
    public void Count_ReflectsStoredEntries()
    {
        WidgetStateStorage storage = new();

        storage.GetOrCreate<SampleState>(new WidgetID(1));
        storage.GetOrCreate<SampleState>(new WidgetID(2));

        Assert.AreEqual(2, storage.Count);
    }

    [TestMethod]
    public void Remove_DeletesState()
    {
        WidgetStateStorage storage = new();
        WidgetID id = new(1);
        storage.GetOrCreate<SampleState>(id);

        bool removed = storage.Remove(id);

        Assert.IsTrue(removed);
        Assert.IsFalse(storage.Contains(id));
    }
}
