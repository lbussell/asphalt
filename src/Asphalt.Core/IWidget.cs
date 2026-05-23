// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

using Asphalt.Rendering;

public interface IWidget
{
    WidgetLayout Measure() => WidgetLayout.Zero;
    Dimensions Layout(Dimensions available) => Measure().Preferred;
    void Render(Rect bounds, ICanvas canvas);
}

public readonly record struct WidgetLayout(Dimensions Minimum, Dimensions Preferred)
{
    public static WidgetLayout Zero { get; } = new(new Dimensions(0, 0), new Dimensions(0, 0));
}
