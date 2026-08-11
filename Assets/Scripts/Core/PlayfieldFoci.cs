using UnityEngine;

namespace ManosLimpias.Core
{
    public class PlayfieldFoci : MonoBehaviour
    {
        public GameObject faucet;
        public GameObject hands;
        public GameObject soap;
        public GameObject towel;

        public GameObject FocusForStage(int stageIndex) => stageIndex switch
        {
            0 or 4 => faucet,
            1 or 3 => hands,
            2 => soap,
            5 => towel,
            _ => null
        };

        public void SetActiveStage(int stageIndex)
        {
            SetHighlight(faucet, stageIndex is 0 or 4);
            SetHighlight(hands, stageIndex is 1 or 3);
            SetHighlight(soap, stageIndex == 2);
            SetHighlight(towel, stageIndex == 5);
        }

        static void SetHighlight(GameObject go, bool active)
        {
            if (go == null) return;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                c.a = active ? 1f : 0.45f;
                sr.color = c;
            }

            foreach (var col in go.GetComponentsInChildren<Collider2D>())
                col.enabled = active;
        }
    }
}
