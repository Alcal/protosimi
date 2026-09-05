using ManosLimpias.UI.Rive;
using NUnit.Framework;
using UnityEngine;

namespace ManosLimpias.Tests
{
    public class RiveGlowPlayModeTests
    {
        [Test]
        public void RiveGlow_SetOn_DoesNotThrow()
        {
            var go = new GameObject("RiveGlow PlayMode");
            try
            {
                var glow = go.AddComponent<RiveGlow>();
                Assert.DoesNotThrow(() => glow.SetOn(true));
                Assert.That(glow.IsOn, Is.True);
                glow.SetOn(false);
                Assert.That(glow.IsOn, Is.False);
            }
            finally
            {
                Object.Destroy(go);
            }
        }
    }
}
