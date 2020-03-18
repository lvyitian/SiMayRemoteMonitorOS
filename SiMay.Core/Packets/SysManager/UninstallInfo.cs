namespace SiMay.Core.Packets
{
    public class UninstallInfo : ReflectCache.EntitySerializerBase
    {
        public string DisplayName { get; set; }
        public string InstallLocation { get; set; }
        public string UninstallString { get; set; }
        public string ReleaseType { get; set; }
        public string DisplayVersion { get; set; }
        public string Publisher { get; set; }
        public string InstallSource { get; set; }
        public string Size { get; set; }
        public string InstallDate { get; set; }
    }
}
