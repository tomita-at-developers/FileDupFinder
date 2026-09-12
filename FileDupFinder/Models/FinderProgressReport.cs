namespace FileDupFinder.Models
{
    /// <summary>
    /// バックグラウンドのワーカースレッド（重い計算処理を行う側）から、
    /// UIのメインスレッド（画面を描画する側）へ、テキストとプログレスバーの数値を
    /// まとめて安全かつ一元的に引き渡すために定義された進捗データ専用のデータ構造クラスです。
    /// </summary>
    public class FinderProgressReport
    {
        /// <summary>
        /// 画面下のステータスラベルにリアルタイム表示する進捗文字列（例：「STEP 2/4：重複ファイル検索中...」）。
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// プログレスバーの現在の進捗値（分子）。現在処理が完了したファイル数をセットします。
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// プログレスバーの最大値（分母）。処理対象となっているファイルの総数をセットします。
        /// </summary>
        public double Maximum { get; set; }

        /// <summary>
        /// プログレスバー自体を画面に「表示する（Visible）」か「隠す（Collapsed）」かを制御するWPF用の可視性ステータス。
        /// </summary>
        public System.Windows.Visibility ProgressBarVisibility { get; set; } = System.Windows.Visibility.Collapsed;
    }
}
