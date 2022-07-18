using MediaBrowser.Model.Plugins;

namespace GenerateStrmFromRclone.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            rcloneServeIP = null;
            rcloneServePort = 5000;
            rcloneRcPort = 5572;
            rclonePATH = null;
            rcloneConfigPATH = null;
            rcloneMediaPATH = null;
            rcloneRemoteDrive = null;
            rcloneDrivePATH = "";
        }
        public string? rcloneServeIP { get; set; }
        public int rcloneServePort { get; set; }
        public int rcloneRcPort { get; set; }
        public string? rclonePATH { get; set; }
        public string? rcloneConfigPATH { get; set; }
        public string? rcloneRemoteDrive { get; set; }
        public string rcloneDrivePATH { get; set; }
        public string? rcloneMediaPATH { get; set; }
    }
}