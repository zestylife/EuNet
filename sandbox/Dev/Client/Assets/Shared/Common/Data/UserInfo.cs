using EuNet.Core;

namespace Common
{
    [NetDataObject]
    public class UserInfo
    {
        public string Name;
        public int Int;
        public string String;
        public int Property { get; set; }
    }
}
