using Deduplicator.Cli.Models;
using Deduplicator.Cli.Services;
using Spectre.Console;

IFileScanner scanner = new PhysicalFileScanner();
IDeduplicationEngine engine = new DeduplicationEngine();
var filesToDelete = new List<FileData>(); // collects dupes for deletion

AnsiConsole.Write(new FigletText("deduplicator v1").Centered().Color(Color.Green));

var path = AnsiConsole.Ask<string>("Enter the [bold yellow]path[/] to scan:");

await AnsiConsole.Status().StartAsync("Analyzing...", async ctx => 
{
    var allFiles = scanner.ScanDirectory(path).ToList();
    
    await foreach (var group in engine.FindDuplicatesAsync(allFiles))
    {
        var duplicatesInGroup = group.Skip(1).ToList(); // keeps first // marks rest
        filesToDelete.AddRange(duplicatesInGroup);

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Path").AddColumn("Size");
        foreach (var file in group) 
            table.AddRow(file.FullPath, file.SizeInBytes.ToString("N0"));

        AnsiConsole.MarkupLine($"[bold green]Group found (Hash: {group.Key.Substring(0, 8)}...)[/]");
        AnsiConsole.Write(table);
    }
});

if (filesToDelete.Any())
{
    long totalSaved = filesToDelete.Sum(f => f.SizeInBytes);
    
    if (AnsiConsole.Confirm($"\nFound {filesToDelete.Count} duplicates. [red]Delete them[/] and save {totalSaved / 1024} KB?"))
    {
        await AnsiConsole.Progress()
            .StartAsync(async ctx => 
            {
                var task = ctx.AddTask("[red]Deleting files[/]");
                double increment = 100.0 / filesToDelete.Count;

                foreach (var file in filesToDelete)
                {
                    try {
                        File.Delete(file.FullPath);
                        AnsiConsole.MarkupLine($"[grey]Deleted:[/] {Path.GetFileName(file.FullPath)}");
                    } catch (Exception ex) {
                        AnsiConsole.MarkupLine($"[red]Failed:[/] {file.FullPath} ({ex.Message})");
                    }
                    task.Increment(increment);
                }
            });
        AnsiConsole.MarkupLine("[bold green]Cleanup complete![/]");
    }
}
else
{
    AnsiConsole.MarkupLine("[yellow]Nothing to delete.[/]");
}