using EuNet.Core;
using UnityEngine;

namespace EuNet.Unity
{
    public static class NetDataReaderExtensions
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

        public static Vector4 ReadVector4(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();
            var v2 = reader.ReadSingle();
            var v3 = reader.ReadSingle();

            return new Vector4(v0, v1, v2, v3);
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

        public static Bounds ReadBounds(this NetDataReader reader)
        {
            var v0 = reader.ReadVector3();
            var v1 = reader.ReadVector3();

            return new Bounds(v0, v1);
        }

        public static Rect ReadRect(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();
            var v2 = reader.ReadSingle();
            var v3 = reader.ReadSingle();

            return new Rect(v0, v1, v2, v3);
        }

        public static Keyframe ReadKeyframe(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();
            var v2 = reader.ReadSingle();
            var v3 = reader.ReadSingle();

            return new Keyframe(v0, v1, v2, v3);
        }

        public static AnimationCurve ReadAnimationCurve(this NetDataReader reader)
        {
            var result = new AnimationCurve();

            var length = reader.ReadInt32();
            result.keys = new Keyframe[length];

            for (int i = 0; i < length; ++i)
                result.keys[i] = reader.ReadKeyframe();

            result.preWrapMode = (WrapMode)reader.ReadByte();
            result.postWrapMode = (WrapMode)reader.ReadByte();

            return result;
        }

        public static Matrix4x4 ReadMatrix4x4(this NetDataReader reader)
        {
            var result = new Matrix4x4();

            result.m00 = reader.ReadSingle();
            result.m10 = reader.ReadSingle();
            result.m20 = reader.ReadSingle();
            result.m30 = reader.ReadSingle();
            result.m01 = reader.ReadSingle();
            result.m11 = reader.ReadSingle();
            result.m21 = reader.ReadSingle();
            result.m31 = reader.ReadSingle();
            result.m02 = reader.ReadSingle();
            result.m12 = reader.ReadSingle();
            result.m22 = reader.ReadSingle();
            result.m32 = reader.ReadSingle();
            result.m03 = reader.ReadSingle();
            result.m13 = reader.ReadSingle();
            result.m23 = reader.ReadSingle();
            result.m33 = reader.ReadSingle();

            return result;
        }

        public static GradientColorKey ReadGradientColorKey(this NetDataReader reader)
        {
            var v0 = reader.ReadColor();
            var v1 = reader.ReadSingle();

            return new GradientColorKey(v0, v1);
        }

        public static GradientAlphaKey ReadGradientAlphaKey(this NetDataReader reader)
        {
            var v0 = reader.ReadSingle();
            var v1 = reader.ReadSingle();

            return new GradientAlphaKey(v0, v1);
        }

        public static Gradient ReadGradient(this NetDataReader reader)
        {
            var result = new Gradient();

            var length = reader.ReadInt32();
            result.colorKeys = new GradientColorKey[length];
            for (int i = 0; i < length; ++i)
                result.colorKeys[i] = reader.ReadGradientColorKey();

            length = reader.ReadInt32();
            result.alphaKeys = new GradientAlphaKey[length];
            for (int i = 0; i < length; ++i)
                result.alphaKeys[i] = reader.ReadGradientAlphaKey();

            result.mode = (GradientMode)reader.ReadByte();

            return result;
        }

        public static Color32 ReadColor32(this NetDataReader reader)
        {
            var v0 = reader.ReadByte();
            var v1 = reader.ReadByte();
            var v2 = reader.ReadByte();
            var v3 = reader.ReadByte();

            return new Color32(v0, v1, v2, v3);
        }

        public static RectOffset ReadRectOffset(this NetDataReader reader)
        {
            var v0 = reader.ReadInt32();
            var v1 = reader.ReadInt32();
            var v2 = reader.ReadInt32();
            var v3 = reader.ReadInt32();

            return new RectOffset(v0, v1, v2, v3);
        }

        public static LayerMask ReadLayerMask(this NetDataReader reader)
        {
            var result = new LayerMask();
            result.value = reader.ReadInt32();
            
            return result;
        }

        public static Vector2Int ReadVector2Int(this NetDataReader reader)
        {
            var v0 = reader.ReadInt32();
            var v1 = reader.ReadInt32();

            return new Vector2Int(v0, v1);
        }

        public static Vector3Int ReadVector3Int(this NetDataReader reader)
        {
            var v0 = reader.ReadInt32();
            var v1 = reader.ReadInt32();
            var v2 = reader.ReadInt32();

            return new Vector3Int(v0, v1, v2);
        }

        public static RangeInt ReadRangeInt(this NetDataReader reader)
        {
            var v0 = reader.ReadInt32();
            var v1 = reader.ReadInt32();

            return new RangeInt(v0, v1);
        }

        public static RectInt ReadRectInt(this NetDataReader reader)
        {
            var v0 = reader.ReadInt32();
            var v1 = reader.ReadInt32();
            var v2 = reader.ReadInt32();
            var v3 = reader.ReadInt32();

            return new RectInt(v0, v1, v2, v3);
        }

        public static BoundsInt ReadBoundsInt(this NetDataReader reader)
        {
            var v0 = reader.ReadVector3Int();
            var v1 = reader.ReadVector3Int();

            return new BoundsInt(v0, v1);
        }
    }
}
