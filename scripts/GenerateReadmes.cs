#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:package Fluid.Core@2.31.0

using System.Diagnostics;
using System.Text.RegularExpressions;
using Fluid;
using Fluid.Values;

var parser = new FluidParser();

await Render(template: "./samples/README.template.md", output: "./samples/README.md");

async Task Render(string template, string output)
{
    var options = new TemplateOptions();
    options.Filters.AddFilter("code_segment", CodeSegment);
    options.Filters.AddFilter("vhs", Vhs);

    var parsed = parser.Parse(File.ReadAllText(template));
    var context = new TemplateContext(options);
    context.SetValue("templateFile", template);
    context.AmbientValues["templateDir"] = Path.GetDirectoryName(Path.GetFullPath(template))!;
    var rendered = await parsed.RenderAsync(context);

    File.WriteAllText(output, rendered);
}

static ValueTask<FluidValue> CodeSegment(
    FluidValue input,
    FilterArguments arguments,
    TemplateContext context
)
{
    var path = input.ToStringValue();
    var marker = arguments.At(0).ToStringValue();
    var lang = arguments.Count > 1 ? arguments.At(1).ToStringValue() : "";

    if (
        !Path.IsPathRooted(path)
        && context.AmbientValues.TryGetValue("templateDir", out var dirObj)
        && dirObj is string templateDir
    )
    {
        path = Path.Combine(templateDir, path);
    }

    if (!File.Exists(path))
        return new StringValue($"<!-- missing file: {path} -->");

    var lines = File.ReadAllLines(path);
    var startTag = $"#region {marker}";
    const string endTag = "#endregion";

    var collected = new List<string>();
    var inSegment = false;
    int indent = 0;

    foreach (var line in lines)
    {
        if (!inSegment && line.Contains(startTag))
        {
            inSegment = true;
            indent = line.TakeWhile(char.IsWhiteSpace).Count();
            continue;
        }
        if (inSegment && line.Contains(endTag))
            break;
        if (inSegment)
            collected.Add(line.Length >= indent ? line[indent..] : line);
    }

    return new StringValue($"```{lang}\n{string.Join("\n", collected)}\n```");
}

static async ValueTask<FluidValue> Vhs(
    FluidValue input,
    FilterArguments arguments,
    TemplateContext context
)
{
    var tape = input.ToStringValue();
    var templateDir = (string)context.AmbientValues["templateDir"];

    var match = Regex.Match(
        tape,
        @"^\s*(?:Screenshot|Output)\s+""(?<file>[^""]+)""",
        RegexOptions.Multiline
    );
    if (!match.Success)
        return new StringValue("<!-- vhs: tape is missing a Screenshot or Output directive -->");
    var outputName = match.Groups["file"].Value;

    var tempTape = Path.Combine(templateDir, $".tmp-{Guid.NewGuid():N}.tape");
    await File.WriteAllTextAsync(tempTape, tape);
    try
    {
        Console.Error.WriteLine($"vhs -> {outputName}");
        var startInfo = new ProcessStartInfo("vhs", [Path.GetFileName(tempTape)])
        {
            WorkingDirectory = templateDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        var result = await Process.RunAndCaptureTextAsync(startInfo);
        if (result.ExitStatus.ExitCode != 0)
            throw new Exception(
                $"vhs failed for {outputName} (exit {result.ExitStatus.ExitCode}):\n{result.StandardError}"
            );
    }
    finally
    {
        File.Delete(tempTape);
    }

    return new StringValue($"![{outputName}]({outputName})");
}
