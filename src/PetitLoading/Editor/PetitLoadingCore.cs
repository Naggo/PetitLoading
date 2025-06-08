using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;


namespace PetitLoading.Editor {
    [InitializeOnLoad]
    public static class PetitLoadingCore {
        const string TempFileName = "PetitLoadingFlag_Deletable.tmp";


        public class BuildHandler : IPreprocessBuildWithReport {
            public int callbackOrder => 1;

            public void OnPreprocessBuild(BuildReport report) {
                EditorApplication.delayCall += StopAnimation;
            }
        }


        static int updateCount;
        static string tempFilePath;

        static PetitLoadingCore() {
            updateCount = 0;
            EditorApplication.update += FirstSetup;
        }


        static void FirstSetup() {
            updateCount++;
            if (updateCount < 2) return;

            EditorApplication.update -= FirstSetup;
            Setup();
            StopAnimation();
        }

        public static void Setup() {
            tempFilePath = Path.GetFullPath(
                Path.Join(Application.temporaryCachePath, TempFileName)
            );

            CompilationPipeline.compilationStarted -= CompilationStarted;
            CompilationPipeline.assemblyCompilationFinished -= AssemblyCompilationFinished;
            if (PetitLoadingSettings.instance.enabled) {
                CompilationPipeline.compilationStarted += CompilationStarted;
                CompilationPipeline.assemblyCompilationFinished += AssemblyCompilationFinished;
            }
        }

        static void CompilationStarted(object obj) {
            StartAnimation();
        }

        static void AssemblyCompilationFinished(object obj, CompilerMessage[] messages) {
            foreach (CompilerMessage compilerMessage in messages) {
                if (compilerMessage.type == CompilerMessageType.Error) {
                    StopAnimation();
                    return;
                }
            }
        }


        public static Process StartAnimation() {
            if (File.Exists(tempFilePath)) return null;

            string sourceFilePath = GetSourceFilePath();
            string pythonFilePath = Path.GetFullPath(
                Path.Join(sourceFilePath, "../../PyScripts~/invoker.py")
            );
            PetitLoadingSettings settings = PetitLoadingSettings.instance;
            string imagesFolderPath = settings.imagesPath;
            string rectString = $"{settings.x},{settings.y},{settings.width},{settings.height},{settings.anchor}";

            File.Create(tempFilePath).Close();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            var platform = Environment.OSVersion.Platform;
            if (platform is PlatformID.Win32NT) {
                startInfo.FileName = "pythonw";
                startInfo.Arguments = $"\"{pythonFilePath}\" \"{tempFilePath}\" \"{imagesFolderPath}\" \"{rectString}\"";
            } else if (platform is PlatformID.Unix or PlatformID.MacOSX) {
                startInfo.FileName = "/bin/zsh";
                startInfo.Arguments = $"-l -c 'python3 \"{pythonFilePath}\" \"{tempFilePath}\" \"{imagesFolderPath}\" \"{rectString}\"'";
            } else {
                startInfo.FileName = "python3";
                startInfo.Arguments = $"\"{pythonFilePath}\" \"{tempFilePath}\" \"{imagesFolderPath}\" \"{rectString}\"";
            }

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            return Process.Start(startInfo);
        }

        public static void StopAnimation() {
            File.Delete(tempFilePath);
        }

        static string GetSourceFilePath([CallerFilePath]string sourceFilePath = "") {
            return sourceFilePath;
        }
    }
}
