// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public static class ContainerExtensions
{
    extension(ImtuiContext context)
    {
        public ContainerScope VStack(Padding padding = default, int gap = 0, bool grow = false) =>
            context.Stack(Direction.Vertical, padding, gap, grow);

        public ContainerScope HStack(Padding padding = default, int gap = 0, bool grow = false) =>
            context.Stack(Direction.Horizontal, padding, gap, grow);

        private ContainerScope Stack(Direction direction, Padding padding, int gap, bool grow)
        {
            LayoutLength length = grow ? LayoutLength.Grow() : LayoutLength.Fit();

            context.OpenElement(
                style: new LayoutStyle
                {
                    Direction = direction,
                    Width = length,
                    Height = length,
                    ChildGap = gap,
                    Padding = padding,
                }
            );

            return new ContainerScope(context.CloseElement);
        }
    }
}
