using System.Collections.Generic;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Down-loaded wobbling wet outline on an isolated Rive panel, plus one
    /// drip emitter prefab instance behind each <c>hitbox_#</c>.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(91)]
    public sealed class RiveDrip : MonoBehaviour
    {
        public const string ShaderName = "ManosLimpias/UI/RiveAlphaDrip";
        public const string TemplateAssetPath = "Assets/Materials/RiveAlphaDrip.mat";
        public const string ParticleTemplateAssetPath = "Assets/Materials/RiveDripParticle.mat";
        public const string EmitterPrefabPath = RiveDripEmitter.PrefabAssetPath;

        public static readonly int DripColorId = Shader.PropertyToID("_DripColor");
        public static readonly int RimWidthId = Shader.PropertyToID("_RimWidth");
        public static readonly int DripWidthId = Shader.PropertyToID("_DripWidth");
        public static readonly int DripLoadId = Shader.PropertyToID("_DripLoad");
        public static readonly int DripIntensityId = Shader.PropertyToID("_DripIntensity");
        public static readonly int DripThresholdId = Shader.PropertyToID("_DripThreshold");
        public static readonly int WobbleAmpId = Shader.PropertyToID("_WobbleAmp");
        public static readonly int WobbleFreqId = Shader.PropertyToID("_WobbleFreq");
        public static readonly int WobbleSpeedId = Shader.PropertyToID("_WobbleSpeed");
        public static readonly int GravityId = Shader.PropertyToID("_Gravity");

        public RiveWidget widget;
        public Material template;
        public RiveDripEmitter emitterPrefab;
        public bool followWaterContact;
        public Color color = new(0.55f, 0.85f, 1f, 0.85f);
        public float rimWidth = 3f;
        public float dripWidth = 24f;
        public float dripLoad = 4f;
        public float intensity = 1f;
        public float threshold = 0.05f;
        public float wobbleAmp = 0.35f;
        public float wobbleFreq = 6f;
        public float wobbleSpeed = 2.5f;
        public Vector2 gravity = new(0f, -1f);

        readonly List<RiveDripEmitter> _emitters = new();
        readonly List<RiveDripEmitter> _spawned = new();

        Material _instance;
        bool _isOn;
        bool _followLatched;
        float _wetness;

        public bool IsOn => _isOn;
        public float Wetness => _wetness;
        public Material RuntimeMaterial => _instance;
        public IReadOnlyList<RiveDripEmitter> Emitters => _emitters;

        public static RiveDrip ForWidget(RiveWidget widget)
        {
            return widget != null ? widget.GetComponentInParent<RiveDrip>(true) : null;
        }

        public static void SetForWidget(RiveWidget widget, bool on)
        {
            var drip = ForWidget(widget);
            if (drip != null)
                drip.SetOn(on);
        }

        public void SetOn(bool on)
        {
            SetWetness(on ? 1f : 0f);
        }

        public void SetWetness(float progress01)
        {
            _wetness = Mathf.Clamp01(progress01);
            _isOn = _wetness > 0.001f;
            _followLatched = true;
            EnsureMaterial();
            EnsureEmitters();
            ApplyOrRestoreMaterial();
            PushProperties();
            ApplyEmitterWetness();
        }

        public void ResetWetness()
        {
            SetWetness(0f);
        }

        public bool EnsureIsolated()
        {
            if (widget == null)
                widget = GetComponentInChildren<RiveWidget>(true) ?? GetComponent<RiveWidget>();
            if (widget == null)
                return false;

            if (RiveGlow.IsIsolated(widget))
            {
                ApplyOrRestoreMaterial();
                return true;
            }

            var glow = GetComponent<RiveGlow>() ?? GetComponentInParent<RiveGlow>(true);
            if (glow == null)
                return false;

            if (glow.widget == null)
                glow.widget = widget;
            bool isolated = glow.EnsureIsolated();
            ApplyOrRestoreMaterial();
            return isolated;
        }

        void OnEnable()
        {
            if (widget == null)
                widget = GetComponentInChildren<RiveWidget>(true) ?? GetComponent<RiveWidget>();
            EnsureIsolated();
            EnsureEmitters();
            if (!followWaterContact)
                SetOn(_isOn);
            else
            {
                ApplyOrRestoreMaterial();
                ApplyEmitterWetness();
            }
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (_instance != null)
                    Destroy(_instance);
                for (int i = 0; i < _spawned.Count; i++)
                {
                    if (_spawned[i] != null)
                        Destroy(_spawned[i].gameObject);
                }
            }
            else
            {
                if (_instance != null)
                    DestroyImmediate(_instance);
                for (int i = 0; i < _spawned.Count; i++)
                {
                    if (_spawned[i] != null)
                        DestroyImmediate(_spawned[i].gameObject);
                }
            }

            _spawned.Clear();
            _emitters.Clear();
        }

        void Update()
        {
            if (followWaterContact)
                SyncFromWaterContact();
            if (_isOn)
                ApplyOrRestoreMaterial();
        }

        void SyncFromWaterContact()
        {
            var hands = FindHands();
            bool overlapping = hands != null && hands.IsOverlapping;
            if (!_followLatched || overlapping != _isOn)
                SetOn(overlapping);
        }

        Hands FindHands()
        {
            if (widget != null)
            {
                var onWidget = widget.GetComponent<Hands>();
                if (onWidget != null)
                    return onWidget;
            }

            return GetComponentInChildren<Hands>(true);
        }

        void ApplyOrRestoreMaterial()
        {
            var renderer = FindRenderer();
            if (renderer == null)
                return;

            if (_isOn)
            {
                EnsureMaterial();
                if (_instance != null)
                    renderer.CustomMaterial = _instance;
                return;
            }

            var glow = GetComponent<RiveGlow>() ?? GetComponentInParent<RiveGlow>(true);
            if (glow != null && glow.IsOn && glow.RuntimeMaterial != null)
                renderer.CustomMaterial = glow.RuntimeMaterial;
            else
                renderer.CustomMaterial = null;
        }

        RiveCanvasRenderer FindRenderer()
        {
            var panel = GetComponent<RivePanel>();
            if (panel == null && widget != null)
                panel = widget.GetComponentInParent<RivePanel>();
            return panel != null ? panel.GetComponent<RiveCanvasRenderer>() : GetComponent<RiveCanvasRenderer>();
        }

        void EnsureMaterial()
        {
            if (_instance != null)
                return;

            var source = template;
            if (source == null)
            {
                var shader = Shader.Find(ShaderName);
                if (shader == null)
                    return;
                _instance = new Material(shader);
            }
            else
            {
                _instance = new Material(source);
            }

            _instance.name = (widget != null ? widget.gameObject.name : name) + " Drip";
            _instance.hideFlags = HideFlags.HideAndDontSave;
        }

        void PushProperties()
        {
            if (_instance == null)
                EnsureMaterial();
            if (_instance == null)
                return;

            _instance.SetColor(DripColorId, color);
            _instance.SetFloat(RimWidthId, rimWidth);
            _instance.SetFloat(DripWidthId, dripWidth);
            _instance.SetFloat(DripLoadId, dripLoad);
            _instance.SetFloat(DripIntensityId, intensity * _wetness);
            _instance.SetFloat(DripThresholdId, threshold);
            _instance.SetFloat(WobbleAmpId, wobbleAmp);
            _instance.SetFloat(WobbleFreqId, wobbleFreq);
            _instance.SetFloat(WobbleSpeedId, wobbleSpeed);
            _instance.SetVector(GravityId, new Vector4(gravity.x, gravity.y, 0f, 0f));
        }

        void EnsureEmitters()
        {
            DestroyLegacyNamedParticles();
            _emitters.Clear();

            var hitboxes = FindHitboxes();
            for (int i = 0; i < hitboxes.Count; i++)
            {
                var hitbox = hitboxes[i];
                var existing = ExistingEmitter(hitbox);
                if (existing != null)
                {
                    _emitters.Add(existing);
                    existing.FitBehindHitbox();
                    continue;
                }

                if (emitterPrefab == null)
                    continue;

                var instance = Instantiate(emitterPrefab, hitbox, false);
                instance.name = emitterPrefab.name;
                instance.gameObject.SetActive(true);
                instance.FitBehindHitbox();
                _emitters.Add(instance);
                _spawned.Add(instance);
            }

            if (Application.isPlaying)
                EnsureDrawOrder();
            else
            {
                for (int i = 0; i < _emitters.Count; i++)
                {
                    if (_emitters[i] != null)
                        _emitters[i].SyncSorting();
                }
            }

            ApplyEmitterWetness();
        }

        public void EnsureDrawOrder()
        {
            var rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null)
                rootCanvas = rootCanvas.rootCanvas;

            var panel = widget != null
                ? widget.GetComponentInParent<RivePanel>()
                : GetComponentInParent<RivePanel>();
            var overlayParent = panel != null ? panel.transform.parent : transform.parent;
            int rootOrder = rootCanvas != null ? rootCanvas.sortingOrder : 0;

            if (overlayParent != null)
            {
                for (int i = 0; i < overlayParent.childCount; i++)
                {
                    var child = overlayParent.GetChild(i);
                    if (IsBackgroundLayer(child))
                        continue;
                    int order = rootOrder + RiveDripEmitter.OverlaySortingOffsetFor(child.name);
                    RiveDripEmitter.EnsureOverlayCanvas(child.gameObject, order);
                }
            }
            else if (panel != null)
            {
                int order = rootOrder + RiveDripEmitter.OverlaySortingOffsetFor(panel.name);
                RiveDripEmitter.EnsureOverlayCanvas(panel.gameObject, order);
            }

            for (int i = 0; i < _emitters.Count; i++)
            {
                if (_emitters[i] != null)
                    _emitters[i].SyncSorting();
            }
        }

        static bool IsBackgroundLayer(Transform layer)
        {
            if (layer == null)
                return false;
            if (layer.name == "Rive Panel")
                return true;
            for (int i = 0; i < layer.childCount; i++)
            {
                if (layer.GetChild(i).name == Background.WidgetName)
                    return true;
            }

            return false;
        }

        void ApplyEmitterWetness()
        {
            for (int i = 0; i < _emitters.Count; i++)
            {
                if (_emitters[i] != null)
                    _emitters[i].SetWetness(_wetness);
            }
        }

        static RiveDripEmitter ExistingEmitter(Transform hitbox)
        {
            if (hitbox == null)
                return null;
            for (int i = 0; i < hitbox.childCount; i++)
            {
                var emitter = hitbox.GetChild(i).GetComponent<RiveDripEmitter>();
                if (emitter != null)
                    return emitter;
            }

            return hitbox.GetComponent<RiveDripEmitter>();
        }

        List<RectTransform> FindHitboxes()
        {
            var list = new List<RectTransform>();
            var host = widget != null ? widget.transform : transform;
            CollectHitboxes(host, list);
            return list;
        }

        static void CollectHitboxes(Transform host, List<RectTransform> list)
        {
            if (host == null)
                return;

            for (int i = 0; i < host.childCount; i++)
            {
                var child = host.GetChild(i);
                if (child.GetComponent<RiveDripEmitter>() != null)
                    continue;
                if (IsHitboxName(child.name) && child is RectTransform rt)
                    list.Add(rt);
            }
        }

        static bool IsHitboxName(string name)
        {
            return !string.IsNullOrEmpty(name)
                && name.StartsWith("hitbox_")
                && name.Length > "hitbox_".Length;
        }

        void DestroyLegacyNamedParticles()
        {
            var host = widget != null ? widget.transform : transform;
            if (host == null)
                return;
            var legacy = host.Find("RiveDripParticles");
            if (legacy == null)
                return;
            if (Application.isPlaying)
                Destroy(legacy.gameObject);
            else
                DestroyImmediate(legacy.gameObject);
        }
    }
}
