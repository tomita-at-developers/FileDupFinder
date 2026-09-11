using System;
using System.IO;
using System.Security.Cryptography;

namespace FileDupFinder.Models
{
    public class FileInfoItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public string FolderPath => Path.GetDirectoryName(FilePath) ?? string.Empty;
        public long FileSize { get; set; }
        public string DisplaySize => $"{(FileSize / 1024.0 / 1024.0):F2} MB";
        public string Hash { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }

        public static string CalculateHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                return string.Empty; // アクセス権限のないファイルなどの対策
            }
        }
    }
}
