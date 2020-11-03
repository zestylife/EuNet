using EuNet.Core;
using UnityEngine;

namespace EuNet.Unity
{
    public static class NetDataWriterExtensions
    {
        public static void Write(this NetDataWriter writer, Vector2 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        public static void Write(this NetDataWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public static void Write(this NetDataWriter writer, Vector4 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public static void Write(this NetDataWriter writer, Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public static void Write(this NetDataWriter writer, Color value)
        {
            writer.Write(value.r);
            writer.Write(value.g);
            writer.Write(value.b);
            writer.Write(value.a);
        }

        public static void Write(this NetDataWriter writer, Bounds value)
        {
            writer.Write(value.center);
            writer.Write(value.extents);
        }

        public static void Write(this NetDataWriter writer, Rect value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.width);
            writer.Write(value.height);
        }

        public static void Write(this NetDataWriter writer, Keyframe value)
        {
            writer.Write(value.time);
            writer.Write(value.value);
            writer.Write(value.inTangent);
            writer.Write(value.outTangent);
        }

        public static void Write(this NetDataWriter writer, AnimationCurve value)
        {
            writer.Write(value.keys.Length);
            foreach (var key in value.keys)
                writer.Write(key);

            writer.Write((byte)value.preWrapMode);
            writer.Write((byte)value.postWrapMode);
        }

        public static void Write(this NetDataWriter writer, Matrix4x4 value)
        {
            writer.Write(value.m00);
            writer.Write(value.m10);
            writer.Write(value.m20);
            writer.Write(value.m30);
            writer.Write(value.m01);
            writer.Write(value.m11);
            writer.Write(value.m21);
            writer.Write(value.m31);
            writer.Write(value.m02);
            writer.Write(value.m12);
            writer.Write(value.m22);
            writer.Write(value.m32);
            writer.Write(value.m03);
            writer.Write(value.m13);
            writer.Write(value.m23);
            writer.Write(value.m33);
        }

        public static void Write(this NetDataWriter writer, GradientColorKey value)
        {
            writer.Write(value.color);
            writer.Write(value.time);
        }

        public static void Write(this NetDataWriter writer, GradientAlphaKey value)
        {
            writer.Write(value.alpha);
            writer.Write(value.time);
        }

        public static void Write(this NetDataWriter writer, Gradient value)
        {
            writer.Write(value.colorKeys.Length);
            foreach (var key in value.colorKeys)
                writer.Write(key);

            writer.Write(value.alphaKeys.Length);
            foreach (var key in value.alphaKeys)
                writer.Write(key);

            writer.Write((byte)value.mode);
        }

        public static void Write(this NetDataWriter writer, Color32 value)
        {
            writer.Write(value.r);
            writer.Write(value.g);
            writer.Write(value.b);
            writer.Write(value.a);
        }

        public static void Write(this NetDataWriter writer, RectOffset value)
        {
            writer.Write(value.left);
            writer.Write(value.right);
            writer.Write(value.top);
            writer.Write(value.bottom);
        }

        public static void Write(this NetDataWriter writer, LayerMask value)
        {
            writer.Write(value.value);
        }

        public static void Write(this NetDataWriter writer, Vector2Int value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        public static void Write(this NetDataWriter writer, Vector3Int value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public static void Write(this NetDataWriter writer, RangeInt value)
        {
            writer.Write(value.start);
            writer.Write(value.length);
        }

        public static void Write(this NetDataWriter writer, RectInt value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.width);
            writer.Write(value.height);
        }

        public static void Write(this NetDataWriter writer, BoundsInt value)
        {
            writer.Write(value.position);
            writer.Write(value.size);
        }
    }
}
