// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

/// <summary>
/// Stores per-widget state across frames, keyed by <see cref="WidgetID"/>.
/// State persists until explicitly removed.
/// </summary>
internal sealed class WidgetStateStorage
{
    private readonly Dictionary<WidgetID, object> _states = [];

    /// <summary>
    /// Gets or creates state of type <typeparamref name="T"/> for the given
    /// widget ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if state already exists for <paramref name="id"/> but is not of
    /// type <typeparamref name="T"/>.
    /// </exception>
    public T GetOrCreate<T>(WidgetID id)
        where T : class, new()
    {
        if (_states.TryGetValue(id, out object? existing))
        {
            if (existing is T typed)
            {
                return typed;
            }

            throw new InvalidOperationException(
                $"Widget state type mismatch for ID {id}. "
                    + $"Expected {typeof(T).Name} but found {existing.GetType().Name}."
            );
        }

        T state = new();
        _states[id] = state;
        return state;
    }

    /// <summary>
    /// Checks whether state exists for the given ID.
    /// </summary>
    public bool Contains(WidgetID id) => _states.ContainsKey(id);

    /// <summary>
    /// Number of stored widget states.
    /// </summary>
    public int Count => _states.Count;

    /// <summary>
    /// Removes state for a specific widget ID.
    /// </summary>
    public bool Remove(WidgetID id) => _states.Remove(id);
}
