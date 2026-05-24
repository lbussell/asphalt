// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

// Tracks frame keypresses and which ones have already been consumed by widgets,
// application fallback handlers, or default focus navigation.
internal sealed class KeyboardDispatcher
{
    // Keys stay in original frame order. Consumption marks the parallel slot in
    // _consumed so skipped keys remain available to later handlers.
    private ConsoleKeyInfo[] _keys = [];
    private bool[] _consumed = [];
    private bool _consumedAnyThisFrame;

    // Loads the keypresses for the frame. The dispatcher copies the input
    // because callers own the FrameInput list and may reuse or mutate it after
    // BeginLayout.
    public void BeginFrame(IReadOnlyList<ConsoleKeyInfo>? keys)
    {
        _consumedAnyThisFrame = false;

        if (keys is null || keys.Count == 0)
        {
            _keys = [];
            _consumed = [];
            return;
        }

        _keys = new ConsoleKeyInfo[keys.Count];
        _consumed = new bool[keys.Count];

        for (int index = 0; index < keys.Count; index++)
            _keys[index] = keys[index];
    }

    // Returns true if any key was consumed during the frame. The application
    // loop uses this to schedule an immediate follow-up frame: a consumed key
    // is assumed to have mutated state that the next render needs to reflect.
    public bool EndFrame() => _consumedAnyThisFrame;

    public bool ConsumeKeys(Func<ConsoleKeyInfo, bool> handleKey)
    {
        ArgumentNullException.ThrowIfNull(handleKey);

        bool consumedAny = false;

        // Restart after each consumed key so a handler that mutates widget state
        // can make a different decision for the next unconsumed key.
        while (true)
        {
            bool consumedThisPass = false;

            for (int index = 0; index < _keys.Length; index++)
            {
                if (_consumed[index] || !handleKey(_keys[index]))
                    continue;

                _consumed[index] = true;
                _consumedAnyThisFrame = true;
                consumedAny = true;
                consumedThisPass = true;
                break;
            }

            if (!consumedThisPass)
                return consumedAny;
        }
    }

    public bool ConsumeKeys(Action<ConsoleKeyInfo> handleKey)
    {
        ArgumentNullException.ThrowIfNull(handleKey);

        return ConsumeKeys(key =>
        {
            handleKey(key);
            return true;
        });
    }
}
