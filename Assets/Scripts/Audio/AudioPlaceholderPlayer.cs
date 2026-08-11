using System.Collections.Generic;
using UnityEngine;

namespace ManosLimpias.Audio
{
    public class AudioPlaceholderPlayer : MonoBehaviour
    {
        AudioSource _source;
        readonly Dictionary<string, AudioClip> _clips = new();

        void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            EnsureSilentClips();
        }

        void EnsureSilentClips()
        {
            string[] keys =
            {
                "vo_welcome", "vo_stage_0", "vo_stage_1", "vo_stage_2", "vo_stage_3", "vo_stage_4", "vo_stage_5",
                "vo_caf_praise", "vo_waf_hint", "vo_waf_assist", "vo_complete",
                "sfx_caf_positive", "sfx_progress_tick", "sfx_water_loop", "sfx_soap_rub", "sfx_towel_rub", "sfx_germ_pop"
            };
            foreach (var key in keys)
            {
                var clip = AudioClip.Create(key, 4410, 1, 44100, false);
                _clips[key] = clip;
            }
        }

        public void Play(string key)
        {
            if (string.IsNullOrEmpty(key) || !_clips.TryGetValue(key, out var clip)) return;
            _source.PlayOneShot(clip);
            Debug.Log($"[AudioPlaceholder] play {key}");
        }

        public void StopAll()
        {
            if (_source != null) _source.Stop();
        }
    }
}
