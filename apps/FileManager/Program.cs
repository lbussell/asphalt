// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

string currentDirectory = Directory.GetCurrentDirectory();

List<FileEntry> files =
[
    ..Directory.GetDirectories(currentDirectory).Select(FileEntry.Directory),
    ..Directory.GetFiles(currentDirectory).Select(FileEntry.File),
];

AsphaltApplication.Run(
    context =>
    {
        using (context.HStack(grow: true))
        {
            using (context.Panel("Files", style: LayoutStyle.Grow))
            {
                foreach (FileEntry file in files)
                {
                    if (file.IsDirectory)
                    {
                        context.Text($"{file.Path}/", foregroundColor: TerminalColor.Cyan);
                    }
                    else
                    {
                        context.Text($"{file.Path}");
                    }
                }
            }

            using (context.Panel("Details", style: LayoutStyle.Grow))
            {
                context.Text("Hello details");
            }
        }
    },
    altScreen: true
);

readonly record struct FileEntry(string Path, bool IsDirectory)
{
    public static FileEntry Directory(string path) => new FileEntry(path, true);
    public static FileEntry File(string path) => new FileEntry(path, false);
}
