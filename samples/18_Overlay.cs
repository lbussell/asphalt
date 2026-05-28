#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Widgets;

// Overlays are detached subtrees that do not consume space in their parent
// and render on top of the primary layout.

bool showModal = false;
TimeSpan toastDuration = TimeSpan.FromSeconds(3);
TimeSpan? toastSpawnedAt = null;

AsphaltApplication.Run(
    context =>
    {
        using (context.Panel("Overlay demo", style: LayoutStyle.Grow))
        {
            context.Text("Press T to add a toast.");
            context.Text("Press M to toggle the modal.");
            context.Text("Press Q to quit.");
        }

        if (context.KeyDown(ConsoleKey.M))
            showModal = !showModal;

        if (context.KeyDown(ConsoleKey.Q))
            context.QuitAfterThisFrame();

        if (context.KeyDown(ConsoleKey.T))
            toastSpawnedAt = context.Time;

        // Show the toast for `toastDuration` after it spawns. Use context.Time
        // (the frame's logical clock) and ask the loop to wake up again
        // exactly when the toast should disappear. Otherwise, the loop only
        // re-renders on the next keypress.
        if (toastSpawnedAt is not null)
        {
            TimeSpan remaining = toastDuration - (context.Time - toastSpawnedAt.Value);
            if (remaining > TimeSpan.Zero)
            {
                using (context.Overlay(Anchor.Bottom | Anchor.Right))
                using (context.Panel("Toast"))
                {
                    context.Text("Hello there!");
                }
                context.RequestRedrawIn(remaining);
            }
            else
            {
                toastSpawnedAt = null;
            }
        }

        if (showModal)
        {
            using (context.Overlay(Anchor.Center))
            using (context.Panel("Modal"))
            {
                context.Text("This panel is centered over everything else.");
                context.Text("Press M again to dismiss.");
            }
        }
    },
    altScreen: true
);
