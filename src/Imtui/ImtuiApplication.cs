// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using Imtui.Rendering;

public static class ImtuiApplication
{
    // Runs an Imtui application that takes over the full terminal. When
    // altScreen is true (default), the alternate screen buffer is used so the
    // application's output doesn't disturb existing scrollback. The frame
    // callback is invoked once per frame to build the UI; return false to
    // exit the loop.
    public static void Run(Func<ImtuiContext, bool> frame, bool altScreen = false)
    {
        ArgumentNullException.ThrowIfNull(frame);

        TextWriter output = Console.Out;
        TerminalGuard guard = new TerminalGuard(output, altScreen);

        try
        {
            Cursor.Hide(output);

            if (altScreen)
                AltScreen.Enter(output);

            ImtuiContext imtui = new ImtuiContext();
            Dimensions dimensions = GetTerminalDimensions();
            TerminalCanvas canvas = new TerminalCanvas(dimensions);
            ConsoleKeyInfo? input = null;

            while (true)
            {
                Dimensions current = GetTerminalDimensions();
                if (current != dimensions)
                {
                    dimensions = current;
                    canvas = new TerminalCanvas(dimensions);
                }

                imtui.BeginLayout(dimensions, input);
                bool keepRunning = frame(imtui);
                LayoutNode root = imtui.EndLayout();

                if (!keepRunning)
                    break;

                LayoutRenderer.Render(root, canvas);
                canvas.Present(output, altScreen: altScreen);

                input = Console.ReadKey(intercept: true);
            }
        }
        finally
        {
            guard.Restore();
        }
    }

    private static Dimensions GetTerminalDimensions()
    {
        int width = Math.Max(1, Console.WindowWidth);
        int height = Math.Max(1, Console.WindowHeight);
        return new Dimensions(width, height);
    }

    // Ensures the terminal is returned to its normal state, even if the
    // process is interrupted (Ctrl+C) or exits abnormally.
    private sealed class TerminalGuard
    {
        private readonly TextWriter _output;
        private readonly bool _altScreen;
        private readonly ConsoleCancelEventHandler _cancelHandler;
        private readonly EventHandler _exitHandler;
        private readonly object _lock = new object();
        private bool _restored;

        public TerminalGuard(TextWriter output, bool altScreen)
        {
            _output = output;
            _altScreen = altScreen;
            _cancelHandler = (_, _) => Restore();
            _exitHandler = (_, _) => Restore();
            Console.CancelKeyPress += _cancelHandler;
            AppDomain.CurrentDomain.ProcessExit += _exitHandler;
        }

        public void Restore()
        {
            lock (_lock)
            {
                if (_restored)
                    return;
                _restored = true;
            }

            try
            {
                if (_altScreen)
                    AltScreen.Exit(_output);
                Cursor.Show(_output);
            }
            catch
            {
                // Best-effort cleanup; never throw from teardown.
            }

            Console.CancelKeyPress -= _cancelHandler;
            AppDomain.CurrentDomain.ProcessExit -= _exitHandler;
        }
    }
}
