using EuNet.Core;
using UnityEngine;

namespace EuNet.Unity
{
    public static class NetDataReaderExtension
    {
        public static Vector2 ReadVector2(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();

            return new Vector2(v0, v1);
        }

        public static Vector3 ReadVector3(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();
            var v2 = reader.ReadSingle();

            return new Vector3(v0, v1, v2);
        }

        public static Quaternion ReadQuaternion(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();
            var v2 = reader.ReadSingle();
            var v3 = reader.ReadSingle();

            return new Quaternion(v0, v1, v2, v3);
        }

        public static Color ReadColor(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();
            var v2 = reader.ReadSingle();
            var v3 = reader.ReadSingle();

            return new Color(v0, v1, v2, v3);
        }
    }
}
