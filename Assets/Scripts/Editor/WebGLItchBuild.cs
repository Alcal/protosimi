#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ManosLimpias.Editor
{
    /// <summary>
    /// Batch WebGL player used by <c>scripts/deploy-itch-webgl.sh</c>.
    /// Honors <c>-buildOutput</c> from <c>unity build --output-path</c>.
    /// </summary>
    public static class WebGLItchBuild
    {
        public static void PerformBuild()
        {
            var output = OutputPath();
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes in Editor Build Settings.");

            Debug.Log($"[WebGLItchBuild] Building {scenes.Length} scene(s) -> {output}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"WebGL build {report.summary.result}: {report.summary.totalErrors} error(s).");
        }

        static string OutputPath()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildOutput")
                    return args[i + 1];
            }

            return "build/WebGL";
        }
    }
}
#endif
