using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Applies a transparency-aware outline glow to a Rive artboard widget by
    /// isolating it onto its own panel RenderTexture.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(90)]
    public sealed class RiveGlow : MonoBehaviour
    {
        public const string ShaderName = "ManosLimpias/UI/RiveAlphaGlow";
        public const string TemplateAssetPath = "Assets/Materials/RiveAlphaGlow.mat";
        public const string HudPanelName = "Rive Hud Panel";

        public static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
        public static readonly int GlowWidthId = Shader.PropertyToID("_GlowWidth");
        public static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");
        public static readonly int GlowThresholdId = Shader.PropertyToID("_GlowThreshold");

        public RiveWidget widget;
        public Material template;
        public Color color = new(1f, 0.85f, 0.2f, 1f);
        public float width = 8f;
        public float intensity = 1f;
        public float threshold = 0.05f;
        public float pulseHertz = 1f;

        Material _instance;
        bool _isOn;
        bool _oneShot;
        float _pulseElapsed;
        float _pulseDuration;
        float _displayIntensity;

        public bool IsOn => _isOn;
        public float DisplayIntensity => _displayIntensity;
        public Material RuntimeMaterial => _instance;

        public static string PanelNameFor(string widgetName) =>
            string.IsNullOrEmpty(widgetName) ? "RiveGlowPanel" : widgetName + "Panel";

        public static RiveGlow ForWidget(RiveWidget widget)
        {
            return widget != null ? widget.GetComponentInParent<RiveGlow>(true) : null;
        }

        public static void SetForWidget(RiveWidget widget, bool on)
        {
            var glow = ForWidget(widget);
            if (glow != null)
                glow.SetOn(on);
        }

        /// <summary>
        /// Rect that should be positioned (or dragged): the isolated panel when
        /// this widget is the only child of its RivePanel; otherwise the widget.
        /// </summary>
        public static RectTransform LayoutRect(RiveWidget widget)
        {
            if (widget == null)
                return null;

            var rt = widget.RectTransform;
            var panel = widget.GetComponentInParent<RivePanel>();
            if (panel == null || rt == null)
                return rt;

            var panelRt = panel.WidgetContainer;
            if (rt.parent != panelRt)
                return rt;

            return IsSoleWidget(panel, widget) ? panelRt : rt;
        }

        public static bool IsIsolated(RiveWidget widget)
        {
            if (widget == null)
                return false;

            var rt = widget.RectTransform;
            var panel = widget.GetComponentInParent<RivePanel>();
            if (panel == null || rt == null)
                return false;

            return rt.parent == panel.WidgetContainer && IsSoleWidget(panel, widget);
        }

        public void SetOn(bool on)
        {
            _isOn = on;
            _oneShot = false;
            _pulseElapsed = 0f;
            _displayIntensity = on ? intensity * LoopWave(0f) : 0f;
            EnsureMaterial();
            PushProperties();
        }

        public void Pulse(float duration)
        {
            _pulseDuration = Mathf.Max(0.01f, duration);
            _pulseElapsed = 0f;
            _oneShot = true;
            _isOn = true;
            _displayIntensity = 0f;
            EnsureMaterial();
            PushProperties();
        }

        public void Tick(float deltaTime)
        {
            if (!_isOn)
                return;

            if (_oneShot)
            {
                _pulseElapsed += deltaTime;
                float t = _pulseElapsed / _pulseDuration;
                if (t >= 1f)
                {
                    SetOn(false);
                    return;
                }

                float wave = t < 0.5f ? t * 2f : 2f * (1f - t);
                _displayIntensity = intensity * wave;
                PushProperties();
                return;
            }

            _pulseElapsed += deltaTime;
            _displayIntensity = intensity * LoopWave(_pulseElapsed);
            PushProperties();
        }

        /// <summary>
        /// If the widget still shares a panel with other widgets, wrap it in a
        /// dedicated RivePanel sibling. Returns true when the widget is isolated.
        /// </summary>
        public bool EnsureIsolated()
        {
            if (widget == null)
                widget = GetComponentInChildren<RiveWidget>(true) ?? GetComponent<RiveWidget>();
            if (widget == null)
                return false;

            if (IsIsolated(widget))
            {
                ApplyMaterial(widget.GetComponentInParent<RivePanel>());
                return true;
            }

            return Isolate();
        }

        void OnEnable()
        {
            if (widget == null)
                widget = GetComponentInChildren<RiveWidget>(true) ?? GetComponent<RiveWidget>();
            EnsureIsolated();
            SetOn(_isOn);
        }

        void OnDestroy()
        {
            if (_instance == null)
                return;
            if (Application.isPlaying)
                Destroy(_instance);
            else
                DestroyImmediate(_instance);
        }

        void Update()
        {
            if (_isOn)
                Tick(Time.unscaledDeltaTime);
        }

        float LoopWave(float elapsed)
        {
            float hz = Mathf.Max(0.01f, pulseHertz);
            return 0.5f * (1f + Mathf.Cos(elapsed * hz * Mathf.PI * 2f));
        }

        bool Isolate()
        {
            var widgetRt = widget.RectTransform;
            if (widgetRt == null)
                return false;

            var shared = widget.GetComponentInParent<RivePanel>();
            var oldParent = widgetRt.parent as RectTransform;
            Transform host = shared != null ? shared.transform.parent : oldParent;
            if (host == null)
                host = widgetRt.parent;
            if (host == null)
                return false;

            int insertIndex = shared != null
                ? shared.transform.GetSiblingIndex() + 1
                : widgetRt.GetSiblingIndex();

            var panelGo = new GameObject(PanelNameFor(widget.gameObject.name));
            var panelRt = panelGo.AddComponent<RectTransform>();
            panelGo.transform.SetParent(host, false);
            panelGo.transform.SetSiblingIndex(Mathf.Clamp(insertIndex, 0, host.childCount - 1));
            panelGo.AddComponent<RivePanel>();
            panelGo.AddComponent<RiveCanvasRenderer>();

            CopyWorldRect(widgetRt, panelRt, host as RectTransform);
            widgetRt.SetParent(panelRt, false);
            ArtboardSpace.StretchFill(widgetRt);

            ApplyMaterial(panelGo.GetComponent<RivePanel>());
            return IsIsolated(widget);
        }

        void ApplyMaterial(RivePanel panel)
        {
            if (panel == null && widget != null)
                panel = widget.GetComponentInParent<RivePanel>();
            var renderer = panel != null ? panel.GetComponent<RiveCanvasRenderer>() : null;
            if (renderer == null)
                return;

            EnsureMaterial();
            renderer.CustomMaterial = _instance;
            PushProperties();
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

            _instance.name = (widget != null ? widget.gameObject.name : name) + " Glow";
            _instance.hideFlags = HideFlags.HideAndDontSave;
        }

        void PushProperties()
        {
            if (_instance == null)
                EnsureMaterial();
            if (_instance == null)
                return;

            _instance.SetColor(GlowColorId, color);
            _instance.SetFloat(GlowWidthId, width);
            _instance.SetFloat(GlowIntensityId, _displayIntensity);
            _instance.SetFloat(GlowThresholdId, threshold);
        }

        static bool IsSoleWidget(RivePanel panel, RiveWidget widget)
        {
            if (panel == null || widget == null)
                return false;

            var children = panel.GetComponentsInChildren<RiveWidget>(true);
            return children.Length == 1 && children[0] == widget;
        }

        static void CopyWorldRect(RectTransform source, RectTransform dest, RectTransform host)
        {
            if (source == null || dest == null)
                return;

            if (host == null)
            {
                dest.anchorMin = source.anchorMin;
                dest.anchorMax = source.anchorMax;
                dest.pivot = source.pivot;
                dest.anchoredPosition = source.anchoredPosition;
                dest.sizeDelta = source.sizeDelta;
                dest.localRotation = source.localRotation;
                dest.localScale = source.localScale;
                return;
            }

            var corners = new Vector3[4];
            source.GetWorldCorners(corners);
            var min = (Vector2)host.InverseTransformPoint(corners[0]);
            var max = (Vector2)host.InverseTransformPoint(corners[2]);
            var mapped = Rect.MinMaxRect(
                Mathf.Min(min.x, max.x),
                Mathf.Min(min.y, max.y),
                Mathf.Max(min.x, max.x),
                Mathf.Max(min.y, max.y));
            ArtboardSpace.ApplyNormalizedAnchors(dest, host, mapped, source.pivot);
        }
    }
}
