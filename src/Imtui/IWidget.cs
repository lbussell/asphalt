// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

internal interface IWidget
{
    void Execute(ImtuiContext context);
}

internal interface IWidget<out TResult>
{
    TResult Execute(ImtuiContext context);
}
