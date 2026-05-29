// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Widgets;
using static GitHelper;
using static ProcessHelper;

Task<Result<string>> gitStatus = Run("git", "status", "--porcelain");

Task<Result<Worktree[]>> gitWorktrees = GetWorktrees();
Worktree? selectedWorktree = default;

List<string> log = [];

AsphaltApplication.Run(
    new Dimensions(80, 24),
    context =>
    {
        context.AddShortcutHint(label: "Q", value: "Quit");
        if (context.KeyDown(ConsoleKey.Q))
        {
            context.QuitAfterThisFrame();
        }

        using (context.VStack(grow: true))
        {
            using (context.Panel("Worktrees", style: Layout.Grow))
            {
                context.Await(
                    gitWorktrees,
                    (context, worktrees) =>
                    {
                        using (context.SelectableList(worktrees, worktree => $"{worktree?.Path ?? ""}", ref selectedWorktree))
                        {
                            if (context.KeyDown(ConsoleKey.Enter))
                            {
                                log.Add($"Selected worktree: {selectedWorktree}");
                            }
                            else if (context.KeyDown(ConsoleKey.N))
                            {
                                log.Add($"Create new worktree: {selectedWorktree}");
                            }
                            else if (context.KeyDown(ConsoleKey.E))
                            {
                                log.Add($"Edit worktree: {selectedWorktree}");
                            }
                            else if (context.KeyDown(ConsoleKey.D))
                            {
                                log.Add($"Delete worktree: {selectedWorktree}");
                            }

                            context.AddShortcutHint(label: "Enter", value: "Select");
                            context.AddShortcutHint(label: "N", value: "New");
                            context.AddShortcutHint(label: "E", value: "Edit");
                            context.AddShortcutHint(label: "D", value: "Delete");
                        }
                    }
                );
            }

            using (context.Panel("Git Status", style: Layout.Grow))
            {
                context.Await(gitStatus, (context, status) => context.Text(status));
            }

            using (context.Panel("Shortcuts", style: Layout.Grow))
            {
                string shortcutText = string.Join(" | ", context.ShortcutHints.Select(hint => $"{hint.Label}: {hint.Value}"));
                context.Text(shortcutText);
            }

            using (context.Panel("Log", style: Layout.Grow))
            {
                foreach (string entry in log)
                    context.Text(entry);
            }
        }
    }
);

internal static class CustomWidgets
{
    extension(AsphaltContext context)
    {
        public void Await<T>(Task<Result<T>> loadingTask, Action<AsphaltContext, T> onSuccess)
        {
            if (loadingTask.IsCompleted && loadingTask.Result is T result)
            {
                onSuccess(context, result);
            }
            else if (loadingTask.IsCompleted && loadingTask.Result is Error error)
            {
                context.Text(error.Message);
            }
            else if (loadingTask.IsFaulted)
            {
                context.Text("An error occurred while loading.");
            }
            else
            {
                context.Spinner();
            }
        }
    }
}
