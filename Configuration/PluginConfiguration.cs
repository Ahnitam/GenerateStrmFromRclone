using MediaBrowser.Model.Plugins;

namespace GenerateStrmFromRclone.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            rcloneServeUrl = null;
            rcloneRcUrl = null;
            rcloneAuthType = null;
            rcloneAuth = null;
            rcloneMediaPATH = null;
            rcloneRemoteDrive = null;
            rcloneDrivePATH = "";
        }
        public string? rcloneServeUrl { get; set; }
        public string? rcloneRcUrl { get; set; }
        public string? rcloneAuthType { get; set; }
        public string? rcloneAuth { get; set; }
        public string? rcloneRemoteDrive { get; set; }
        public string rcloneDrivePATH { get; set; }
        public string? rcloneMediaPATH { get; set; }
    }
}