// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Widgets;

const float maxSeconds = 60f;

float durationSeconds = 10f;
bool running = false;
TimeSpan startTime = TimeSpan.Zero;

AsphaltApplication.Run(asphalt =>
{
    float remaining;
    if (running)
    {
        double elapsed = (asphalt.Time - startTime).TotalSeconds;
        remaining = (float)Math.Max(0.0, durationSeconds - elapsed);

        if (remaining <= 0f)
        {
            running = false;
            remaining = 0f;
        }
        else
        {
            // Keep the run loop waking so the bar animates while counting down.
            asphalt.RequestRedrawIn(TimeSpan.FromMilliseconds(100));
        }
    }
    else
    {
        remaining = durationSeconds;
    }

    // Fraction of time left, so the bar drains from full toward empty.
    float progress = durationSeconds <= 0f ? 0f : remaining / durationSeconds;

    using (asphalt.Panel("Countdown Timer"))
    {
        asphalt.Text($"{remaining:0.0}s remaining");

        asphalt.ProgressBar(progress);

        using (asphalt.HStack(gap: 1))
        {
            asphalt.Text("Duration");
            asphalt.Slider(ref durationSeconds, min: 0f, max: maxSeconds, step: 1f);
            asphalt.Text($"{durationSeconds:0}s");
        }

        using (asphalt.HStack(gap: 1))
        {
            if (asphalt.Button(running ? "Stop" : "Start"))
            {
                if (running)
                {
                    running = false;
                }
                else if (durationSeconds > 0f)
                {
                    running = true;
                    startTime = asphalt.Time;
                }
            }

            if (asphalt.Button("Quit"))
                asphalt.QuitAfterThisFrame();
        }
    }
});
