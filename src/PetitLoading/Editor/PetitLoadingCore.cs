using System.Collections;
using System.Collections.Generic;
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
    public class PetitLoadingCore {
        const string TempFileName = "PetitLoadingFlag_Deletable.tmp";


        public class BuildHandler : IPreprocessBuildWithReport {
            public int callbackOrder => 1;

            public void OnPreprocessBuild(BuildReport report) {
                EditorApplication.delayCall += StopAnimation;
            }
        }


        static int updateCount;

        static PetitLoadingCore() {
            updateCount = 0;
            EditorApplication.update += FirstSetup;
        }


        static void FirstSetup() {
            updateCount++;
            if (updateCount < 2) return;

            EditorApplication.update -= FirstSetup;
            StopAnimation();
            Setup();
        }

        public static void Setup() {
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


        public static void StartAnimation() {
            string tempFilePath = Path.GetFullPath(
                Path.Combine(Application.temporaryCachePath, TempFileName)
            );

            if (File.Exists(tempFilePath)) return;

            string sourceFilePath = GetSourceFilePath();
            string pythonFilePath = Path.GetFullPath(
                Path.Combine(sourceFilePath, "..", "..", "PyScripts~", "invoker.py")
            );
            PetitLoadingSettings settings = PetitLoadingSettings.instance;
            string imagesFolderPath = settings.imagesPath;
            string rectString = $"{settings.x},{settings.y},{settings.width},{settings.height},{settings.anchor}";

            File.Create(tempFilePath).Close();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "pythonw";
            startInfo.Arguments = $"\"{pythonFilePath}\" \"{tempFilePath}\" \"{imagesFolderPath}\" \"{rectString}\"";

            Process.Start(startInfo);
        }

        public static void StopAnimation() {
            string tempFilePath = Path.GetFullPath(
                Path.Combine(Application.temporaryCachePath, TempFileName)
            );

            File.Delete(tempFilePath);
        }

        static string GetSourceFilePath([CallerFilePath]string sourceFilePath = "") {
            return sourceFilePath;
        }
    }
}
