using System;
using ManosLimpias.Core;
using Rive;
using Rive.Components;
using UnityEngine;
using Random = System.Random;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Soap-progress foam on the Hands hitboxes plus a ParticleSystem on SoapRive.
    /// Foam widgets stay parented to the hands after ApplySoap exits. Rinse lowers
    /// coverage so bubbles shrink in reverse wake order.
    /// </summary>
    [DefaultExecutionOrder(120)]
    public sealed class SoapBubbles : MonoBehaviour, ISoapFoamControl
    {
        public const string ParticlesName = "SoapScrubParticles";
        public const string SlotPrefix = "GameBubbles_";
        public const string OverlayName = "SoapParticleOverlay";

        public Hands hands;
        public Soap soap;
        public Sprite bubbleSprite;
        public Material particleMaterial;
        public ParticleSystem scrubParticles;
        public int bubblesPerHitbox = 12;
        public float hitboxFit = 0.4f;
        public float growDuration = 0.5f;
        public float emitPerSecond = 8f;
        public float particleLifetime = 1.2f;
        public float particleRiseMin = 80f;
        public float particleRiseMax = 140f;
        public float particleDrift = 20f;
        public float particleSize = 56f;

        public float Coverage { get; private set; }
        public bool IsScrubbing { get; private set; }

        readonly Random _rng = new();
        FoamSlot[] _slots = Array.Empty<FoamSlot>();
        RiveWidget _handsWidget;
        bool _poolReady;

        public void Bind(Hands handsTarget, Soap soapTarget)
        {
            hands = handsTarget;
            soap = soapTarget;
            EnsureParticles();
            if (!Application.isPlaying)
                return;
            SubscribeHands();
            EnsurePool();
            ApplyScrubbing();
        }

        public void SetCoverage(float progress01)
        {
            Coverage = Mathf.Clamp01(progress01);
            WakeForCoverage();
            SleepForCoverage();
        }

        public void SetScrubbing(bool scrubbing)
        {
            IsScrubbing = scrubbing;
            ApplyScrubbing();
        }

        public void ResetFoam()
        {
            Coverage = 0f;
            IsScrubbing = false;
            ReshuffleSlots();
            ApplyScrubbing();
            if (scrubParticles != null)
            {
                scrubParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        void OnEnable()
        {
            SubscribeHands();
        }

        void OnDisable()
        {
            UnsubscribeHands();
        }

        void Start()
        {
            if (hands == null)
                hands = GetComponent<Hands>();
            SubscribeHands();
            EnsurePool();
            EnsureParticles();
            ApplyScrubbing();
        }

        void Update()
        {
            if (!_poolReady)
                EnsurePool();
            TickScale(Time.deltaTime);
        }

        void SubscribeHands()
        {
            var widget = hands != null ? hands.widget : null;
            if (widget == _handsWidget)
                return;

            UnsubscribeHands();
            _handsWidget = widget;
            if (_handsWidget != null)
                _handsWidget.OnWidgetStatusChanged += OnHandsStatusChanged;
        }

        void UnsubscribeHands()
        {
            if (_handsWidget != null)
                _handsWidget.OnWidgetStatusChanged -= OnHandsStatusChanged;
            _handsWidget = null;
        }

        void OnHandsStatusChanged()
        {
            EnsurePool();
        }

        void EnsurePool()
        {
            if (hands == null)
                return;
            if (hands.hitbox1 == null || hands.hitbox2 == null)
                hands.Bind(hands.widget, hands.faucetWidget);
            if (hands.hitbox1 == null || hands.hitbox2 == null)
                return;

            int wanted = Mathf.Max(0, bubblesPerHitbox) * 2;
            if (_poolReady && _slots.Length == wanted && !AnySlotLoadError())
                return;

            ClearPool();
            _slots = new FoamSlot[wanted];
            int i = 0;
            AddSlots(hands.hitbox1, "1", ref i);
            AddSlots(hands.hitbox2, "2", ref i);
            _poolReady = _slots.Length > 0;
            ReshuffleSlots();
            WakeForCoverage();
            SleepForCoverage();
        }

        void AddSlots(RiveNodeHitbox hitbox, string label, ref int index)
        {
            if (hitbox == null)
                return;
            var file = SourceFile();
            var asset = SourceAsset();
            ResolveFoam(file, out var artboard, out var sm, out var artSize);
            for (int n = 0; n < bubblesPerHitbox; n++)
            {
                var go = new GameObject($"{SlotPrefix}{label}_{n}", typeof(RectTransform));
                go.transform.SetParent(hitbox.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.zero;
                rt.localRotation = Quaternion.identity;

                var widget = go.AddComponent<RiveWidget>();
                widget.HitTestBehavior = HitTestBehavior.None;
                widget.Fit = Fit.Contain;
                LoadArtboard(widget, asset, file, artboard, sm);

                _slots[index++] = new FoamSlot
                {
                    transform = rt,
                    widget = widget,
                    hitbox = hitbox,
                    artSize = artSize
                };
            }
        }

        void ClearPool()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot?.transform == null)
                    continue;
                if (Application.isPlaying)
                    Destroy(slot.transform.gameObject);
                else
                    DestroyImmediate(slot.transform.gameObject);
            }

            _slots = Array.Empty<FoamSlot>();
            _poolReady = false;
        }

        void ReshuffleSlots()
        {
            var thresholds = SoapFoamMath.CreateThresholds(_slots.Length, _rng);
            for (int i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot == null)
                    continue;
                slot.threshold = thresholds[i];
                slot.speed = SoapFoamMath.RandomSpeed(_rng);
                slot.woken = false;
                slot.shrinking = false;
                slot.growElapsed = 0f;
                slot.shrinkElapsed = 0f;
                slot.placed = false;
                if (slot.widget != null)
                    slot.widget.Speed = slot.speed;
                if (slot.transform != null)
                    slot.transform.localScale = Vector3.zero;
            }
        }

        void WakeForCoverage()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.woken)
                    continue;
                if (!SoapFoamMath.ShouldWake(slot.threshold, Coverage))
                    continue;
                slot.woken = true;
                slot.shrinking = false;
                slot.shrinkElapsed = 0f;
                slot.growElapsed = 0f;
                PlaceOnce(slot);
            }
        }

        void SleepForCoverage()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot == null || !slot.woken || slot.shrinking)
                    continue;
                if (SoapFoamMath.ShouldWake(slot.threshold, Coverage))
                    continue;
                slot.shrinking = true;
                float current = SoapFoamMath.GrowScale(slot.growElapsed, growDuration);
                slot.shrinkElapsed = SoapFoamMath.ShrinkElapsedFromScale(current, growDuration);
            }
        }

        void PlaceOnce(FoamSlot slot)
        {
            if (slot == null || slot.placed || slot.transform == null || slot.hitbox == null)
                return;

            var parent = slot.hitbox.RectTransform;
            if (parent == null)
                return;

            var parentSize = parent.rect.size;
            var art = slot.artSize.x > 0f && slot.artSize.y > 0f
                ? slot.artSize
                : SimiPrototypeArtboards.GameBubblesSize;
            float fit = Mathf.Max(0.05f, hitboxFit);
            float scale = 1f;
            if (art.x > 0f && art.y > 0f && parentSize.x > 0f && parentSize.y > 0f)
                scale = Mathf.Min(parentSize.x / art.x, parentSize.y / art.y) * fit;

            var size = art * scale;
            slot.transform.sizeDelta = size;

            float halfW = Mathf.Max(0f, (parent.rect.width - size.x) * 0.5f);
            float halfH = Mathf.Max(0f, (parent.rect.height - size.y) * 0.5f);
            slot.transform.anchoredPosition = new Vector2(
                NextRange(-halfW, halfW),
                NextRange(-halfH, halfH));
            slot.placed = true;
        }

        void TickScale(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;
            for (int i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.transform == null)
                    continue;

                if (slot.shrinking)
                {
                    slot.shrinkElapsed += deltaTime;
                    float s = SoapFoamMath.ShrinkScale(slot.shrinkElapsed, growDuration);
                    slot.transform.localScale = new Vector3(s, s, 1f);
                    if (s <= 0f)
                    {
                        slot.shrinking = false;
                        slot.woken = false;
                        slot.growElapsed = 0f;
                        slot.shrinkElapsed = 0f;
                    }

                    continue;
                }

                if (!slot.woken)
                    continue;
                slot.growElapsed += deltaTime;
                float g = SoapFoamMath.GrowScale(slot.growElapsed, growDuration);
                slot.transform.localScale = new Vector3(g, g, 1f);
            }
        }

        void EnsureParticles()
        {
            var host = ParticleHost();
            if (host == null)
                return;

            if (scrubParticles == null)
            {
                var existing = host.Find(ParticlesName);
                if (existing != null)
                    scrubParticles = existing.GetComponent<ParticleSystem>();
            }

            if (scrubParticles == null)
            {
                var go = new GameObject(ParticlesName, typeof(RectTransform), typeof(ParticleSystem));
                go.transform.SetParent(host, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                rt.localScale = Vector3.one;
                scrubParticles = go.GetComponent<ParticleSystem>();
            }
            else if (scrubParticles.transform.parent != host)
            {
                scrubParticles.transform.SetParent(host, false);
            }

            ConfigureParticles(scrubParticles);
        }

        Transform ParticleHost()
        {
            if (soap == null)
                return null;
            if (soap.widget != null)
                return soap.widget.transform;
            return soap.transform;
        }

        void ConfigureParticles(ParticleSystem ps)
        {
            if (ps == null)
                return;

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.startLifetime = Mathf.Max(0.05f, particleLifetime);
            main.startSpeed = 0f;
            main.startSize = Mathf.Max(1f, particleSize);
            main.startColor = UnityEngine.Color.white;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 64;
            main.gravityModifier = -0.1f;

            var emission = ps.emission;
            emission.rateOverTime = Mathf.Max(0f, emitPerSecond);
            emission.enabled = IsScrubbing;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 24f;
            shape.radiusThickness = 1f;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-particleDrift, particleDrift);
            vel.y = new ParticleSystem.MinMaxCurve(particleRiseMin, particleRiseMax);
            vel.z = 0f;

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new UnityEngine.Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(UnityEngine.Color.white, 0f),
                    new GradientColorKey(UnityEngine.Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            if (particleMaterial != null)
                renderer.sharedMaterial = particleMaterial;
            else if (bubbleSprite != null && bubbleSprite.texture != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    var mat = new Material(shader) { mainTexture = bubbleSprite.texture };
                    mat.hideFlags = HideFlags.HideAndDontSave;
                    renderer.material = mat;
                }
            }

            var canvas = ps.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                renderer.sortingLayerID = canvas.sortingLayerID;
                renderer.sortingOrder = canvas.sortingOrder + 1;
            }
        }

        void ApplyScrubbing()
        {
            if (scrubParticles == null)
                return;

            var emission = scrubParticles.emission;
            emission.enabled = IsScrubbing;
            emission.rateOverTime = Mathf.Max(0f, emitPerSecond);
            if (IsScrubbing && !scrubParticles.isPlaying)
                scrubParticles.Play(true);
            else if (!IsScrubbing && scrubParticles.isPlaying)
                scrubParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        File SourceFile()
        {
            return hands != null && hands.widget != null ? hands.widget.File : null;
        }

        Asset SourceAsset()
        {
            return hands != null && hands.widget != null ? hands.widget.Asset : null;
        }

        float NextRange(float min, float max)
        {
            if (max <= min)
                return min;
            return min + (float)_rng.NextDouble() * (max - min);
        }

        bool AnySlotLoadError()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var widget = _slots[i]?.widget;
                if (widget != null && widget.Status == WidgetStatus.Error)
                    return true;
            }

            return false;
        }

        static void ResolveFoam(File file, out string artboard, out string stateMachine, out Vector2 size)
        {
            bool hasSm = ArtboardHasStateMachine(file, GameBubbles.Artboard);
            artboard = SoapFoamMath.FoamArtboard(hasSm);
            size = artboard == GameBubble.Artboard
                ? SimiPrototypeArtboards.GameBubbleSize
                : SimiPrototypeArtboards.GameBubblesSize;
            string preferred = artboard == GameBubble.Artboard
                ? GameBubble.StateMachine
                : GameBubbles.StateMachine;
            stateMachine = ResolveStateMachine(file, artboard, preferred);
        }

        static bool ArtboardHasStateMachine(File file, string artboardName)
        {
            if (file == null || string.IsNullOrEmpty(artboardName))
                return false;

            for (uint i = 0; i < file.ArtboardCount; i++)
            {
                if (file.ArtboardName(i) != artboardName)
                    continue;
                var artboard = file.Artboard(i);
                return artboard != null && artboard.StateMachineCount > 0;
            }

            return false;
        }

        static string ResolveStateMachine(File file, string artboardName, string preferred)
        {
            if (file == null || string.IsNullOrEmpty(artboardName))
                return preferred ?? string.Empty;

            for (uint i = 0; i < file.ArtboardCount; i++)
            {
                if (file.ArtboardName(i) != artboardName)
                    continue;
                var artboard = file.Artboard(i);
                if (artboard == null || artboard.StateMachineCount == 0)
                    return string.Empty;
                if (!string.IsNullOrEmpty(preferred))
                {
                    for (uint j = 0; j < artboard.StateMachineCount; j++)
                    {
                        if (artboard.StateMachineName(j) == preferred)
                            return preferred;
                    }
                }

                return artboard.StateMachineName(0);
            }

            return preferred ?? string.Empty;
        }

        static void LoadArtboard(RiveWidget widget, Asset asset, File file, string artboard, string stateMachine)
        {
            if (widget == null)
                return;
            if (file != null)
                widget.Load(file, artboard, stateMachine ?? string.Empty);
            else if (asset != null)
                widget.Load(asset, artboard, stateMachine ?? string.Empty);
        }

        sealed class FoamSlot
        {
            public RectTransform transform;
            public RiveWidget widget;
            public RiveNodeHitbox hitbox;
            public float threshold = 1f;
            public float speed = 1f;
            public float growElapsed;
            public float shrinkElapsed;
            public bool woken;
            public bool shrinking;
            public bool placed;
            public Vector2 artSize;
        }
    }
}
