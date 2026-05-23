#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

#region Example
Task<string>? fetch = null;
CancellationTokenSource? cts = null;
AsphaltApplication.Run(
    asphalt =>
    {
        using (asphalt.Panel("Loading Example"))
        {
            asphalt.Text("A simulated network fetch wakes the run loop on completion.");
            asphalt.HRule();

            string buttonLabel = "Fetch";
            if (fetch is not null)
            {
                if (fetch.IsCompletedSuccessfully)
                {
                    asphalt.Text($"Result: {fetch.Result}");
                    buttonLabel = "Refetch";
                }
                else if (fetch.IsFaulted)
                {
                    asphalt.Text(
                        $"Failed: {fetch.Exception?.InnerException?.Message ?? "unknown error"}"
                    );
                    buttonLabel = "Try again";
                }
                else if (fetch.IsCanceled)
                {
                    asphalt.Text("Canceled");
                    buttonLabel = "Try again";
                }
                else
                {
                    asphalt.Spinner();
                    buttonLabel = "Cancel";
                }
            }
            else
            {
                asphalt.Text("Press the button to load.");
            }

            asphalt.HRule();
            if (asphalt.Button(buttonLabel))
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
                asphalt.WakeOn(fetch);
                // Re-draw immediately since the button is below the spinner, but
                // we want the spinner to show up right away.
                asphalt.RequestRedrawIn(TimeSpan.Zero);
            }

            if (asphalt.Button("Quit"))
                asphalt.QuitAfterThisFrame();

            asphalt.Text($"Frame Count: {asphalt.FrameCount}");
        }
    }
);
#endregion Example

static async Task<string> FetchAsync(CancellationToken cancellationToken = default)
{
    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    return "Hello from the network!";
}
