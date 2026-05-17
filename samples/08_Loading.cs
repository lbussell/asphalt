#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Imtui/Imtui.csproj

using Imtui;
using Imtui.Widgets;

#region Example
Task<string>? fetch = null;
CancellationTokenSource? cts = null;
ImtuiApplication.Run(imtui =>
{
    bool keepRunning = true;
    using (imtui.BorderPanel("Loading Example"))
    {
        imtui.Text("A simulated network fetch wakes the run loop on completion.");
        imtui.HRule();

        string buttonLabel = "Fetch";
        if (fetch is not null)
        {
            if (fetch.IsCompletedSuccessfully)
            {
                imtui.Text($"Result: {fetch.Result}");
                buttonLabel = "Refetch";
            }
            else if (fetch.IsFaulted)
            {
                imtui.Text($"Failed: {fetch.Exception?.InnerException?.Message ?? "unknown error"}");
                buttonLabel = "Try again";
            }
            else if (fetch.IsCanceled)
            {
                imtui.Text("Canceled");
                buttonLabel = "Try again";
            }
            else
            {
                imtui.Spinner();
                buttonLabel = "Cancel";
            }
        }
        else
        {
            imtui.Text("Press the button to load.");
        }
        imtui.HRule();
        if (imtui.Button(buttonLabel))
        {
            if (fetch?.IsCompleted ?? true)
            {
                cts = new CancellationTokenSource();
                fetch = FetchAsync(cts.Token);
            }
            else
            {
                cts?.Cancel();
                cts?.Dispose();
            }

            // Re-draw as soon as the fetch completes so the UI will be updated
            // with the result.
            imtui.WakeOn(fetch);
            // Re-draw immediately since the button is below the spinner, but
            // we want the spinner to show up right away.
            imtui.RequestRedrawIn(TimeSpan.Zero);
        }

        if (imtui.Button("Quit"))
            keepRunning = false;

        imtui.Text($"Frame Count: {imtui.FrameCount}");
    }
    return keepRunning;
});
#endregion Example

static async Task<string> FetchAsync(CancellationToken cancellationToken = default)
{
    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    return "Hello from the network!";
}
