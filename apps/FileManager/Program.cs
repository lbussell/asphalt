// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Asphalt;
using Asphalt.Widgets;

string currentDirectory = Directory.GetCurrentDirectory();
string selectedFile = "Select a file";

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
                    string label = file.IsDirectory ? $"{file.Path}/" : file.Path;

                    if (context.Selectable(label, uniqueKey: file.Path))
                    {
                        selectedFile = file.Path;
                    }
                }
            }

            using (context.Panel(selectedFile, style: LayoutStyle.Grow))
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
