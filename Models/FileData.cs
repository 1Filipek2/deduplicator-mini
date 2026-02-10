namespace Deduplicator.Cli.Models;

public record FileData (
    string FullPath,
    long SizeInBytes,
    string? Hash = null
);