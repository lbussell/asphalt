#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:package Fluid.Core@2.31.0

using Fluid;
using Fluid.Values;

var parser = new FluidParser();

await Render(
    template: "./scripts/templates/samples.md.liquid",
    output: "./samples/README.md",
    model: new { samples = GetSamples() });

static IEnumerable<Sample> GetSamples()
{
    const string samplesDir = "./samples";
    return Directory
        .GetFiles(samplesDir, "*.cs")
        .Select(path => new Sample(Path.GetFileName(path), path))
        .ToList();
}

async Task Render(string template, string output, object model)
{
    var options = new TemplateOptions();
    options.Filters.AddFilter("code_segment", CodeSegment);
    options.MemberAccessStrategy.Register<Sample>();

    var parsed = parser.Parse(File.ReadAllText(template));
    var context = new TemplateContext(model, options);
    context.SetValue("templateFile", template);
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

record Sample(string Name, string Path);
