// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

internal sealed class FocusState
{
    private readonly List<WidgetID> _order = [];

    public WidgetID? FocusedId { get; private set; }

    public void BeginFrame(ImtuiInput input)
    {
        if (input.HasKey(ImtuiKey.ShiftTab))
        {
            MoveFocus(-1);
        }
        else if (input.HasKey(ImtuiKey.Tab))
        {
            MoveFocus(1);
        }

        _order.Clear();
    }

    public bool Register(WidgetID id)
    {
        if (!_order.Contains(id))
        {
            _order.Add(id);
        }

        FocusedId ??= id;
        return FocusedId == id;
    }

    private void MoveFocus(int offset)
    {
        if (_order.Count == 0)
        {
            return;
        }

        int currentIndex = FocusedId is { } focusedId ? _order.IndexOf(focusedId) : -1;
        int nextIndex =
            currentIndex < 0 ? 0 : (currentIndex + offset + _order.Count) % _order.Count;
        FocusedId = _order[nextIndex];
    }
}
