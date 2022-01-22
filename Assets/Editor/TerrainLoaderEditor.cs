using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(TerrainLoader))]
public class TerrainLoaderEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        TerrainLoader terrainLoader = (TerrainLoader)target;

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
