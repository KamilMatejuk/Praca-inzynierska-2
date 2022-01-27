using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {

    public override void OnInspectorGUI() {
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        if (DrawDefaultInspector()) {
            terrainGenerator.GenerateTerrain();
        }

        if (GUILayout.Button("Generate Terrain")) {
            terrainGenerator.GenerateTerrain();
        }

		if (GUILayout.Button("Generate Textures")) {
            terrainGenerator.GenerateTerrainTextures();
        }

        if (GUILayout.Button("Save")) {
            terrainGenerator.Save();
        }
    }
}
