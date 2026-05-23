// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

// A mutable, stable handle to a piece of state that persists across frames
// for a single immediate-mode call site. Returned by AsphaltContext.UseState.
//
// The Value field is intentionally a public field rather than a property so
// that callers can read and write it without ceremony — including taking it
// by ref. The same State<T> instance is returned for the same id across
// frames, so a widget can simply mutate Value and the change will be visible
// on subsequent frames.
public sealed class State<T>
{
    public T Value;

    internal State(T initial)
    {
        Value = initial;
    }
}
