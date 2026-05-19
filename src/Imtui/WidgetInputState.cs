// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

internal readonly struct WidgetInputState(bool focused, KeyboardDispatcher keyboard)
{
    public bool Focused { get; } = focused;

    // Gives the focused widget first chance to consume matching keypresses.
    // Return true from the handler to mark a key consumed; return false to
    // leave it available for application fallback or default focus navigation.
    // Unfocused widgets never consume keys.
    public bool ConsumeKeys(Func<ConsoleKeyInfo, bool> handleKey) =>
        Focused && keyboard.ConsumeKeys(handleKey);

    // Convenience overload for widgets that consume every key they inspect.
    public bool ConsumeKeys(Action<ConsoleKeyInfo> handleKey) =>
        Focused && keyboard.ConsumeKeys(handleKey);
}
