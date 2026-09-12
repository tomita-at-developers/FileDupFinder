using System;
using System.IO;
using System.Security.Cryptography;

namespace FileDupFinder.Models
{
    /// <summary>
    /// スキャンされた個々のファイル情報を保持する純粋なデータモデルクラスです。
    /// 画面表示に必要なサイズ変換プロパティや、重いハッシュ計算のロジックを内包しています。
    /// </summary>
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

        /// <summary>
        /// 指定されたパスのファイルを開き、ストリームを分割しながらハッシュ（SHA-256）を計算します。
        /// 大容量ファイルを読み込んでもメモリ（RAM）を圧迫しない設計です。
        /// </summary>
        public static string CalculateHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = sha256.ComputeHash(stream);
                // 💡 .NET 9.0/10.0以降の環境に最適化された、超高速・省メモリな16進数小文字化関数
                return Convert.ToHexStringLower(hashBytes);
            }
            catch
            {
                // アクセス権限のない保護ファイルや、別プロセスがロック中のファイルは安全にスキップ
                return string.Empty;
            }
        }
    }
}
