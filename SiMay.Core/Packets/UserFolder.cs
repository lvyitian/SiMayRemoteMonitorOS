using SiMay.ReflectCache;

namespace SiMay.Core.Packets
{
    public class UserFolder : EntitySerializerBase
    {
        public string UserName { get; set; }
        public string USID { get; set; }
        public UserShellFolders[] UserShellFolders { get; set; }
    }

    public class UserShellFolders : EntitySerializerBase
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
