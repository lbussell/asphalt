// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;

namespace Imtui;

/// <summary>
/// Keyboard input understood by Imtui widgets.
/// </summary>
public enum ImtuiKey
{
    /// <summary>Moves focus to the next focusable widget.</summary>
    Tab,

    /// <summary>Moves focus to the previous focusable widget.</summary>
    ShiftTab,

    /// <summary>Activates the focused widget or confirms text editing.</summary>
    Enter,

    /// <summary>Activates the focused widget.</summary>
    Space,

    /// <summary>Cancels the current interaction.</summary>
    Escape,

    /// <summary>Moves a text cursor one character left.</summary>
    LeftArrow,

    /// <summary>Moves a text cursor one character right.</summary>
    RightArrow,

    /// <summary>Deletes the character before a text cursor.</summary>
    Backspace,

    /// <summary>Deletes the character at a text cursor.</summary>
    Delete,
}

/// <summary>
/// A single input event for the current frame.
/// </summary>
public readonly record struct ImtuiInputEvent
{
    private ImtuiInputEvent(ImtuiKey? key, Rune? character)
    {
        Key = key;
        Character = character;
    }

    /// <summary>
    /// The key for key input events, or <see langword="null"/> for character input.
    /// </summary>
    public ImtuiKey? Key { get; }

    /// <summary>
    /// The character for text input events, or <see langword="null"/> for key input.
    /// </summary>
    public Rune? Character { get; }

    /// <summary>
    /// Creates a key input event.
    /// </summary>
    public static ImtuiInputEvent FromKey(ImtuiKey key) => new(key, null);

    /// <summary>
    /// Creates a character input event.
    /// </summary>
    public static ImtuiInputEvent FromCharacter(char character) =>
        FromCharacter(new Rune(character));

    /// <summary>
    /// Creates a character input event.
    /// </summary>
    public static ImtuiInputEvent FromCharacter(Rune character) => new(null, character);
}

/// <summary>
/// Input supplied to a single Imtui frame.
/// </summary>
public readonly record struct ImtuiInput
{
    /// <summary>
    /// Creates frame input from ordered input events.
    /// </summary>
    public ImtuiInput(params ImtuiInputEvent[] events)
    {
        Events = events;
    }

    /// <summary>
    /// Creates frame input from a single key.
    /// </summary>
    public static ImtuiInput FromKey(ImtuiKey key) => new(ImtuiInputEvent.FromKey(key));

    /// <summary>
    /// Creates frame input from a single character, mapping known control characters to widget keys.
    /// </summary>
    public static ImtuiInput FromCharacter(char character) =>
        character switch
        {
            '\t' => FromKey(ImtuiKey.Tab),
            '\r' or '\n' => FromKey(ImtuiKey.Enter),
            ' ' => FromKey(ImtuiKey.Space),
            '\b' => FromKey(ImtuiKey.Backspace),
            '\u001b' => FromKey(ImtuiKey.Escape),
            _ when !char.IsControl(character) => new ImtuiInput(
                ImtuiInputEvent.FromCharacter(character)
            ),
            _ => default,
        };

    /// <summary>
    /// Creates frame input from console key information.
    /// </summary>
    public static ImtuiInput FromConsoleKeyInfo(ConsoleKeyInfo keyInfo)
    {
        if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0 && keyInfo.Key == ConsoleKey.Tab)
        {
            return FromKey(ImtuiKey.ShiftTab);
        }

        return keyInfo.Key switch
        {
            ConsoleKey.Tab => FromKey(ImtuiKey.Tab),
            ConsoleKey.Enter => FromKey(ImtuiKey.Enter),
            ConsoleKey.Spacebar => FromKey(ImtuiKey.Space),
            ConsoleKey.Escape => FromKey(ImtuiKey.Escape),
            ConsoleKey.LeftArrow => FromKey(ImtuiKey.LeftArrow),
            ConsoleKey.RightArrow => FromKey(ImtuiKey.RightArrow),
            ConsoleKey.Backspace => FromKey(ImtuiKey.Backspace),
            ConsoleKey.Delete => FromKey(ImtuiKey.Delete),
            _ => FromCharacter(keyInfo.KeyChar),
        };
    }

    /// <summary>
    /// Ordered input events for this frame.
    /// </summary>
    public ReadOnlyMemory<ImtuiInputEvent> Events { get; }

    internal bool HasKey(ImtuiKey key)
    {
        foreach (ImtuiInputEvent inputEvent in Events.Span)
        {
            if (inputEvent.Key == key)
            {
                return true;
            }
        }

        return false;
    }
}
