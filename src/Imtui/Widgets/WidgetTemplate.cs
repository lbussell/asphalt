// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;
using Imtui.Rendering;

internal static class WidgetTemplate
{
    extension(ImtuiContext context)
    {
        // You define your widget's public API here. This is what you call in the application loop
        // to instantiate your widget. You can create as many extension methods as you want.
        public bool TemplateWidget(
            string label,
            LayoutStyle? style = null,
            string uniqueKey = "",
            [CallerArgumentExpression(nameof(label))] string? labelExpression = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            // You are responsible for creating a unique ID for each widget instance. This is how
            // widgets are tracked across frames. If the widget ID changes, then it's treated as a
            // totally new widget, meaning it won't preserve state or focus.
            //
            // Using .NET's Caller* attributes is the easiest way to construct a unique ID.
            // However, when a widget is rendered multiple times from the same call site (e.g. in a
            // loop), you should provide callers a way to disambiguate those instances. That's what
            // the `uniqueKey` parameter is doing here.
            string id = $"{filePath}:{lineNumber}:{labelExpression}:{uniqueKey}";

            // Comment this out if you don't want your widget to be focusable or accept input.
            WidgetInputState inputState = context.RegisterFocusable(id);

            // For self-contained widgets, open and close the element right away. For
            // container-style widgets, leave it open so callers can add children before closing.
            context.OpenElement(new WidgetTemplateImplementation(label, inputState.Focused), style);
            context.CloseElement();

            // Since imtui handles focus navigation and tracking, it also handles dispatching input
            // to the correct widget (instead of widgets checking focus and inputs themselves).
            // ConsumeKeys will only call your handleKey function when your widget is focused and
            // a higher priority widget didn't already consume the key. Return true if this widget
            // handled the key and prevent other widgets from seeing it.
            bool activated = inputState.ConsumeKeys(handleKey: static key => key.Key == ConsoleKey.Enter);

            // Return whatever the caller needs to drive app logic. By convention:
            // - return boolean to report "something happened this frame".
            // - return void and use ref parameters for mutable values.
            // - container-style widgets return a small IDisposable struct that calls
            //   CloseElement() on dispose, so that callers can use the `using` syntax for nesting.
            // - or just return whatever makes the widget most convenient to use.
            return activated;
        }
    }

    // Actual widget implementation
    private sealed record WidgetTemplateImplementation(string Label, bool Focused) : IWidget
    {
        // Measure reports what size the widget wants. The layout engine calls this during the
        // layout pass.
        //
        // - Minimum: hard floor; the widget will not shrink below this size.
        // - Preferred: target size; extra space is distributed among children whose Preferred > Minimum.
        //   Imagine a text widget as an example. It prefers not to wrap text, so Preferred is the
        //   full length of the text. However, its minimum would be equal to the length of the
        //   longest word, since text can wrap.
        //
        // For a rigid widget, Minimum == Preferred (as below). For flexible widgets, return a
        // smaller Minimum than Preferred. Widgets that want to fill available space should set
        // Width/Height via LayoutStyle rather than reporting it here. Measure is for intrinsic
        // content size only.
        public WidgetLayout Measure()
        {
            Dimensions dimensions = new(Width: Label.Length, Height: 1);
            return new WidgetLayout(Minimum: dimensions, Preferred: dimensions);
        }

        // Render draws into the rect the layout engine assigned. The bounds may be smaller than
        // what Measure() asked for. Render code should generally stay within bounds and degrade
        // gracefully (truncate, clip, skip). But, you know, there are no rules. Render wherever
        // you want. I'm not your boss.
        //
        // The canvas is a per-frame back buffer; the differential renderer only writes cells that
        // actually changed between frames, so draw every character every frame.
        public void Render(Rect bounds, ICanvas canvas)
        {
            canvas.Draw(
                position: new Position(X: 0, Y: 0),
                character: 'X',
                style: TextStyle.None,
                foregroundColor: TerminalColor.White,
                backgroundColor: Focused ? TerminalColor.Blue : TerminalColor.Black
            );
        }
    }
}
