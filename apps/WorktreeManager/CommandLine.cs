// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text.Json.Nodes;

internal static class ProcessHelper
{
    public static async Task<Result<string>> Run(params string[] args)
    {
        ProcessTextOutput output = await Process.RunAndCaptureTextAsync(args[0], args[1..]);

        return output switch
        {
            { ExitStatus.ExitCode: 0 } => output.StandardOutput,

            { StandardError: var standardError } => new Error(
                $"Failed to execute git command: {standardError}"
            ),
        };
    }

    public static async Task<Result<JsonNode>> RunGitHubCli(params string[] args)
    {
        ProcessTextOutput output = await Process.RunAndCaptureTextAsync("gh", args);

        return output switch
        {
            { ExitStatus.ExitCode: 0 } => JsonNode.Parse(output.StandardOutput),

            { StandardError: var standardError } => new Error(
                $"Failed to execute GitHub CLI command: {standardError}"
            ),
        };
    }
}

internal static class GitHelper
{
    public static Worktree[] ParseWorktrees(string output) =>
        output
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(block =>
                block
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(line =>
                    {
                        int index = line.IndexOf(' ');
                        return index < 0
                            ? (Key: line, Value: "")
                            : (Key: line[..index], Value: line[(index + 1)..]);
                    })
                    .ToLookup(kv => kv.Key, kv => kv.Value)
            )
            .Select(fields => new Worktree(
                Path: fields["worktree"].FirstOrDefault() ?? "",
                Head: fields["HEAD"].FirstOrDefault() ?? "",
                Branch: fields["branch"].FirstOrDefault() ?? "",
                IsBare: fields.Contains("bare"),
                IsDetached: fields.Contains("detached"),
                IsLocked: fields.Contains("locked"),
                LockReason: fields["locked"].FirstOrDefault()
            ))
            .ToArray();

    public static Task<Result<Worktree[]>> GetWorktrees() =>
        ProcessHelper.Run("git", "worktree", "list", "--porcelain").Map(ParseWorktrees);
}

record Worktree(
    string Path,
    string Head,
    string Branch,
    bool IsBare,
    bool IsDetached,
    bool IsLocked,
    string? LockReason
);
