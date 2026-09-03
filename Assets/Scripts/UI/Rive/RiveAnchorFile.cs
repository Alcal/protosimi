using System;
using System.Text;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Reads Node x/y for named background anchors out of a .riv byte blob.
    /// Used to keep <see cref="BackgroundAnchors"/> in sync with the exported file.
    /// </summary>
    public static class RiveAnchorFile
    {
        const byte ParentIdKey = 5;
        const byte XKey = 13;
        const byte YKey = 14;

        public static bool TryReadLocal(byte[] data, string name, out int parentId, out Vector2 local)
        {
            parentId = 0;
            local = Vector2.zero;
            if (data == null || string.IsNullOrEmpty(name))
                return false;

            var token = Encoding.UTF8.GetBytes(name);
            int i = IndexOf(data, token);
            if (i < 0)
                return false;

            int p = i + token.Length;
            float x = 0f;
            float y = 0f;
            while (p < data.Length)
            {
                byte key = data[p++];
                if (key == 0)
                    break;
                if (key == ParentIdKey)
                    parentId = data[p++];
                else if (key == XKey)
                    x = ReadF32(data, ref p);
                else if (key == YKey)
                    y = ReadF32(data, ref p);
                else
                    break;
            }

            local = new Vector2(x, y);
            return true;
        }

        public static bool TryReadStepGroupOrigin(byte[] data, out Vector2 origin)
        {
            origin = Vector2.zero;
            if (data == null)
                return false;

            var token = Encoding.UTF8.GetBytes(BackgroundAnchors.Step4);
            int i = IndexOf(data, token);
            if (i < 17)
                return false;

            if (data[i - 17] != 2 || data[i - 16] != ParentIdKey || data[i - 14] != XKey || data[i - 9] != YKey)
                return false;

            int xAt = i - 13;
            int yAt = i - 8;
            origin = new Vector2(
                BitConverter.ToSingle(data, xAt),
                BitConverter.ToSingle(data, yAt));
            return true;
        }

        const byte OriginXKey = 11;
        const byte OriginYKey = 12;

        /// <summary>
        /// Reads artboard originX/originY (0–1). Missing keys mean the Rive default of (0, 0).
        /// </summary>
        public static bool TryReadArtboardOrigin(byte[] data, string artboardName, out Vector2 origin)
        {
            origin = Vector2.zero;
            if (data == null || string.IsNullOrEmpty(artboardName))
                return false;

            var token = Encoding.UTF8.GetBytes(artboardName);
            int nameAt = IndexOfNamedProperty(data, token);
            if (nameAt < 0)
                return false;

            int keyAt = nameAt - 2;
            for (int p = keyAt - 10; p >= keyAt - 48 && p >= 0; p--)
            {
                if (data[p] != OriginXKey || p + 9 >= data.Length || data[p + 5] != OriginYKey)
                    continue;

                float ox = BitConverter.ToSingle(data, p + 1);
                float oy = BitConverter.ToSingle(data, p + 6);
                if (ox < 0f || ox > 1f || oy < 0f || oy > 1f)
                    continue;

                origin = new Vector2(ox, oy);
                return true;
            }

            return true;
        }

        static int IndexOfNamedProperty(byte[] data, byte[] token)
        {
            int i = 0;
            while (true)
            {
                i = IndexOf(data, token, i);
                if (i < 0)
                    return -1;
                if (i >= 2 && data[i - 2] == 4 && data[i - 1] == token.Length)
                    return i;
                i++;
            }
        }

        static float ReadF32(byte[] data, ref int p)
        {
            float value = BitConverter.ToSingle(data, p);
            p += 4;
            return value;
        }

        static int IndexOf(byte[] data, byte[] token)
        {
            return IndexOf(data, token, 0);
        }

        static int IndexOf(byte[] data, byte[] token, int start)
        {
            int end = data.Length - token.Length;
            for (int i = Mathf.Max(0, start); i <= end; i++)
            {
                int t = 0;
                while (t < token.Length && data[i + t] == token[t])
                    t++;
                if (t == token.Length)
                    return i;
            }

            return -1;
        }
    }
}
