// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using System.Threading.Channels;

// Reads keypresses from Console on a dedicated background thread and pushes
// them into the run loop's wake channel. Console.ReadKey has no async form
// and blocks until a key arrives, so a thread is the simplest way to make
// keyboard input cooperate with an async event loop.
internal sealed class KeyboardReader
{
    private readonly ChannelWriter<FrameEvent> _writer;
    private readonly Thread _thread;

    public KeyboardReader(ChannelWriter<FrameEvent> writer)
    {
        _writer = writer;
        _thread = new Thread(ReadLoop) { IsBackground = true, Name = "Imtui.KeyboardReader" };
    }

    public void Start() => _thread.Start();

    private void ReadLoop()
    {
        try
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (!_writer.TryWrite(new KeyEvent(key)))
                    return;
            }
        }
        catch (InvalidOperationException)
        {
            // Stdin is redirected or otherwise unavailable. The application
            // will simply never receive key events; the loop continues to
            // service time-based wake-ups.
        }
    }
}
