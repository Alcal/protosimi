using System;
using System.Text;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Reads an in-band PNG out of a .riv by its embedded image name.
    /// </summary>
    public static class RiveEmbeddedPng
    {
        static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public static bool TryExtractNamedPng(byte[] riv, string name, out byte[] png)
        {
            png = null;
            if (riv == null || string.IsNullOrEmpty(name) || name.Length > 255)
                return false;

            var token = Encoding.UTF8.GetBytes(name);
            var prefixed = new byte[token.Length + 1];
            prefixed[0] = (byte)token.Length;
            Buffer.BlockCopy(token, 0, prefixed, 1, token.Length);

            int nameAt = IndexOf(riv, prefixed);
            if (nameAt < 0)
                return false;

            int pngAt = IndexOf(riv, PngSignature, nameAt);
            if (pngAt < 0 || pngAt - nameAt > 256)
                return false;

            int end = FindPngEnd(riv, pngAt);
            if (end <= pngAt)
                return false;

            png = new byte[end - pngAt];
            Buffer.BlockCopy(riv, pngAt, png, 0, png.Length);
            return true;
        }

        static int FindPngEnd(byte[] data, int pngStart)
        {
            int pos = pngStart + 8;
            while (pos + 8 <= data.Length)
            {
                int length = (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];
                if (length < 0 || pos + 12 + length > data.Length)
                    return -1;
                bool iend = data[pos + 4] == (byte)'I'
                    && data[pos + 5] == (byte)'E'
                    && data[pos + 6] == (byte)'N'
                    && data[pos + 7] == (byte)'D';
                pos += 12 + length;
                if (iend)
                    return pos;
            }

            return -1;
        }

        static int IndexOf(byte[] data, byte[] token, int start = 0)
        {
            int end = data.Length - token.Length;
            for (int i = Math.Max(0, start); i <= end; i++)
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
