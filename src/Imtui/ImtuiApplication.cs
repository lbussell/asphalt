// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Diagnostics;
using System.Threading.Channels;
using Imtui.Rendering;

public static class ImtuiApplication
{
    // Runs an Imtui application that takes over the full terminal. When
    // altScreen is true (default), the alternate screen buffer is used so the
    // application's output doesn't disturb existing scrollback. The frame
    // callback is invoked once per frame to build the UI; return false to
    // exit the loop.
    public static void Run(Func<ImtuiContext, bool> frame, bool altScreen = false) =>
        RunAsync(frame, altScreen).GetAwaiter().GetResult();

    // Async entry point for the run loop. The loop awaits a wake channel
    // that aggregates every source of "render another frame" - keyboard
    // input, animation deadlines, and (in a later phase) Task completions.
    // Multiple wake-ups in the same instant collapse into a single frame
    // via a drain step after each await.
    public static async Task RunAsync(Func<ImtuiContext, bool> frame, bool altScreen = false)
    {
        ArgumentNullException.ThrowIfNull(frame);

        TextWriter output = Console.Out;
        TerminalGuard guard = new TerminalGuard(output, altScreen);

        Channel<FrameEvent> wakeChannel = Channel.CreateUnbounded<FrameEvent>(
            new UnboundedChannelOptions { SingleReader = true }
        );

        try
        {
            Cursor.Hide(output);

            if (altScreen)
                AltScreen.Enter(output);

            KeyboardReader keyboardReader = new KeyboardReader(wakeChannel.Writer);
            keyboardReader.Start();

            ImtuiContext imtui = new ImtuiContext();
            imtui.SetWakeHandler(() => wakeChannel.Writer.TryWrite(WakeEvent.Instance));
            Dimensions terminalDimensions = GetTerminalDimensions();
            TerminalCanvas canvas = new TerminalCanvas(terminalDimensions);
            List<ConsoleKeyInfo> pendingKeys = [];
            // Monotonic clock for the application; passed to each frame so
            // animated widgets can compute their state as a pure function of
            // time.
            long appStartTimestamp = Stopwatch.GetTimestamp();

            while (true)
            {
                // Re-read the terminal size every frame so that resizing the
                // window is reflected on the next render.
                terminalDimensions = GetTerminalDimensions();

                FrameInput frameInput = new FrameInput(
                    Keys: pendingKeys.Count > 0 ? pendingKeys.ToArray() : null,
                    Time: Stopwatch.GetElapsedTime(appStartTimestamp)
                );
                pendingKeys.Clear();

                // Layout always runs against the full terminal size. This way
                // Grow widgets behave the same in both presentation modes - a
                // Grow child fills the terminal regardless of whether we're in
                // altscreen mode.
                imtui.BeginLayout(terminalDimensions, frameInput);
                bool keepRunning = frame(imtui);
                LayoutNode root = imtui.EndLayout();

                if (!keepRunning)
                    break;

                // In altscreen mode we own the whole terminal, so the canvas
                // always matches the terminal. In inline mode the canvas is
                // sized to the laid-out content, so output only consumes as
                // many rows and columns as needed. Users who want a full-size
                // inline app can opt in by placing Grow widgets at the top of
                // their layout.
                Dimensions canvasDimensions = altScreen
                    ? terminalDimensions
                    : GetUsedDimensions(root);

                // Only allocate a new canvas when dimensions actually change -
                // avoids per-frame allocation churn in the steady state.
                if (canvas.Dimensions != canvasDimensions)
                    canvas = new TerminalCanvas(canvasDimensions);

                LayoutRenderer.Render(root, canvas);
                canvas.Present(output, altScreen: altScreen);
                imtui.EndFrame();

                await WaitForNextFrameAsync(
                    wakeChannel.Reader,
                    imtui.NextScheduledRedraw,
                    pendingKeys
                );
            }
        }
        finally
        {
            wakeChannel.Writer.TryComplete();
            guard.Restore();
        }
    }

    // Waits for the next frame trigger and appends every observed keypress
    // to `keys` in arrival order. Returns when either a wake-up arrives or
    // the animation deadline elapses. WakeEvents carry no payload and are
    // coalesced; KeyEvents are preserved in order so the next frame sees
    // every keystroke the user produced.
    private static async Task WaitForNextFrameAsync(
        ChannelReader<FrameEvent> reader,
        TimeSpan? nextRedraw,
        List<ConsoleKeyInfo> keys
    )
    {
        FrameEvent? first;

        if (nextRedraw is null)
        {
            first = await reader.ReadAsync();
        }
        else
        {
            using CancellationTokenSource cts = new CancellationTokenSource(nextRedraw.Value);
            try
            {
                first = await reader.ReadAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                first = null;
            }
        }

        if (first is KeyEvent firstKey)
            keys.Add(firstKey.Key);

        // Drain any other events already queued so simultaneous wake-ups
        // collapse into a single frame. Every KeyEvent is preserved in
        // order; WakeEvents have no payload and are simply consumed.
        while (reader.TryRead(out FrameEvent? more))
        {
            if (more is KeyEvent moreKey)
                keys.Add(moreKey.Key);
        }
    }

    private static Dimensions GetTerminalDimensions()
    {
        int width = Math.Max(1, Console.WindowWidth);
        int height = Math.Max(1, Console.WindowHeight);
        return new Dimensions(width, height);
    }

    // The dimensions actually used by laid-out content within the root. Used
    // to size the canvas in non-altscreen mode so output only takes up as much
    // space as it needs.
    private static Dimensions GetUsedDimensions(LayoutNode root) =>
        new Dimensions(
            root.ChildrenContentWidth + root.Padding.TotalHorizontal,
            root.LaidOutContentHeight + root.Padding.TotalVertical
        );

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
