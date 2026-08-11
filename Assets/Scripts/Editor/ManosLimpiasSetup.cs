#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ManosLimpias.Editor
{
    /// <summary>
    /// Lightweight helpers only. Scene scaffolding is done via Unity MCP (RunCommand), not runtime bootstrap.
    /// </summary>
    public static class ManosLimpiasSetup
    {
        [MenuItem("ManosLimpias/Select FineTuning Asset")]
        public static void SelectTuning()
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>("Assets/Data/FineTuningVariables.asset");
            if (asset == null)
            {
                Debug.LogWarning("FineTuningVariables.asset missing at Assets/Data/");
                return;
            }
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
#endif
