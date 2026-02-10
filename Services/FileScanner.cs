using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Deduplicator.Cli.Models;

namespace Deduplicator.Cli.Services;

public interface IFileScanner
{
    IEnumerable<FileData> ScanDirectory(string roothPath);
}

public class PhysicalFileScanner : IFileScanner
{
    public IEnumerable<FileData> ScanDirectory(string roothPath)
    {
        if (!Directory.Exists(roothPath))
        {
            throw new DirectoryNotFoundException($"path {roothPath} does not exist");
        }
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.System
        };
        var directoryInfo = new DirectoryInfo(roothPath);

        foreach (var file in directoryInfo.EnumerateFiles("*", options))
        {
            FileData? data = null;

            try
            {
                data = new FileData(file.FullName, file.Length);
            }
             catch (Exception ex) when ( ex is UnauthorizedAccessException or IOException)
            {
                Console.WriteLine($"[warning]  could not access {file.Name}: {ex.Message}");
                continue;
            }
            
            if (data != null)
            {
                yield return data; // stream res 1 by 1
            }
        }
    }
}