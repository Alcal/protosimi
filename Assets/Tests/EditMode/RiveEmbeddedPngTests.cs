using System.IO;
using ManosLimpias.UI.Rive;
using NUnit.Framework;
using UnityEngine;

namespace ManosLimpias.Tests
{
    public class RiveEmbeddedPngTests
    {
        [Test]
        public void ExtractsNamedBubblePngFromPrototypeRiv()
        {
            var path = Path.Combine(Application.dataPath, "Art/Rive/simi_prototype.riv");
            Assert.That(File.Exists(path), Is.True, path);

            var png = Extract("bubble");
            Assert.That(png[0], Is.EqualTo(0x89));
            Assert.That(png[1], Is.EqualTo((byte)'P'));
            Assert.That(png[2], Is.EqualTo((byte)'N'));
            Assert.That(png[3], Is.EqualTo((byte)'G'));
            Assert.That(ReadPngSize(png), Is.EqualTo((67, 65)));
        }

        [Test]
        public void MissingName_ReturnsFalse()
        {
            var riv = File.ReadAllBytes(Path.Combine(Application.dataPath, "Art/Rive/simi_prototype.riv"));
            Assert.That(RiveEmbeddedPng.TryExtractNamedPng(riv, "no-such-image", out var png), Is.False);
            Assert.That(png, Is.Null);
        }

        static byte[] Extract(string name)
        {
            var riv = File.ReadAllBytes(Path.Combine(Application.dataPath, "Art/Rive/simi_prototype.riv"));
            Assert.That(RiveEmbeddedPng.TryExtractNamedPng(riv, name, out var png), Is.True);
            Assert.That(png, Is.Not.Null);
            Assert.That(png.Length, Is.GreaterThan(24));
            return png;
        }

        static (int width, int height) ReadPngSize(byte[] png)
        {
            int width = (png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19];
            int height = (png[20] << 24) | (png[21] << 16) | (png[22] << 8) | png[23];
            return (width, height);
        }
    }
}
