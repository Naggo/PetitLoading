using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;


namespace PetitLoading.Editor {
    public class PetitLoadingWindow : EditorWindow {
        static readonly Vector2 WinSize = new Vector2(250, 250);
        static readonly string[] Anchors = new string[] {"NE", "NW", "SE", "SW"};

        [MenuItem("Tools/Petit Loading")]
        static void Init() {
            EditorWindow window = GetWindow<PetitLoadingWindow>(true, "Petit Loading", true);
            window.minSize = WinSize;
            window.maxSize = WinSize;
            window.ShowUtility();
        }


        bool showImagesFolder = false;
        bool showPosition = false;
        Vector2 scrollPos;

        void OnGUI() {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            DrawToggle();
            DrawImagesFolder();
            DrawPosition();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tests", EditorStyles.boldLabel);
            DrawTests();

            EditorGUILayout.EndScrollView();
        }

        void DrawToggle() {
            PetitLoadingSettings settings = PetitLoadingSettings.instance;

            using (var check = new EditorGUI.ChangeCheckScope()) {
                settings.enabled = EditorGUILayout.Toggle("Enabled", settings.enabled);

                if (check.changed) {
                    settings.Save();
                    PetitLoadingCore.Setup();
                }
            }
        }

        void DrawImagesFolder() {
            PetitLoadingSettings settings = PetitLoadingSettings.instance;

            showImagesFolder = EditorGUILayout.Foldout(
                showImagesFolder, "Images Folder", true);

            if (showImagesFolder) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    settings.imagesPath = EditorGUILayout.TextField(settings.imagesPath);

                    if (check.changed) {
                        settings.Save();
                    }
                }

                if (GUILayout.Button("Browse")) {
                    string path;
                    try {
                        path = EditorUtility.OpenFolderPanel(
                            "Images Path",
                            Path.GetDirectoryName(settings.imagesPath),
                            Path.GetFileName(settings.imagesPath)
                        );
                    } catch (ArgumentException) {
                        path = EditorUtility.OpenFolderPanel(
                            "Images Path",
                            "",
                            ""
                        );
                    }

                    if (!string.IsNullOrEmpty(path)) {
                        settings.imagesPath = path;
                        settings.Save();
                        GUI.FocusControl(null);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        void DrawPosition() {
            PetitLoadingSettings settings = PetitLoadingSettings.instance;

            showPosition = EditorGUILayout.Foldout(
                showPosition, "Position", true);

            if (showPosition) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    DrawVector2IntField("Offset", ref settings.x, ref settings.y);
                    DrawVector2IntField("Size", ref settings.width, ref settings.height);
                    DrawStringPopup("Anchor", ref settings.anchor, Anchors);

                    if (check.changed) {
                        settings.Save();
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        void DrawTests() {
            using (var h = new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Start")) {
                    PetitLoadingCore.StartAnimation();
                }
                if (GUILayout.Button("Stop")) {
                    PetitLoadingCore.StopAnimation();
                }
            }
            if (GUILayout.Button("Test")) {
                Task.Run(async () => {
                    var p = PetitLoadingCore.StartAnimation();
                    if (p is null) return;
                    await Task.Delay(1000);
                    PetitLoadingCore.StopAnimation();
                    p.WaitForExit(500);
                    var output = await p.StandardOutput.ReadToEndAsync();
                    var err = await p.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(output)) {
                        Debug.Log(output);
                    }
                    if (!string.IsNullOrEmpty(err)) {
                        Debug.LogWarning(err);
                    }
                });
            }
        }

        void DrawVector2IntField(string label, ref int x, ref int y) {
            Vector2Int v = new Vector2Int(x, y);
            v = EditorGUILayout.Vector2IntField(label, v);
            x = v.x;
            y = v.y;
        }

        void DrawStringPopup(string label, ref string current, string[] options) {
            int index = Array.IndexOf(options, current);
            index = EditorGUILayout.Popup(label, index, options);
            if (0 <= index && index < options.Length) {
                current = options[index];
            }
        }
    }
}
