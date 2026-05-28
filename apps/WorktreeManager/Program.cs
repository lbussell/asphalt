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

int applicationWidth = Math.Min(Console.WindowWidth, 80);
int applicationHeight = Math.Min(Console.WindowHeight, 24);
Dimensions applicationSize = new Dimensions(applicationWidth, applicationHeight);

AsphaltApplication.Run(
    context =>
    {
        using (context.Container(applicationSize))
        {
            using (context.Panel("Worktrees", style: LayoutStyle.Grow))
            {
                context.Await(
                    gitWorktrees,
                    (context, worktrees) =>
                    {
                        bool activated = context.SelectableList(
                            items: worktrees,
                            display: worktree => $"{worktree?.Path ?? ""}",
                            selected: ref selectedWorktree
                        );

                        if (activated)
                            log.Add($"Selected worktree: {selectedWorktree}");
                    }
                );
            }

            using (context.Panel("Git Status", style: LayoutStyle.Grow))
            {
                context.Await(gitStatus, (context, status) => context.Text(status));
            }

            using (context.Panel("Log", style: LayoutStyle.Grow))
            {
                foreach (string entry in log)
                    context.Text(entry);
            }
        }
    },
    altScreen: false
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
