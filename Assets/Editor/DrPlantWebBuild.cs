using System;
using System.IO;
using UnityEditor.Build;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class DrPlantWebBuild
{
    private const string DefaultOutputPath = "Builds/WebGL";
    private const string OutputArgument = "-drplant-web-output";

    [MenuItem("Dr.Plant/Build/WebGL")]
    public static void BuildWebGL()
    {
        string projectRoot = Path.GetFullPath(
            Path.Combine(Application.dataPath, ".."));
        string outputPath = ResolveOutputPath(projectRoot);
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new BuildFailedException("No enabled scenes are configured.");

        Directory.CreateDirectory(outputPath);

        PlayerSettings.productName = "Dr.Plant";
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.dataCaching = true;
        AssetDatabase.SaveAssets();

        BuildReport report = BuildPipeline.BuildPlayer(
            new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException(
                $"WebGL build failed: {report.summary.result} "
                + $"({report.summary.totalErrors} errors).");
        }

        File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), string.Empty);

        Debug.Log(
            $"Dr.Plant WebGL build completed: {outputPath} "
            + $"({report.summary.totalSize} bytes)");
    }

    private static string ResolveOutputPath(string projectRoot)
    {
        string[] arguments = Environment.GetCommandLineArgs();

        for (int index = 0; index < arguments.Length - 1; index++)
        {
            if (!string.Equals(
                arguments[index],
                OutputArgument,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string configuredPath = arguments[index + 1];
            return Path.GetFullPath(
                Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.Combine(projectRoot, configuredPath));
        }

        return Path.GetFullPath(Path.Combine(projectRoot, DefaultOutputPath));
    }
}
