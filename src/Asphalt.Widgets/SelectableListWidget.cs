// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

using System.Runtime.CompilerServices;
using Asphalt.Rendering;

/// <summary>
/// A scrollable, single-focusable list that renders a row per item and lets
/// the user move a highlighted selection with arrow keys (or vim-style
/// j/k/g/G), page through with PageUp/PageDown/Home/End, and activate the
/// current row with Enter.
/// </summary>
/// <remarks>
/// The list itself is one focus target — individual rows are not focusable.
/// Selection is owned by the caller via a <c>ref int</c>: the widget mutates
/// it in response to navigation keys and clamps it to the available item range
/// every frame. When the list has more items than fit, a one-cell scrollbar is
/// drawn on the right edge and the visible window scrolls just enough to keep
/// the selected row in view.
/// </remarks>
public static class SelectableListWidget
{
    extension(AsphaltContext context)
    {
        /// <summary>
        /// Declares a scrollable selectable list for this frame.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">
        /// Items to render. The collection is retained only for this frame and
        /// is indexed only for visible rows.
        /// </param>
        /// <param name="display">Function mapping an item to its row label.</param>
        /// <param name="selected">
        /// Currently selected row index. Clamped to <c>[0, items.Count-1]</c>
        /// each frame and mutated by ↑/↓, j/k, PageUp/PageDown, Home/End, g/G
        /// while the list is focused. Left untouched when <paramref name="items"/>
        /// is empty.
        /// </param>
        /// <param name="layoutStyle">
        /// Optional layout overrides. Defaults to a container that grows in
        /// both directions. Because the widget cannot know the widest label
        /// without iterating every item, <see cref="IWidget.Measure"/> does
        /// not report a content-based preferred width — place the list
        /// inside a Grow or fixed-width parent for predictable sizing.
        /// </param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple selectable lists that
        /// share a call site (e.g. when rendered in a loop).
        /// </param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        /// <returns>
        /// A <see cref="WidgetScope"/> to be disposed when the list's input
        /// scope ends. Inside the scope, <see cref="AsphaltContext.KeyDown(ConsoleKey)"/>
        /// observes keys not consumed by the list's own navigation (notably
        /// <c>Enter</c>, <c>Space</c>, <c>Delete</c>, letters, ...).
        /// </returns>
        public WidgetScope SelectableList<T>(
            IReadOnlyList<T> items,
            Func<T, string> display,
            ref int selected,
            Layout? layoutStyle = null,
            string uniqueKey = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(display);
            ArgumentNullException.ThrowIfNull(items);

            return RenderSelectableList(
                context,
                items,
                display,
                ref selected,
                layoutStyle,
                uniqueKey,
                filePath,
                lineNumber
            );
        }

        /// <summary>
        /// Declares a scrollable selectable list for this frame.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">
        /// Items to render. The span is consumed during this call and
        /// snapshotted internally because spans cannot be retained for render.
        /// Prefer the <see cref="IReadOnlyList{T}"/> overload when the items
        /// already live in a persistent collection.
        /// </param>
        /// <param name="display">Function mapping an item to its row label.</param>
        /// <param name="selected">
        /// Currently selected row index. Clamped to <c>[0, items.Length-1]</c>
        /// each frame and mutated by ↑/↓, j/k, PageUp/PageDown, Home/End, g/G
        /// while the list is focused. Left untouched when <paramref name="items"/>
        /// is empty.
        /// </param>
        /// <param name="layoutStyle">
        /// Optional layout overrides. Defaults to a container that grows in
        /// both directions. Because the widget cannot know the widest label
        /// without iterating every item, <see cref="IWidget.Measure"/> does
        /// not report a content-based preferred width — place the list
        /// inside a Grow or fixed-width parent for predictable sizing.
        /// </param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple selectable lists that
        /// share a call site (e.g. when rendered in a loop).
        /// </param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        /// <returns>
        /// A <see cref="WidgetScope"/> to be disposed when the list's input
        /// scope ends. See the <see cref="IReadOnlyList{T}"/> overload for details.
        /// </returns>
        public WidgetScope SelectableList<T>(
            ReadOnlySpan<T> items,
            Func<T, string> display,
            ref int selected,
            Layout? layoutStyle = null,
            string uniqueKey = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(display);

            T[] snapshot = items.Length == 0 ? [] : items.ToArray();

            return RenderSelectableList(
                context,
                snapshot,
                display,
                ref selected,
                layoutStyle,
                uniqueKey,
                filePath,
                lineNumber
            );
        }

        /// <summary>
        /// Declares a scrollable selectable list for this frame, with the
        /// selected item itself tracked by reference instead of an index.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">
        /// Items to render. The collection is retained only for this frame and
        /// is indexed only for visible rows.
        /// </param>
        /// <param name="display">Function mapping an item to its row label.</param>
        /// <param name="selected">
        /// Currently selected item. Located in <paramref name="items"/> via
        /// <see cref="EqualityComparer{T}.Default"/> each frame; if not found,
        /// selection falls back to the first item. Updated to the navigated
        /// item by ↑/↓, j/k, PageUp/PageDown, Home/End, g/G while the list is
        /// focused. Left untouched when <paramref name="items"/> is empty.
        /// </param>
        /// <param name="layoutStyle">
        /// Optional layout overrides. Defaults to a container that grows in
        /// both directions.
        /// </param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple selectable lists that
        /// share a call site (e.g. when rendered in a loop).
        /// </param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        /// <returns>
        /// <c>true</c> on the single frame in which Enter was pressed while
        /// the list was focused; otherwise <c>false</c>.
        /// </returns>
        public WidgetScope SelectableList<T>(
            IReadOnlyList<T> items,
            Func<T, string> display,
            ref T selected,
            Layout? layoutStyle = null,
            string uniqueKey = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(display);
            ArgumentNullException.ThrowIfNull(items);

            int selectedIndex = IndexOf(items, selected);
            WidgetScope scope = RenderSelectableList(
                context,
                items,
                display,
                ref selectedIndex,
                layoutStyle,
                uniqueKey,
                filePath,
                lineNumber
            );

            if (items.Count > 0)
                selected = items[selectedIndex];

            return scope;
        }

        /// <summary>
        /// Declares a scrollable selectable list for this frame, with the
        /// selected item itself tracked by reference instead of an index.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">
        /// Items to render. The span is consumed during this call and
        /// snapshotted internally because spans cannot be retained for render.
        /// </param>
        /// <param name="display">Function mapping an item to its row label.</param>
        /// <param name="selected">
        /// Currently selected item. Located in <paramref name="items"/> via
        /// <see cref="EqualityComparer{T}.Default"/> each frame; if not found,
        /// selection falls back to the first item. Updated to the navigated
        /// item by ↑/↓, j/k, PageUp/PageDown, Home/End, g/G while the list is
        /// focused. Left untouched when <paramref name="items"/> is empty.
        /// </param>
        /// <param name="layoutStyle">
        /// Optional layout overrides. Defaults to a container that grows in
        /// both directions.
        /// </param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple selectable lists that
        /// share a call site (e.g. when rendered in a loop).
        /// </param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        public WidgetScope SelectableList<T>(
            ReadOnlySpan<T> items,
            Func<T, string> display,
            ref T selected,
            Layout? layoutStyle = null,
            string uniqueKey = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            ArgumentNullException.ThrowIfNull(display);

            T[] snapshot = items.Length == 0 ? [] : items.ToArray();
            int selectedIndex = IndexOf(snapshot, selected);

            WidgetScope scope = RenderSelectableList(
                context,
                snapshot,
                display,
                ref selectedIndex,
                layoutStyle,
                uniqueKey,
                filePath,
                lineNumber
            );

            if (snapshot.Length > 0)
                selected = snapshot[selectedIndex];

            return scope;
        }
    }

    private static int IndexOf<T>(IReadOnlyList<T> items, T value)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < items.Count; i++)
        {
            if (comparer.Equals(items[i], value))
                return i;
        }
        return 0;
    }

    private static WidgetScope RenderSelectableList<T>(
        AsphaltContext context,
        IReadOnlyList<T> items,
        Func<T, string> display,
        ref int selected,
        Layout? layoutStyle,
        string uniqueKey,
        string filePath,
        int lineNumber
    )
    {
        string id = $"{filePath}:{lineNumber}:{uniqueKey}";
        // Empty lists have nothing to select; skip focus registration so
        // they don't consume Enter or appear in the focus cycle.
        WidgetInputState inputState = items.Count == 0 ? default : context.RegisterFocusable(id);
        State<ListState> state = context.UseState(
            id + ":selectable-list",
            () => new ListState(ScrollOffset: 0, LastViewportRows: 0)
        );

        int count = items.Count;
        if (count > 0)
            selected = Math.Clamp(selected, 0, count - 1);

        if (count > 0)
        {
            // At the boundaries (top with ↑/k, bottom with ↓/j) we
            // intentionally return false so the framework's default
            // focus navigation moves focus out of the list instead of
            // swallowing the keypress.

            // Lambdas can't capture `ref` parameters, so route mutations
            // through a local and copy back after.
            int newSelected = selected;

            inputState.ConsumeKeys(key =>
            {
                bool shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;
                switch (key.Key)
                {
                    // Move up
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.K when !shift:
                        if (newSelected > 0)
                        {
                            newSelected--;
                            return true;
                        }
                        return false;

                    // Move down
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.J when !shift:
                        if (newSelected < count - 1)
                        {
                            newSelected++;
                            return true;
                        }
                        return false;

                    // Jump to top
                    case ConsoleKey.Home:
                    case ConsoleKey.G when !shift:
                        newSelected = 0;
                        return true;

                    // Jump to bottom
                    case ConsoleKey.End:
                    case ConsoleKey.G when shift:
                        newSelected = count - 1;
                        return true;

                    default:
                        return false;
                }
            });

            selected = newSelected;
        }

        context.OpenElement(
            new Implementation<T>(
                items,
                display,
                selected,
                inputState.Focused,
                state,
                context.Theme
            ),
            layoutStyle ?? s_defaultStyle
        );
        context.PushWidgetInputScope(inputState.Focused);

        return new WidgetScope(() =>
        {
            context.PopWidgetInputScope();
            context.CloseElement();
        });
    }

    private static readonly Layout s_defaultStyle = new()
    {
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Grow(),
    };

    // ScrollOffset is the index of the row drawn at the top of the
    // viewport. LastViewportRows is the height of the viewport from the
    // previous frame; reserved for navigation that needs a page size.
    private record struct ListState(int ScrollOffset, int LastViewportRows);

    private sealed record Implementation<T>(
        IReadOnlyList<T> Items,
        Func<T, string> Display,
        int Selected,
        bool Focused,
        State<ListState> State,
        Theme Theme
    ) : IWidget
    {
        public WidgetLayout Measure()
        {
            // Height preference is meaningful (one row per item, so Fit
            // parents wrap and the shrinker handles overflow). Width
            // preference is intentionally 1 — computing the widest label
            // would force iterating every item, which defeats the
            // virtualized display() contract. Callers size width via the
            // parent container.
            int preferredHeight = Math.Max(1, Items.Count);
            return new WidgetLayout(
                Minimum: new Dimensions(1, 1),
                Preferred: new Dimensions(1, preferredHeight)
            );
        }

        public void Render(Rect bounds, ICanvas canvas)
        {
            int width = bounds.Dimensions.Width;
            int height = bounds.Dimensions.Height;

            if (width <= 0 || height <= 0)
                return;

            if (Items.Count == 0)
            {
                State.Value = State.Value with { LastViewportRows = height };
                return;
            }

            // Minimum-scroll-to-keep-selected-visible: leave scrollOffset
            // alone if Selected is already on-screen; otherwise nudge it
            // just enough to bring Selected to the nearest edge. The
            // pre-clamp handles the case where the item count shrunk
            // since last frame.
            int count = Items.Count;
            int viewportRows = height;
            int scrollOffset = State.Value.ScrollOffset;
            int maxScroll = Math.Max(0, count - viewportRows);

            scrollOffset = Math.Clamp(scrollOffset, 0, maxScroll);
            if (Selected < scrollOffset)
                scrollOffset = Selected;
            else if (Selected >= scrollOffset + viewportRows)
                scrollOffset = Selected - viewportRows + 1;
            scrollOffset = Math.Clamp(scrollOffset, 0, maxScroll);

            // Reserve the right-edge column for the scrollbar only when
            // content actually overflows, so non-overflowing lists get the
            // full width for their labels.
            bool overflowing = count > viewportRows;
            int labelWidth = overflowing ? Math.Max(0, width - 1) : width;

            for (int row = 0; row < viewportRows; row++)
            {
                int itemIndex = scrollOffset + row;
                if (itemIndex >= count)
                    break;

                bool isSelected = itemIndex == Selected;
                TextStyle style = TextStyle.None;
                TerminalColor bg = default;

                // Two-tier selection highlight: Reverse when the list owns
                // focus (strong, matches Selectable's focused look); a
                // surface-color fill when focus is elsewhere (visible but
                // doesn't compete with the truly-focused widget).
                if (isSelected)
                {
                    if (Focused)
                        style = TextStyle.Reverse;
                    else
                        bg = Theme.InteractableSurface.Unfocused.Background;
                }

                // display() is invoked here, only for visible rows — the
                // virtualization promise that makes the ReadOnlySpan input
                // meaningful. Each row is hard-padded to labelWidth so the
                // selection highlight spans the entire row.
                string label = Display(Items[itemIndex]) ?? string.Empty;
                int y = bounds.Position.Y + row;
                for (int x = 0; x < labelWidth; x++)
                {
                    char c = x < label.Length ? label[x] : ' ';
                    canvas.Draw(
                        new Position(bounds.Position.X + x, y),
                        c,
                        backgroundColor: bg,
                        style: style
                    );
                }
            }

            // Proportional scrollbar: thumb size reflects the fraction of
            // the list that's visible; thumb position reflects how far
            // through the list scrollOffset has advanced. Only the thumb
            // cells are drawn; the track cells are left untouched so they
            // render as blank space.
            if (overflowing && width > 0)
            {
                int scrollbarX = bounds.Position.X + width - 1;
                int thumbSize = Math.Max(1, viewportRows * viewportRows / count);
                thumbSize = Math.Min(thumbSize, viewportRows);
                int maxThumbStart = viewportRows - thumbSize;
                int thumbStart =
                    maxScroll == 0 ? 0 : (int)((long)scrollOffset * maxThumbStart / maxScroll);
                TerminalColor thumbColor = Focused
                    ? TerminalColor.White
                    : Theme.InteractableSurface.Unfocused.Background;

                for (int row = 0; row < thumbSize; row++)
                {
                    canvas.Draw(
                        new Position(scrollbarX, bounds.Position.Y + thumbStart + row),
                        '┃',
                        foregroundColor: thumbColor
                    );
                }
            }

            State.Value = new ListState(scrollOffset, viewportRows);
        }
    }
}
