#if !(UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS)

namespace UnityEngine
{
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator *(Vector3 a, float d)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }
    }

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }

    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b)
            : this(r, g, b, 1.0f)
        {
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public struct Bounds
    {
        public Vector3 center;
        public Vector3 extents;

        public Vector3 size
        {
            get
            {
                return this.extents * 2f;
            }

            set
            {
                this.extents = value * 0.5f;
            }
        }

        public Bounds(Vector3 center, Vector3 size)
        {
            this.center = center;
            this.extents = size * 0.5f;
        }
    }

    public struct Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rect(Vector2 position, Vector2 size)
        {
            this.x = position.x;
            this.y = position.y;
            this.width = size.x;
            this.height = size.y;
        }

        public Rect(Rect source)
        {
            this.x = source.x;
            this.y = source.y;
            this.width = source.width;
            this.height = source.height;
        }
    }

    public struct Keyframe
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;

        public Keyframe(float time, float value)
        {
            this.time = time;
            this.value = value;
            this.inTangent = 0f;
            this.outTangent = 0f;
        }

        public Keyframe(float time, float value, float inTangent, float outTangent)
        {
            this.time = time;
            this.value = value;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
        }
    }

    public enum WrapMode
    {
        Once = 1,
        Loop,
        PingPong = 4,
        Default = 0,
        ClampForever = 8,
        Clamp = 1,
    }

    public sealed class AnimationCurve
    {
        public Keyframe[] keys;

        public int length
        {
            get { return this.keys.Length; }
        }

        public WrapMode preWrapMode;
        public WrapMode postWrapMode;
    }

    public struct Matrix4x4
    {
        public float m00;
        public float m10;
        public float m20;
        public float m30;
        public float m01;
        public float m11;
        public float m21;
        public float m31;
        public float m02;
        public float m12;
        public float m22;
        public float m32;
        public float m03;
        public float m13;
        public float m23;
        public float m33;
    }

    public struct GradientColorKey
    {
        public Color color;
        public float time;

        public GradientColorKey(Color col, float time)
        {
            this.color = col;
            this.time = time;
        }
    }

    public struct GradientAlphaKey
    {
        public float alpha;
        public float time;

        public GradientAlphaKey(float alpha, float time)
        {
            this.alpha = alpha;
            this.time = time;
        }
    }

    public enum GradientMode
    {
        Blend,
        Fixed,
    }

    public sealed class Gradient
    {
        public GradientColorKey[] colorKeys;
        public GradientAlphaKey[] alphaKeys;
        public GradientMode mode;
    }

    public struct Color32
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public Color32(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public sealed class RectOffset
    {
        public int left;
        public int right;
        public int top;
        public int bottom;

        public RectOffset()
        {
        }

        public RectOffset(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }

    public struct LayerMask
    {
        public int value;
    }

    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct Vector3Int
    {
        public int x;
        public int y;
        public int z;

        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3Int operator *(Vector3Int a, int d)
        {
            return new Vector3Int(a.x * d, a.y * d, a.z * d);
        }
    }

    public struct RangeInt
    {
        public int start;
        public int length;

        public RangeInt(int start, int length)
        {
            this.start = start;
            this.length = length;
        }
    }

    public struct RectInt
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public RectInt(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public RectInt(Vector2Int position, Vector2Int size)
        {
            this.x = position.x;
            this.y = position.y;
            this.width = size.x;
            this.height = size.y;
        }

        public RectInt(RectInt source)
        {
            this.x = source.x;
            this.y = source.y;
            this.width = source.width;
            this.height = source.height;
        }
    }

    public struct BoundsInt
    {
        public Vector3Int position;
        public Vector3Int size;

        public BoundsInt(Vector3Int position, Vector3Int size)
        {
            this.position = position;
            this.size = size;
        }
    }
}

#endif