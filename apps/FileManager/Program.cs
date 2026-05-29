// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Widgets;

string currentDirectory = Directory.GetCurrentDirectory();
List<FileEntry> entries = ListEntries(currentDirectory);
int selectedIndex = 0;

AsphaltApplication.Run(
    context =>
    {
        using (context.HStack(grow: true))
        {
            using (context.Panel(currentDirectory, style: Layout.Grow))
            {
                using (
                    context.SelectableList<FileEntry>(
                        entries,
                        e => e.IsDirectory ? $"{e.Name}/" : e.Name,
                        ref selectedIndex
                    )
                )
                {
                    if (
                        context.KeyDown(ConsoleKey.Enter)
                        && entries.Count > 0
                        && entries[selectedIndex].IsDirectory
                    )
                    {
                        currentDirectory = Path.Combine(currentDirectory, entries[selectedIndex].Name);
                        currentDirectory = Path.GetFullPath(currentDirectory);
                        entries = ListEntries(currentDirectory);
                        selectedIndex = 0;
                    }
                }
            }

            FileEntry? hoveredFileEntry =
                entries.Count > 0 ? entries[Math.Clamp(selectedIndex, 0, entries.Count - 1)] : null;

            string detailsTitle = hoveredFileEntry?.Name ?? "Details";
            using (context.Panel(detailsTitle, style: Layout.Grow))
            {
                if (hoveredFileEntry is { IsDirectory: true } dir)
                {
                    string fullPath = Path.Combine(currentDirectory, dir.Name);
                    foreach (FileEntry child in ListEntries(fullPath))
                    {
                        context.Text(child.IsDirectory ? $"{child.Name}/" : child.Name);
                    }
                }
                else if (hoveredFileEntry is { IsDirectory: false } file)
                {
                    context.Text($"File: {file.Name}");
                }
                else
                {
                    context.Text("Select an entry to preview");
                }
            }
        }
    },
    altScreen: true
);

static List<FileEntry> ListEntries(string path)
{
    List<FileEntry> fileEntries = [];

    string? parentDirectory = Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(parentDirectory))
    {
        fileEntries.Add(FileEntry.Directory(".."));
    }

    try
    {
        fileEntries.AddRange(
            Directory
                .GetDirectories(path)
                .Select(p => FileEntry.Directory(Path.GetFileName(p)))
        );
        fileEntries.AddRange(
            Directory.GetFiles(path).Select(p => FileEntry.File(Path.GetFileName(p)))
        );
    }
    catch (UnauthorizedAccessException) { }
    catch (DirectoryNotFoundException) { }

    return fileEntries;
}

readonly record struct FileEntry(string Name, bool IsDirectory)
{
    public static FileEntry Directory(string name) => new FileEntry(name, true);
    public static FileEntry File(string name) => new FileEntry(name, false);
}
