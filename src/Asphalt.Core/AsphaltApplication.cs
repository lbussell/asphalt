// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

using System.Diagnostics;
using System.Threading.Channels;
using Asphalt.Rendering;

public static class AsphaltApplication
{
    // Runs an inline Asphalt application that renders below the existing
    // terminal content. The canvas grows to fit the laid-out content each
    // frame, so output only consumes as many rows and columns as needed.
    // Call AsphaltContext.QuitAfterThisFrame from the frame callback to
    // exit the loop after the current frame finishes rendering.
    public static void Run(Action<AsphaltContext> frame) =>
        RunAsync(frame, altScreen: false, maxSize: null).GetAwaiter().GetResult();

    // Runs an inline Asphalt application whose layout is capped at maxSize.
    // Each frame the effective size is min(terminal, maxSize), so resizing
    // the terminal smaller still works and resizing larger stops growing
    // once the cap is reached.
    public static void Run(Dimensions maxSize, Action<AsphaltContext> frame) =>
        RunAsync(frame, altScreen: false, maxSize: maxSize).GetAwaiter().GetResult();

    // Runs an Asphalt application that takes over the full terminal using
    // the alternate screen buffer, so the application's output doesn't
    // disturb existing scrollback. Call AsphaltContext.QuitAfterThisFrame
    // from the frame callback to exit the loop after the current frame
    // finishes rendering.
    public static void RunAltScreen(Action<AsphaltContext> frame) =>
        RunAsync(frame, altScreen: true, maxSize: null).GetAwaiter().GetResult();

    // Async entry point for the run loop. The loop awaits a wake channel
    // that aggregates every source of "render another frame" - keyboard
    // input, animation deadlines, and (in a later phase) Task completions.
    // Multiple wake-ups in the same instant collapse into a single frame
    // via a drain step after each await.
    private static async Task RunAsync(
        Action<AsphaltContext> frame,
        bool altScreen,
        Dimensions? maxSize
    )
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

            AsphaltContext asphalt = new AsphaltContext();
            asphalt.SetWakeHandler(() => wakeChannel.Writer.TryWrite(WakeEvent.Instance));
            Dimensions layoutDimensions = GetLayoutDimensions(maxSize);
            TerminalCanvas canvas = new TerminalCanvas(layoutDimensions);
            TerminalPresenter presenter = new TerminalPresenter(output, altScreen);
            List<ConsoleKeyInfo> pendingKeys = [];

            // Monotonic clock for the application; passed to each frame so
            // animated widgets can compute their state as a pure function of
            // time.
            long appStartTimestamp = Stopwatch.GetTimestamp();

            while (true)
            {
                // Re-read the terminal size every frame so that resizing the
                // window is reflected on the next render. When a maxSize was
                // provided, the layout is clamped to it.
                layoutDimensions = GetLayoutDimensions(maxSize);

                FrameInput frameInput = new FrameInput(
                    Keys: pendingKeys.Count > 0 ? pendingKeys.ToArray() : null,
                    Time: Stopwatch.GetElapsedTime(appStartTimestamp)
                );
                pendingKeys.Clear();

                asphalt.BeginLayout(layoutDimensions, frameInput);
                frame(asphalt);
                LayoutNode root = asphalt.EndLayout();

                // In altscreen mode we own the whole terminal, so the canvas
                // always matches the terminal. In inline mode the canvas is
                // sized to the laid-out content, so output only consumes as
                // many rows and columns as needed. Users who want a full-size
                // inline app can opt in by placing Grow widgets at the top of
                // their layout.
                Dimensions canvasDimensions = altScreen
                    ? layoutDimensions
                    : GetUsedDimensions(root);

                // Only allocate a new canvas when dimensions actually change -
                // avoids per-frame allocation churn in the steady state.
                if (canvas.Dimensions != canvasDimensions)
                    canvas = new TerminalCanvas(canvasDimensions);

                // Clear last frame's cells so widgets that shrink between
                // frames don't leave stale characters behind.
                canvas.Clear();

                LayoutRenderer.Render(root, canvas);

                // Overlays paint after the primary tree so they appear on top.
                foreach (LayoutNode overlay in asphalt.Overlays)
                    LayoutRenderer.Render(overlay, canvas);

                presenter.Present(canvas);
                asphalt.EndFrame();

                if (asphalt.QuitRequested)
                    break;

                await WaitForNextFrameAsync(
                    wakeChannel.Reader,
                    asphalt.NextScheduledRedraw,
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

    // Returns the dimensions to lay out against this frame: the terminal
    // size, optionally clamped by a caller-provided maximum.
    private static Dimensions GetLayoutDimensions(Dimensions? maxSize)
    {
        int width = Math.Max(1, Console.WindowWidth);
        int height = Math.Max(1, Console.WindowHeight);
        if (maxSize is { } cap)
        {
            width = Math.Min(width, cap.Width);
            height = Math.Min(height, cap.Height);
        }
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
