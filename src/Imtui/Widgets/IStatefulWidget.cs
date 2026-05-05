// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui.Widgets;

public interface IStatefulWidget<out TResult> : IWidget
{
    new TResult Execute(ImtuiContext context);

    void IWidget.Execute(ImtuiContext context) => Execute(context);
}
