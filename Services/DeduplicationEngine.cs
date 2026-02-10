using System.Security.Cryptography;
using Deduplicator.Cli.Models;

namespace Deduplicator.Cli.Services;
public interface IDeduplicationEngine
{
    IAsyncEnumerable<IGrouping<string, FileData>> FindDuplicatesAsync(IEnumerable<FileData> files, CancellationToken ct = default);
}

public class DeduplicationEngine : IDeduplicationEngine
{
    public async IAsyncEnumerable<IGrouping<string, FileData>> FindDuplicatesAsync(
        IEnumerable<FileData> files, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var potentialDuplicates = files     // file size filter
            .GroupBy(f => f.SizeInBytes)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g);

        var hashedFiles = new List<FileData>();

        foreach (var file in potentialDuplicates)
        {
            if (ct.IsCancellationRequested) break;

            string hash = await ComputeHashAsync(file.FullPath, ct);
            hashedFiles.Add(file with { Hash = hash });
        }

        var finalGroups = hashedFiles      // hash group (duplicates)
            .Where(f => f.Hash != null)
            .GroupBy(f => f.Hash!)
            .Where(g => g.Count() > 1);

        foreach (var group in finalGroups)
        {
            yield return group;
        }
    }
    private static async Task<string> ComputeHashAsync(string filePath, CancellationToken ct)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            byte[] hashBytes = await SHA256.HashDataAsync(stream, ct);
            
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)   // kinda weak error handling, needs better solution
        {
            Console.WriteLine($"[Error] Could not hash {filePath}: {ex.Message}");
            return string.Empty;
        }
    }
}