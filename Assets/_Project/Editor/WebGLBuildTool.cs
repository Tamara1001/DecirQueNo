using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DecirQueNo.EditorTools
{
    // One-click WebGL build for itch.io. See BUILD.md.
    // Menu: DecirQueNo/Build/WebGL for itch.io (zip)
    // CLI:  Unity -batchmode -quit -projectPath . -executeMethod DecirQueNo.EditorTools.WebGLBuildTool.BuildCli
    public static class WebGLBuildTool
    {
        private const string OutputFolder = "Builds/WebGL";
        private const string ZipPath = "Builds/DecirQueNo-WebGL.zip";

        private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        [MenuItem("DecirQueNo/Build/WebGL for itch.io (zip)")]
        public static void BuildAndZipMenu()
        {
            string error = BuildAndZip();
            if (error != null)
            {
                EditorUtility.DisplayDialog("WebGL build failed", error, "OK");
                return;
            }

            EditorUtility.RevealInFinder(Path.Combine(ProjectRoot, ZipPath));
        }

        [MenuItem("DecirQueNo/Build/Zip last WebGL build")]
        public static void ZipMenu()
        {
            Debug.Log("[Build] " + Zip());
        }

        // Entry point for batch mode. Exits with 0 on success and 1 on failure.
        public static void BuildCli()
        {
            string error = BuildAndZip();
            if (error != null)
            {
                Debug.LogError("[Build] " + error);
                EditorApplication.Exit(1);
                return;
            }

            EditorApplication.Exit(0);
        }

        // Returns null on success, or an error message.
        public static string BuildAndZip()
        {
            ApplySettings();

            string output = Path.Combine(ProjectRoot, OutputFolder);
            if (Directory.Exists(output))
            {
                Directory.Delete(output, true);
            }

            var options = new BuildPlayerOptions
            {
                scenes = EnabledScenes(),
                locationPathName = output,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            if (options.scenes.Length == 0)
            {
                return "No enabled scenes in Build Settings.";
            }

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                return $"Build {report.summary.result}: {report.summary.totalErrors} error(s).";
            }

            Debug.Log($"[Build] WebGL built in {report.summary.totalTime.TotalSeconds:0}s. {Zip()}");
            return null;
        }

        // WebGL settings that matter for itch.io. Safe to call repeatedly.
        public static void ApplySettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
        }

        // Zips Builds/WebGL with index.html at the root of the zip, as itch.io expects.
        public static string Zip()
        {
            string source = Path.Combine(ProjectRoot, OutputFolder);
            string zip = Path.Combine(ProjectRoot, ZipPath);

            if (!File.Exists(Path.Combine(source, "index.html")))
            {
                throw new InvalidOperationException($"No WebGL build found in {source}");
            }

            if (File.Exists(zip))
            {
                File.Delete(zip);
            }

            using (var stream = new FileStream(zip, FileMode.Create))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                {
                    // Always forward slashes, otherwise itch.io can show broken paths.
                    string entryName = Path.GetRelativePath(source, file).Replace('\\', '/');
                    archive.CreateEntryFromFile(file, entryName, System.IO.Compression.CompressionLevel.Optimal);
                }
            }

            double mb = new FileInfo(zip).Length / (1024.0 * 1024.0);
            return $"Zip ready: {zip} ({mb:0.0} MB)";
        }

        private static string[] EnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }
    }
}
