using UnityEditor;
using UnityEngine;
using System.IO;

namespace RacingGameBot.Editors {
    [CustomEditor(typeof(Terrains.TerrainLoader))]
    public class TerrainLoaderEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            Terrains.TerrainLoader terrainLoader = (Terrains.TerrainLoader)target;

            if (GUILayout.Button("Load Terrain")) {
                string filename = Application.dataPath + "/Resources/SavedTerrains/" + terrainLoader.filename + ".data";
                if (File.Exists(filename)) {
                    terrainLoader.LoadTerrain(terrainLoader.filename);
                } else {
                    Debug.LogWarning("File doesn't exist: " + filename);
                }
            }
        }
    }
}
