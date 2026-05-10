// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public interface IWidget
{
    void Render(Rect bounds, ICanvas canvas);
}
