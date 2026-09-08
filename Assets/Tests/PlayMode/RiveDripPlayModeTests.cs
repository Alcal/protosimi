using ManosLimpias.UI.Rive;
using NUnit.Framework;
using UnityEngine;

namespace ManosLimpias.Tests
{
    public class RiveDripPlayModeTests
    {
        [Test]
        public void RiveDrip_SetOn_DoesNotThrow()
        {
            var go = new GameObject("RiveDrip PlayMode");
            try
            {
                var drip = go.AddComponent<RiveDrip>();
                drip.followWaterContact = false;
                Assert.DoesNotThrow(() => drip.SetOn(true));
                Assert.That(drip.IsOn, Is.True);
                drip.SetOn(false);
                Assert.That(drip.IsOn, Is.False);
            }
            finally
            {
                Object.Destroy(go);
            }
        }
    }
}
