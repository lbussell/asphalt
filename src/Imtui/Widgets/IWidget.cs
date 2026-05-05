// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui.Widgets;

public interface IWidget
{
    bool IsFocusable => false;

    WidgetID ID => WidgetID.Root;

    void Execute(ImtuiContext context);
}
