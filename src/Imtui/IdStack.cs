// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

/// <summary>
/// Manages the hierarchical ID stack used to generate unique widget
/// identifiers in different scopes. Always contains at least the root seed.
/// </summary>
internal sealed class IdStack
{
    private readonly Stack<WidgetID> _stack = new();

    /// <summary>
    /// Creates a new ID stack initialized with the root seed.
    /// </summary>
    public IdStack()
    {
        _stack.Push(WidgetID.Root);
    }

    /// <summary>
    /// The current seed (top of the stack) used for hashing new IDs.
    /// </summary>
    public WidgetID Seed => _stack.Peek();

    /// <summary>
    /// The number of entries on the stack (always at least 1 for the root).
    /// </summary>
    public int Depth => _stack.Count;

    /// <summary>
    /// Pushes an ID onto the stack.
    /// </summary>
    public void Push(WidgetID id) => _stack.Push(id);

    /// <summary>
    /// Pops the top scope from the stack.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to pop the root ID.
    /// </exception>
    public void Pop()
    {
        if (_stack.Count <= 1)
        {
            throw new InvalidOperationException("Cannot pop the root ID from the stack.");
        }

        _stack.Pop();
    }

    /// <summary>
    /// Resets the stack to its initial state with only the root seed.
    /// </summary>
    public void Reset()
    {
        _stack.Clear();
        _stack.Push(WidgetID.Root);
    }
}
