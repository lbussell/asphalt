// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public interface IWidget
{
    void Execute(ImtuiContext context);
}

public interface IWidget<out TResult>
{
    TResult Execute(ImtuiContext context);
}
