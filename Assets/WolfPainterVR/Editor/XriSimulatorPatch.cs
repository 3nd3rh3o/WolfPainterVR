using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace WolfPainterVR.Editor
{
    // Keep the fix in Git as a small diff and reapply it when Unity recreates its local package cache.
    // Git must be available on PATH. No package source is added to the repository.
    [InitializeOnLoad]
    static class XriSimulatorPatch
    {
        const string PackageName = "com.unity.xr.interaction.toolkit";
        const string PackageVersion = "3.6.0";
        const string PatchPath = "Patches/XRI-3.6.0-simulator-offsets.patch";
        const string SourcePath = "Runtime/Inputs/Simulation/XRInteractionSimulator.cs";

        static bool s_Scheduled;

        static XriSimulatorPatch()
        {
            Schedule();
            Events.registeredPackages += _ => Schedule();
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    Schedule();
            };
        }

        static void Schedule()
        {
            if (s_Scheduled)
                return;

            s_Scheduled = true;
            EditorApplication.delayCall += ApplyScheduledPatch;
        }

        static void ApplyScheduledPatch()
        {
            s_Scheduled = false;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Schedule();
                return;
            }

            Apply(false);
        }

        [MenuItem("Tools/WolfPainterVR/Apply XRI Simulator Patch")]
        public static void ApplyNow()
        {
            Apply(true);
        }

        static void Apply(bool reportAlreadyApplied)
        {
            try
            {
                var package = PackageInfo.FindForAssetPath("Packages/" + PackageName);
                if (package == null)
                    return; // Package registration will schedule another attempt.

                if (package.version != PackageVersion)
                {
                    Debug.LogWarning($"[WolfPainterVR] The simulator patch targets XRI {PackageVersion}; installed version is {package.version}. Review the patch before updating its version guard.");
                    return;
                }

                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var packageRoot = Path.GetFullPath(package.resolvedPath);
                var cacheRoot = Path.GetFullPath(Path.Combine(projectRoot, "Library/PackageCache")) + Path.DirectorySeparatorChar;
                var comparison = Application.platform == RuntimePlatform.WindowsEditor
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;
                if (!packageRoot.StartsWith(cacheRoot, comparison))
                    throw new InvalidOperationException("XRI must be installed in this project's Library/PackageCache before its local copy can be patched.");

                if (!File.Exists(Path.Combine(projectRoot, PatchPath)))
                    throw new FileNotFoundException("The tracked XRI patch is missing.", PatchPath);

                var relativePackagePath = Path.GetRelativePath(projectRoot, packageRoot).Replace('\\', '/');
                if (RunGit(projectRoot, relativePackagePath, "--reverse --check").exitCode == 0)
                {
                    if (reportAlreadyApplied)
                        Debug.Log("[WolfPainterVR] The XRI simulator patch is already applied.");
                    return;
                }

                var check = RunGit(projectRoot, relativePackagePath, "--check");
                if (check.exitCode != 0)
                    throw new InvalidOperationException("The XRI source does not match the patch. No changes were made.\n" + check.output);

                var source = Path.Combine(packageRoot, SourcePath);
                var attributes = File.GetAttributes(source);
                try
                {
                    File.SetAttributes(source, attributes & ~FileAttributes.ReadOnly);
                    var result = RunGit(projectRoot, relativePackagePath, "");
                    if (result.exitCode != 0)
                        throw new InvalidOperationException(result.output);
                }
                finally
                {
                    File.SetAttributes(source, attributes);
                }

                Debug.Log($"[WolfPainterVR] Applied the XRI {PackageVersion} simulator patch to the local package cache.");
                AssetDatabase.Refresh();
            }
            catch (Exception exception)
            {
                Debug.LogError("[WolfPainterVR] Automatic XRI patch failed. Git must be available on PATH.\n" + exception.Message);
            }
        }

        static (int exitCode, string output) RunGit(string projectRoot, string relativePackagePath, string options)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"apply --ignore-whitespace --whitespace=nowarn {options} --directory=\"{relativePackagePath}\" \"{PatchPath}\"",
                    WorkingDirectory = projectRoot,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                process.Start();
                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(10000))
                {
                    process.Kill();
                    throw new TimeoutException("git apply did not finish within 10 seconds.");
                }

                return (process.ExitCode, (output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult()).Trim());
            }
        }
    }
}
