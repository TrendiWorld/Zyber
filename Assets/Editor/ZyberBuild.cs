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
using System.Collections.Generic;
using System.IO;

public static class ZyberBuild
{
    const string ScenePath = "Assets/Main.unity";

    // glTFast loads materials at runtime via Shader.Find — those shaders must be
    // force-included or they get stripped from the build (black/magenta models).
    static readonly string[] GltfShaders = {
        "glTF/PbrMetallicRoughness", "glTF/PbrSpecularGlossiness", "glTF/Unlit",
        "Shader Graphs/glTF-pbrMetallicRoughness", "Shader Graphs/glTF-unlit"
    };

    static void EnsureGltfShadersIncluded()
    {
        var go = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        var gs = go.Length > 0 ? new SerializedObject(go[0]) : null;
        if (gs == null) return;
        var prop = gs.FindProperty("m_AlwaysIncludedShaders");
        var have = new HashSet<Object>();
        for (int i = 0; i < prop.arraySize; i++)
            have.Add(prop.GetArrayElementAtIndex(i).objectReferenceValue);

        foreach (var name in GltfShaders)
        {
            var sh = Shader.Find(name);
            if (sh != null && !have.Contains(sh))
            {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = sh;
                have.Add(sh);
                Debug.Log($"[Zyber] always-include shader: {name}");
            }
        }
        gs.ApplyModifiedProperties();
    }

    [MenuItem("Zyber/Build macOS Player")]
    public static void BuildMac()
    {
        EnsureGltfShadersIncluded();

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
