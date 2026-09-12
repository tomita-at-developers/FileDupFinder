namespace FileDupFinder.Models
{
    /// <summary>
    /// 重複ファイルの検索条件および比較アルゴリズムの動作設定を保持するデータモデルです。
    /// メイン画面と設定画面の間でインスタンスを共有し、リアルタイムに設定状態を維持します。
    /// </summary>
    public class SearchSettings
    {
        /// <summary>
        /// 特定の拡張子を持つファイルのみをスキャンの対象にするかどうかのフラグ。
        /// true の場合、<see cref="TargetExtensions"/> に指定されたファイル形式のみを処理します。
        /// </summary>
        public bool IsExtensionFilterEnabled { get; set; } = false;

        /// <summary>
        /// 検索対象とする拡張子のリスト。カンマ区切りで複数指定が可能です。
        /// 内部ではすべて小文字に統一して評価されます。(例: ".jpg,.mp4,.png")
        /// </summary>
        public string TargetExtensions { get; set; } = ".jpg,.mp4,.png";

        /// <summary>
        /// 一致条件：ファイル名が完全に一致していることも重複の条件に含めるかどうか。
        /// </summary>
        public bool CompareFileName { get; set; } = false;

        /// <summary>
        /// 一致条件：タイムスタンプ（最終更新日時）が一致していることも重複の条件に含めるかどうか。
        /// </summary>
        public bool CompareTimestamp { get; set; } = false;

        /// <summary>
        /// 一致条件：ファイル内容のハッシュ値（SHA-256）が一致していることを重複の条件にするかどうか。
        /// </summary>
        public bool CompareHash { get; set; } = true;
    }
}
