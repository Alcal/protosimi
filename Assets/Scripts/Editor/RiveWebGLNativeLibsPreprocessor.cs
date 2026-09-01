#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ManosLimpias.Editor
{
    /// <summary>
    /// Rive ships WebGL <c>.a</c> libraries with Exclude WebGL=1. Their own
    /// <c>WebGLBuildPreprocessor</c> copies them into Assets/Plugins, but that
    /// class is wrapped in <c>#if UNITY_WEBGL</c> and is often missing in GameCI
    /// batchmode (Editor domain compiled before the WebGL target switch). This
    /// project-side copy always compiles in the Editor and links the libs.
    /// </summary>
    public class RiveWebGLNativeLibsPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        const string TempPluginsPath = "Assets/Plugins/WebGL/Rive";
        const string PackageName = "app.rive.rive-unity";

        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            var source = ResolveSourcePath();
            if (string.IsNullOrEmpty(source) || !Directory.Exists(source))
                throw new BuildFailedException($"Rive WebGL native libs not found. Looked for {source}");

            EnsureFolder("Assets/Plugins");
            EnsureFolder("Assets/Plugins/WebGL");
            EnsureFolder(TempPluginsPath);

            var files = Directory.GetFiles(source, "*.a");
            if (files.Length == 0)
                throw new BuildFailedException($"Rive WebGL native libs folder is empty: {source}");

            foreach (var file in files)
            {
                var dest = Path.Combine(TempPluginsPath, Path.GetFileName(file));
                File.Copy(file, dest, true);
                AssetDatabase.ImportAsset(dest);
                var importer = AssetImporter.GetAtPath(dest) as PluginImporter;
                if (importer == null)
                    throw new BuildFailedException($"Failed to import Rive WebGL plugin {dest}");

                importer.SetCompatibleWithAnyPlatform(false);
                importer.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh();
            Debug.Log($"[RiveWebGLNativeLibs] Copied {files.Length} libraries from {source} -> {TempPluginsPath}");
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            if (AssetDatabase.IsValidFolder(TempPluginsPath))
                AssetDatabase.DeleteAsset(TempPluginsPath);
        }

        static string ResolveSourcePath()
        {
            var emscripten = IsUnity6OrNewer(Application.unityVersion) ? "emscripten_3.1.38" : "emscripten_3.1.8";
            return Path.GetFullPath(Path.Combine("Packages", PackageName, "Runtime/Libraries/WebGL", emscripten));
        }

        static bool IsUnity6OrNewer(string unityVersion)
        {
            if (string.IsNullOrEmpty(unityVersion))
                return false;
            var dot = unityVersion.IndexOf('.');
            var majorToken = dot >= 0 ? unityVersion.Substring(0, dot) : unityVersion;
            return int.TryParse(majorToken, out var major) && (major >= 6000 || major == 2023);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
#endif
