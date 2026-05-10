// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public static class ContainerExtensions
{
    extension(ImtuiContext context)
    {
        public ContainerScope VStack(Padding padding = default, int gap = 0) =>
            context.Stack(Direction.Vertical, padding, gap);

        public ContainerScope HStack(Padding padding = default, int gap = 0) =>
            context.Stack(Direction.Horizontal, padding, gap);

        private ContainerScope Stack(Direction direction, Padding padding, int gap)
        {
            context.OpenElement(
                style: new LayoutStyle
                {
                    Direction = direction,
                    Width = LayoutLength.Fit(),
                    Height = LayoutLength.Fit(),
                    ChildGap = gap,
                    Padding = padding,
                }
            );

            return new ContainerScope(context);
        }
    }
}
