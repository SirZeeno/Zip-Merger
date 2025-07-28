using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using ZipMerger.ViewModels;

namespace ZipMerger;

public abstract class HelperClass
{
    /// <summary>
    /// Calculates the MD5 checksum of a file asynchronously.
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>String containing the MD5 checksum</returns>
    internal static string GetMd5Checksum(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hash.GetAwaiter().GetResult()).Replace("-", "").ToLower();
    }

    public static bool CompareDirectories(string directory1, string directory2)
    {
        var files1 = Directory.GetFiles(directory1, "*.*", SearchOption.AllDirectories);
        var files2 = Directory.GetFiles(directory2, "*.*", SearchOption.AllDirectories);
        var uniqueFiles1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Case-insensitive hash set for unique hashes
        var uniqueFiles2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Case-insensitive hash set for unique hashes
        foreach (var file in files1)
        {
            string hash = GetMd5Checksum(file);
            if (!uniqueFiles1.Add(hash)) continue;
        }
        foreach (var file in files2)
        {
            string hash = GetMd5Checksum(file);
            if (!uniqueFiles2.Add(hash)) continue;
        }

        return uniqueFiles1.SetEquals(uniqueFiles2);
    }
}