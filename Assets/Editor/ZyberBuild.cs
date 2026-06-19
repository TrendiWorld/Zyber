// =============================================================
// ZyberBuild.cs — batchmode build entry point. Creates a minimal boot
// scene (GameBootstrap auto-runs on load) and builds a macOS standalone
// player so the game can be launched without the editor GUI.
// Invoke: Unity -batchmode -quit -executeMethod ZyberBuild.BuildMac
// =============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public static class ZyberBuild
{
    const string ScenePath = "Assets/Main.unity";

    [MenuItem("Zyber/Build macOS Player")]
    public static void BuildMac()
    {
        // ensure a boot scene exists (camera + light; GameBootstrap does the rest)
        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        string root = Path.GetDirectoryName(Application.dataPath);   // project root
        string outDir = Path.Combine(root, "Build");
        Directory.CreateDirectory(outDir);
        string outPath = Path.Combine(outDir, "Zyber.app");

        var opts = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        Debug.Log($"[Zyber] build result: {report.summary.result}, output: {outPath}");
        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
