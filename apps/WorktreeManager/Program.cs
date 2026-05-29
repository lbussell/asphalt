// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Widgets;
using static GitHelper;
using static ProcessHelper;

string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string worktreeDirectory = Path.Combine(userHomeDirectory, "w");
var getWorktreeDirectory = (string repoName, string worktreeName) =>
    Path.Combine(worktreeDirectory, repoName, worktreeName.Replace('/', '-'));

Task<Result<string>> gitStatus = Run("git", "status", "--porcelain");
Task<Result<Worktree[]>> gitWorktrees = GetWorktrees();
Task<Result<string>> repoNameTask = GetRepoName();
Worktree? selectedWorktree = default;

Stack<string> log = [];

// New-worktree modal state. The modal is a small wizard: enter a branch
// name, confirm, watch the create task run, then dismiss on success.
bool showNewModal = false;
string newBranchName = "";
Task<Result<string>>? createTask = null;

AsphaltApplication.Run(
    new Dimensions(80, 24),
    context =>
    {
        context.AddShortcutHint(label: "Q", value: "Quit");
        if (context.KeyDown(ConsoleKey.Q))
        {
            context.QuitAfterThisFrame();
        }

        using (context.Panel("Worktrees", style: Layout.Grow))
        {
            context.Await(
                gitWorktrees,
                (context, worktrees) =>
                {
                    using (
                        context.SelectableList(
                            worktrees,
                            worktree => $"{worktree?.Path ?? ""}",
                            ref selectedWorktree
                        )
                    )
                    {
                        context.AddShortcutHint(label: "N", value: "New");
                        if (context.KeyDown(ConsoleKey.N))
                        {
                            showNewModal = true;
                            newBranchName = "";
                            createTask = null;
                        }

                        context.AddShortcutHint(label: "D", value: "Remove");
                        if (context.KeyDown(ConsoleKey.D))
                        {
                            log.Push($"Remove worktree: {selectedWorktree}");
                        }
                    }
                }
            );
        }

        using (context.Panel("Git Status", style: Layout.Grow))
        {
            context.Await(gitStatus, (context, status) => context.Text(status));
        }

        using (context.Panel("Log"))
        {
            if (log.Count > 0)
            {
                context.Text(log.Peek(), wrappingMode: TextWrappingMode.Truncate);
            }
        }

        string shortcutText = string.Join(
            " | ",
            context.ShortcutHints.Select(hint => $"{hint.Label}: {hint.Value}")
        );
        context.Text(shortcutText);

        if (showNewModal)
        {
            using (context.Modal(Anchor.Center))
            using (context.Panel("New worktree"))
            {
                context.Await(
                    repoNameTask,
                    (context, repoName) =>
                    {
                        string path =
                            newBranchName.Length == 0
                                ? "(enter a branch name)"
                                : getWorktreeDirectory(repoName, newBranchName);

                        context.Text("Branch:");
                        context.InputText(ref newBranchName, placeholder: "feature/my-branch");

                        context.Text($"Path: {path}", wrappingMode: TextWrappingMode.Truncate);
                        context.Text("");

                        // While a create is in flight, show progress instead of
                        // the action buttons so the user can't double-submit.
                        if (createTask is { } pending)
                        {
                            if (!pending.IsCompleted)
                            {
                                context.Text("Creating...");
                                context.Spinner();
                            }
                            else if (pending.Result is string success)
                            {
                                log.Push($"Created worktree '{newBranchName}'.");
                                if (success.Trim() is { Length: > 0 } trimmed)
                                    log.Push(trimmed);
                                gitWorktrees = GetWorktrees();
                                showNewModal = false;
                                createTask = null;
                            }
                            else if (pending.Result is Error error)
                            {
                                context.Text(
                                    $"Error: {error.Message}",
                                    wrappingMode: TextWrappingMode.Wrap
                                );
                                context.Text("");
                                if (context.Button("Dismiss"))
                                    createTask = null;
                            }
                        }
                        else
                        {
                            using (context.HStack(gap: 2))
                            {
                                bool canCreate = newBranchName.Length > 0;
                                if (context.Button("Create") && canCreate)
                                {
                                    createTask = AddWorktree(
                                        getWorktreeDirectory(repoName, newBranchName),
                                        newBranchName
                                    );
                                }
                                if (context.Button("Cancel"))
                                {
                                    showNewModal = false;
                                }
                            }
                        }

                        // Escape always dismisses the modal, even mid-error.
                        if (
                            context.KeyDown(ConsoleKey.Escape)
                            && (createTask is null || createTask.IsCompleted)
                        )
                        {
                            showNewModal = false;
                            createTask = null;
                        }
                    }
                );
            }
        }
    }
);
