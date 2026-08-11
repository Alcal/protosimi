using UnityEngine;

namespace ManosLimpias.Analytics
{
    public static class AnalyticsStub
    {
        public const string DesignVersion = "v1";

        public static void SessionStart() =>
            Log("session_start", $"design_version={DesignVersion}");

        public static void PlayPressed() =>
            Log("play_pressed", "");

        public static void StageStart(int stageIndex) =>
            Log("stage_start", $"stageIndex={stageIndex}");

        public static void StageComplete(int stageIndex, float durationSeconds) =>
            Log("stage_complete", $"stageIndex={stageIndex},duration_s={durationSeconds:F2}");

        public static void WafTriggered(int stageIndex, int level) =>
            Log("waf_triggered", $"stageIndex={stageIndex},level={level}");

        public static void AssistHijack(int stageIndex) =>
            Log("assist_hijack", $"stageIndex={stageIndex}");

        public static void SessionComplete(float totalSeconds) =>
            Log("session_complete", $"total_s={totalSeconds:F2}");

        static void Log(string eventName, string payload)
        {
            Debug.Log($"[Analytics] {eventName} {(string.IsNullOrEmpty(payload) ? "" : "| " + payload)}");
        }
    }
}
