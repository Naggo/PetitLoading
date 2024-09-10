using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PetitLoading.Editor {
    [FilePath("PetitLoadingSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class PetitLoadingSettings : ScriptableSingleton<PetitLoadingSettings> {
        public bool enabled;
        public string imagesPath;

        public int x = 32;
        public int y = 128;
        public int width = 128;
        public int height = 128;
        public string anchor = "SE";

        public void Save() {
            Save(true);
        }
    }
}
