using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// One drip ParticleSystem, typically parented behind a <c>hitbox_#</c>.
    /// Particle look is authored on the system; <see cref="Wetness"/> scales
    /// emission rate, start size, and start alpha from that baked full-wet state.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    [DefaultExecutionOrder(92)]
    public sealed class RiveDripEmitter : MonoBehaviour
    {
        public const string PrefabAssetPath = "Assets/Prefabs/RiveDripEmitter.prefab";
        public const string CircleTexturePath = "Assets/Art/Rive/drip-circle.png";

        const float SizeAtDry = 0.55f;

        [SerializeField] ParticleSystem particles;
        [SerializeField, Range(0f, 1f)] float wetness;

        [SerializeField] float fullRate;
        [SerializeField] float fullSize;
        [SerializeField] float fullGravity;
        [SerializeField] Color fullColor = Color.white;
        [SerializeField] bool baked;

        public ParticleSystem Particles => particles;
        public float Wetness => wetness;

        public void SetWetness(float progress01)
        {
            wetness = Mathf.Clamp01(progress01);
            EnsureParticles();
            EnsureBake();
            ApplyWetness();
        }

        [ContextMenu("Bake Full Wetness From Particle System")]
        public void BakeFromParticleSystem()
        {
            EnsureParticles();
            if (particles == null)
                return;

            var emission = particles.emission;
            fullRate = Mathf.Max(0f, CurveMax(emission.rateOverTime));
            var main = particles.main;
            fullSize = Mathf.Max(0.01f, CurveMax(main.startSize));
            fullGravity = CurveMax(main.gravityModifier);
            fullColor = main.startColor.color;
            baked = true;
        }

        public void FitBehindHitbox()
        {
            var parent = transform.parent as RectTransform;
            var rt = transform as RectTransform;
            if (parent == null || rt == null)
                return;

            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0.18f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.SetAsFirstSibling();

            EnsureParticles();
            if (particles == null)
                return;
        }

        void Awake()
        {
            EnsureParticles();
            EnsureBake();
            ApplyWetness();
            SyncSorting();
        }

        void LateUpdate()
        {
            FitBehindHitbox();
        }

        void OnValidate()
        {
            wetness = Mathf.Clamp01(wetness);
            if (!Application.isPlaying || !isActiveAndEnabled || particles == null || !baked)
                return;
            ApplyWetness();
        }

        void EnsureParticles()
        {
            if (particles == null)
                particles = GetComponent<ParticleSystem>();
        }

        void EnsureBake()
        {
            if (!baked)
                BakeFromParticleSystem();
        }

        void ApplyWetness()
        {
            if (particles == null)
                return;

            bool on = wetness > 0.001f;
            var emission = particles.emission;
            emission.enabled = on;
            emission.rateOverTime = fullRate * wetness;

            var main = particles.main;
            main.startSize = fullSize * Mathf.Lerp(SizeAtDry, 1f, wetness);
            var color = fullColor;
            color.a = fullColor.a * wetness;
            main.startColor = color;
            main.gravityModifier = fullGravity;

            if (!Application.isPlaying)
                return;
            if (on && !particles.isPlaying)
                particles.Play(true);
            else if (!on && particles.isPlaying)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        void SyncSorting()
        {
            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
                return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            renderer.sortingLayerID = canvas.sortingLayerID;
            renderer.sortingOrder = canvas.sortingOrder;
        }

        static float CurveMax(ParticleSystem.MinMaxCurve curve)
        {
            return curve.mode == ParticleSystemCurveMode.TwoConstants
                ? curve.constantMax
                : curve.constant;
        }
    }
}
