// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using Imtui.Rendering;

public interface IWidget
{
    void Render(Rect bounds, ICanvas canvas);
}
