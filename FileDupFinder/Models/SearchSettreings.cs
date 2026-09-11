namespace FileDupFinder.Models
{
    public class SearchSettings
    {
        public bool IsExtensionFilterEnabled { get; set; } = false;
        public string TargetExtensions { get; set; } = ".jpg,.mp4,.png";

        public bool CompareFileName { get; set; } = false;
        public bool CompareTimestamp { get; set; } = false;
        public bool CompareHash { get; set; } = true;
    }
}