using System.Collections.Generic;
using UnityEngine;

namespace ManosLimpias.Core
{
    public class GermPop : MonoBehaviour
    {
        public Transform germRoot;
        public int germCount = 8;
        readonly List<GameObject> _germs = new();

        public void ResetGerms()
        {
            foreach (var g in _germs)
            {
                if (g) Destroy(g);
            }
            _germs.Clear();
        }

        public void EnsureGerms()
        {
            if (_germs.Count > 0) return;
            if (germRoot == null) germRoot = transform;
            for (int i = 0; i < germCount; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                g.name = $"Germ_{i}";
                g.transform.SetParent(germRoot, false);
                g.transform.localScale = Vector3.one * 0.25f;
                float ang = (Mathf.PI * 2f * i) / germCount;
                g.transform.localPosition = new Vector3(Mathf.Cos(ang) * 0.9f, Mathf.Sin(ang) * 0.5f, 0f);
                var r = g.GetComponent<Renderer>();
                if (r != null)
                    r.material.color = new Color(0.45f, 0.85f, 0.35f, 0.9f);
                Object.Destroy(g.GetComponent<Collider>());
                _germs.Add(g);
            }
        }

        public void UpdateFromProgress(float progress)
        {
            if (_germs.Count == 0) return;
            int keep = Mathf.CeilToInt((1f - progress) * _germs.Count);
            for (int i = 0; i < _germs.Count; i++)
            {
                if (_germs[i] != null)
                    _germs[i].SetActive(i < keep);
            }
        }
    }
}
