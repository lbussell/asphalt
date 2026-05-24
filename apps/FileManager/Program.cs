// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Widgets;

string currentDirectory = Directory.GetCurrentDirectory();
List<FileEntry> entries = ListEntries(currentDirectory);

AsphaltApplication.Run(
    context =>
    {
        FileEntry? hoveredFileEntry = null;

        using (context.HStack(grow: true))
        {
            using (context.Panel(currentDirectory, style: LayoutStyle.Grow))
            {
                foreach (FileEntry entry in entries)
                {
                    string label = entry.IsDirectory ? $"{entry.Name}/" : entry.Name;

                    SelectableState state = context.Selectable(label, uniqueKey: entry.Name);

                    if (state.Focused)
                    {
                        hoveredFileEntry = entry;
                    }

                    if (state.Activated && entry.IsDirectory)
                    {
                        currentDirectory = Path.Combine(currentDirectory, entry.Name);
                        currentDirectory = Path.GetFullPath(currentDirectory);
                        entries = ListEntries(currentDirectory);
                        hoveredFileEntry = null;
                        break;
                    }
                }
            }

            string detailsTitle = hoveredFileEntry?.Name ?? "Details";
            using (context.Panel(detailsTitle, style: LayoutStyle.Grow))
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
