using System.Collections.Generic;
using UnityEngine;

/*
Create empty object and assignt it this script
Create terrain and assign it to this script
Select texture from dropdown menu
*/

namespace RacingGameBot.Tests {
    public enum MyTexture { Grass1, Grass2, Grass3, Road1, Road2, Road3, Road4, Rocks1, Rocks2, Rocks3, Rocks4, Sand1, Snow1, Snow2, Snow3, Water1 }

    public class TextureTests : MonoBehaviour {

        [SerializeField]
        public bool showAll;

        [SerializeField]
        public Terrain terrain;

        [SerializeField]
        public MyTexture texture;

        // Update is called once per frame
        void Start() {
            if (terrain != null) {
                terrain.transform.position = Vector3.zero;
                terrain.terrainData.size = new Vector3(128, 32, 128);
            }
        }

        /// <summary>
        /// Draw all avaliable textures on one terrain object.
        /// Used to select best combinations of textures for different levels.
        /// </summary>
        void OnValidate() {
            if (terrain != null) {
                // add layers
                List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
                void AddTerrainLayer(string textureName) {
                    TerrainLayer terrainLayer = new TerrainLayer();
                    terrainLayer.diffuseTexture = (Texture2D)Resources.Load("Textures/" + textureName + "/" + textureName + "_base_color");
                    terrainLayer.normalMapTexture = (Texture2D)Resources.Load("Textures/" + textureName + "/" + textureName + "_normal");
                    terrainLayer.metallic = 0f;
                    terrainLayer.smoothness = 0f;
                    terrainLayer.tileSize = Vector2.one * 64;
                    terrainLayer.tileOffset = Vector2.zero;
                    terrainLayers.Add(terrainLayer);
                }
                if (showAll) {
                    foreach (MyTexture tex in MyTexture.GetValues(typeof(MyTexture))) {
                        AddTerrainLayer(tex.ToString());
                    }
                } else {
                    AddTerrainLayer(texture.ToString());
                }
                terrain.terrainData.terrainLayers = terrainLayers.ToArray();
                // show layers
                int alphaMapWidth = terrain.terrainData.alphamapWidth;
                int alphaMapHeight = terrain.terrainData.alphamapHeight;
                int alphaMapLayers = terrain.terrainData.alphamapLayers;
                float[,,] splatmapData = new float[alphaMapWidth, alphaMapHeight, terrain.terrainData.alphamapLayers];
                if (showAll) {
                    for (int xi = 0; xi < alphaMapWidth; xi++) {
                        for (int zi = 0; zi < alphaMapHeight; zi++) {
                            splatmapData[xi, zi, 0] = 0f;
                        }
                    }
                    int scale = Mathf.CeilToInt(Mathf.Sqrt(MyTexture.GetValues(typeof(MyTexture)).Length));
                    int width = Mathf.FloorToInt((float)alphaMapWidth / scale);
                    int height = Mathf.FloorToInt((float)alphaMapHeight / scale);
                    for (int i = 0; i < scale; i++) {
                        for (int j = 0; j < scale; j++) {
                            // fill with texture
                            int textureIndex = i * scale + j;
                            if (textureIndex < alphaMapLayers) {
                                for (int k = 0; k < width; k++) {
                                    for (int l = 0; l < height; l++) {
                                        splatmapData[i * width + k, j * height + l, textureIndex] = 1f;
                                    }
                                }
                            }
                        }
                    }
                } else {
                    for (int xi = 0; xi < alphaMapWidth; xi++) {
                        for (int zi = 0; zi < alphaMapHeight; zi++) {
                            splatmapData[xi, zi, 0] = 1f;
                        }
                    }
                }
                terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
            }
        }
    }
}