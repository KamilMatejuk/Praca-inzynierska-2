using UnityEditor;
using UnityEngine;

namespace RacingGameBot.Editors {
    [CustomEditor(typeof(Terrains.TerrainGenerator))]
    public class TerrainGeneratorEditor : Editor {

        public override void OnInspectorGUI() {
            Terrains.TerrainGenerator terrainGenerator = (Terrains.TerrainGenerator)target;

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
}
