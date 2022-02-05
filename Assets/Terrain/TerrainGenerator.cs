using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RacingGameBot.Terrains {
    public class TerrainGenerator : MonoBehaviour {

        [SerializeField, HideInInspector] Loop controlPoints;
        [SerializeField, HideInInspector] GameObject parent;
        [SerializeField, HideInInspector] GameObject startFinish;
        [SerializeField, HideInInspector] List<GameObject> checkpoints;
        [SerializeField, HideInInspector] List<GameObject> cars;
        [SerializeField] public bool generateOnChanges = false;
        [SerializeField] public int seed = 0;
        [SerializeField] public Data.TerrainGenData terrainGenData;
        [SerializeField] public bool showCheckpoints = false;
        [SerializeField] public bool showBorders = false;
        [SerializeField] public bool showRoadBezier = false;
        [SerializeField] public string filename;

        void OnValidate() {
            if (terrainGenData != null && generateOnChanges) {
                Data.TerrainGenData.OnValuesUpdated -= GenerateTerrain;
                Data.TerrainGenData.OnValuesUpdated += GenerateTerrain;
            }
        }

        /// <summary>
        /// Draw road in editor
        /// </summary>
        public void ShowRoadBezier() {
            if (showRoadBezier) {
                LineRenderer lr = GetComponent<LineRenderer>();
                if (controlPoints != null && controlPoints.terrainGenData != null) {
                    int vertexCount = 0;
                    for (int i = 0; i < controlPoints.NumberOfSegments; i++) {
                        List<OrientedPoint> bezierPoints = controlPoints.GetSegmentBezierPoints(i);
                        for (float t = 0; t < 1; t += 0.01f) {
                            OrientedPoint op = Utils.Bezier.GetBezierOrientedPoint(bezierPoints[0].position,
                                                                             bezierPoints[1].position,
                                                                             bezierPoints[2].position,
                                                                             bezierPoints[3].position, t);
                            lr.positionCount = vertexCount + 1;
                            op.position.y = controlPoints.GetHeight(op.position.x, op.position.z) * 128;
                            op.position += new Vector3(Utils.Variables.TERRAIN_SIZE / -2, 0, Utils.Variables.TERRAIN_SIZE / -2);
                            lr.SetPosition(vertexCount, op.position);
                            vertexCount++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate terrain
        /// </summary>
        public void GenerateTerrain() {
            UnityEngine.Random.InitState(seed);
            GenerateLoop();
            GenerateTerrainShape();
            ShowRoadBezier();
        }

        /// <summary>
        /// Generate road loop
        /// </summary>
        public void GenerateLoop() {
            UnityEngine.Random.InitState(seed);
            controlPoints = new Loop(gameObject.transform.position, terrainGenData);
        }

        /// <summary>
        /// Save current terrain
        /// </summary>
        public void Save() {
            UnityEngine.Random.InitState(seed);
            PutStartFinish();
            PutCheckpoints();
            PutCars(4);
            PutBorders();
            TerrainLoader.SaveTerrain(GetComponent<Terrain>().terrainData,
                                      terrainGenData,
                                      controlPoints.points.ToArray(),
                                      startFinish.transform.localScale,
                                      cars.Select(c => new OrientedPoint(c.transform.position, c.transform.rotation)).ToArray(),
                                      cars.Count > 0 ? cars[0].transform.localScale : Vector3.zero,
                                      checkpoints.Select(c => new OrientedPoint(c.transform.position, c.transform.rotation)).ToArray(),
                                      checkpoints.Count > 0 ? checkpoints[0].transform.localScale : Vector3.zero,
                                      filename);
        }

        /// <summary>
        /// Create terrain shape (heightmap) based on perlin noise
        /// </summary>
        public void GenerateTerrainShape() {
            UnityEngine.Random.InitState(seed);
            GetComponent<Terrain>().terrainData.heightmapResolution = Utils.Variables.TERRAIN_SIZE + 1;
            GetComponent<Terrain>().terrainData.size = new Vector3(Utils.Variables.TERRAIN_SIZE, Utils.Variables.TERRAIN_HEIGHT, Utils.Variables.TERRAIN_SIZE);
            float[,] heights = new float[Utils.Variables.TERRAIN_SIZE, Utils.Variables.TERRAIN_SIZE];
            for (int xi = 0; xi < Utils.Variables.TERRAIN_SIZE; xi++) {
                for (int zi = 0; zi < Utils.Variables.TERRAIN_SIZE; zi++) {
                    heights[xi, zi] = controlPoints.GetHeight(zi, xi);
                }
            }
            GetComponent<Terrain>().terrainData.SetHeights(0, 0, heights);
        }

        /// <summary>
        /// Clear terrain textures
        /// </summary>
        public void RemoveTerrainTextures() {
            GetComponent<Terrain>().terrainData.terrainLayers = new TerrainLayer[0];
        }

        /// <summary>
        /// Add textures and Utils.Objects to terrain based on terrain type
        /// </summary>
        public IEnumerator GenerateTerrainTextures(System.Action<int> callbackUpdateTime = null, System.Action callbackEnd = null) {
            UnityEngine.Random.InitState(seed);
            GetComponent<Terrain>().terrainData.terrainLayers = LoadTerrainLayers(terrainGenData.terrainType);

            GetComponent<Terrain>().terrainData.alphamapResolution = Utils.Variables.TERRAIN_SIZE * 4;
            int alphaMapWidth = GetComponent<Terrain>().terrainData.alphamapWidth;
            int alphaMapHeight = GetComponent<Terrain>().terrainData.alphamapHeight;
            int alphaMapLayers = GetComponent<Terrain>().terrainData.alphamapLayers;
            float[,,] splatmapData = new float[alphaMapWidth, alphaMapHeight, GetComponent<Terrain>().terrainData.alphamapLayers];

            void SubdividedData(int startX, int startZ, int size) {
                float x = (float)Utils.Variables.TERRAIN_SIZE * (startX + 0.5f * size) / alphaMapWidth;
                float z = (float)Utils.Variables.TERRAIN_SIZE * (startZ + 0.5f * size) / alphaMapHeight;
                Vector3 p = new Vector3(x, 0, z);
                float distance = controlPoints.GetNearestBezierPoint(p).other;
                if (size == 1 || distance > 1.5f * terrainGenData.roadWidth * (float)Utils.Variables.TERRAIN_SIZE * size / alphaMapWidth) {
                    float normalizedX = (float)x / (float)Utils.Variables.TERRAIN_SIZE;
                    float normalizedZ = (float)z / (float)Utils.Variables.TERRAIN_SIZE;
                    float height = controlPoints.GetHeight(x, z);
                    Vector3 normal = GetComponent<Terrain>().terrainData.GetInterpolatedNormal(normalizedZ, normalizedX);
                    float steepness = GetComponent<Terrain>().terrainData.GetSteepness(normalizedZ, normalizedX);
                    float[] weights = SelectLayersAlpha(terrainGenData.terrainType, height, normal, steepness, distance);
                    for (int i = 0; i < Mathf.Min(alphaMapLayers, weights.Length); i++) {
                        for (int xi = 0; xi < size; xi++) {
                            for (int zi = 0; zi < size; zi++) {
                                splatmapData[startZ + zi, startX + xi, i] = weights[i];
                            }
                        }
                    }
                } else {
                    for (int xi = 0; xi < 2; xi++) {
                        for (int zi = 0; zi < 2; zi++) {
                            SubdividedData(startX + (size * xi / 2), startZ + (size * zi / 2), size / 2);
                        }
                    }
                }
            }

            int scalar = 64;
            int index = 0;
            int maxIndex = (alphaMapWidth / scalar) * (alphaMapHeight / scalar);
            for (int xi = 0; xi < alphaMapWidth; xi += scalar) {
                for (int zi = 0; zi < alphaMapHeight; zi += scalar) {
                    SubdividedData(xi, zi, scalar);
                    int percentage = 100 * ++index / maxIndex;
                    callbackUpdateTime?.Invoke(percentage);
                    yield return null;
                }
            }

            GetComponent<Terrain>().terrainData.SetAlphamaps(0, 0, splatmapData);
            callbackEnd?.Invoke();
            yield return null;
        }

        /// <summary>
        /// Load texture based on name
        /// </summary>
        /// <param name="textureName">Name of texture file</param>
        /// <param name="size">Texture tiling size</param>
        /// <returns>TerrainLayer object with loaded texture</returns>
        public static TerrainLayer GetTerrainLayerTexture(string textureName, float size) {
            TerrainLayer terrainLayer = new TerrainLayer();
            terrainLayer.diffuseTexture = (Texture2D)Resources.Load("Textures/" + textureName + "/" + textureName + "_base_color");
            terrainLayer.normalMapTexture = (Texture2D)Resources.Load("Textures/" + textureName + "/" + textureName + "_normal");
            terrainLayer.metallic = 0f;
            terrainLayer.smoothness = 0f;
            terrainLayer.tileSize = Vector2.one * size;
            terrainLayer.tileOffset = Vector2.zero;
            terrainLayer.name = "texture_" + textureName;
            return terrainLayer;
        }

        /// <summary>
        /// Load single-colored texture
        /// </summary>
        /// <param name="color">Texture color</param>
        /// <returns>TerrainLayer object with loaded color</returns>
        public static TerrainLayer GetTerrainLayerColor(Color color) {
            TerrainLayer terrainLayer = new TerrainLayer();
            Texture2D tempTexture = new Texture2D(1024, 1024);
            for (int i = 0; i < 1024; i++) {
                for (int j = 0; j < 1024; j++) {
                    tempTexture.SetPixel(i, j, color);
                }
            }
            tempTexture.Apply();
            terrainLayer.diffuseTexture = tempTexture;
            tempTexture = new Texture2D(1024, 1024);
            for (int i = 0; i < 1024; i++) {
                for (int j = 0; j < 1024; j++) {
                    tempTexture.SetPixel(i, j, Color.black);
                }
            }
            tempTexture.Apply();
            terrainLayer.normalMapTexture = tempTexture;
            terrainLayer.metallic = 0f;
            terrainLayer.smoothness = 0f;
            terrainLayer.name = "color_" + Utils.Parser.ColorToString(color);
            return terrainLayer;
        }

        /// <summary>
        /// Load layers based on terrain type
        /// </summary>
        /// <param name="terrainType">Type of terrain</param>
        /// <returns>List of TerrainLayers</returns>
        TerrainLayer[] LoadTerrainLayers(Data.TerrainType terrainType) {
            // https://3dtextures.me/
            List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

            // first layer is road
            switch (terrainType) {
                // case Data.TerrainType.Basic:
                //     terrainLayers.Add(GetTerrainLayerColor(new Color(0f, 0f, 0f)));
                //     terrainLayers.Add(GetTerrainLayerColor(new Color(1f, 1f, 1f)));
                //     break;

                case Data.TerrainType.Forest:
                    terrainLayers.Add(GetTerrainLayerTexture("Road4", 2f));
                    terrainLayers.Add(GetTerrainLayerTexture("Grass3", 30f));
                    terrainLayers.Add(GetTerrainLayerTexture("Grass1", 10f));
                    break;

                case Data.TerrainType.Desert:
                    terrainLayers.Add(GetTerrainLayerTexture("Rocks3", 1f));
                    terrainLayers.Add(GetTerrainLayerColor(new Color(0.7f, 0.4f, 0f)));
                    terrainLayers.Add(GetTerrainLayerTexture("Sand1", 1f));
                    break;

                case Data.TerrainType.Mountains:
                    terrainLayers.Add(GetTerrainLayerTexture("Road2", 5f));
                    terrainLayers.Add(GetTerrainLayerTexture("Snow1", 5f));
                    terrainLayers.Add(GetTerrainLayerTexture("Snow3", 5f));
                    break;

                // all textures
                default:
                    terrainLayers.Add(GetTerrainLayerColor(new Color(0f, 0f, 0f)));
                    terrainLayers.Add(GetTerrainLayerColor(new Color(1f, 1f, 1f)));
                    // terrainLayers.Add(GetTerrainLayerTexture("Grass1", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Grass2", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Grass3", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Road1", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Road2", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Road3", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Road4", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Rocks1", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Rocks2", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Rocks3", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Rocks4", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Sand1", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Snow1", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Snow2", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Snow3", 1f));
                    // terrainLayers.Add(GetTerrainLayerTexture("Water1", 1f));
                    break;
            }
            return terrainLayers.ToArray();
        }

        /// <summary>
        /// Select how leyers should blend in this specific point
        /// </summary>
        /// <param name="terrainType">Type of terrain</param>
        /// <param name="height">Height</param>
        /// <param name="normal">Normal</param>
        /// <param name="steepness">Steepness</param>
        /// <param name="distanceToRoad">Distance to road</param>
        /// <returns>List of splatmap data</returns>
        float[] SelectLayersAlpha(Data.TerrainType terrainType, float height, Vector3 normal, float steepness, float distanceToRoad) {
            List<float> splatmapWeights = new List<float>();
            switch (terrainType) {
                /* ********************************************** Basic ********************************************** */
                // case Data.TerrainType.Basic:
                //     if (distanceToRoad < terrainGenData.roadWidth) {
                //         splatmapWeights.Add(1f);
                //         splatmapWeights.Add(0f);
                //     } else {
                //         splatmapWeights.Add(0f);
                //         splatmapWeights.Add(1f);
                //     }
                //     break;

                /* ********************************************** mountains ********************************************** */
                case Data.TerrainType.Mountains:
                    if (distanceToRoad < terrainGenData.roadWidth) {
                        splatmapWeights.Add(1f);
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(0f);
                    } else if (distanceToRoad < 2 * terrainGenData.roadWidth) {
                        splatmapWeights.Add(1f - (distanceToRoad - terrainGenData.roadWidth) / (1 * terrainGenData.roadWidth));
                        splatmapWeights.Add((distanceToRoad - terrainGenData.roadWidth) / (1 * terrainGenData.roadWidth));
                        splatmapWeights.Add(0f);
                    } else if (steepness > 45f) {
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(0.5f);
                        splatmapWeights.Add(0.5f);
                    } else {
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(1f);
                        splatmapWeights.Add(0f);
                    }
                    break;

                /* ********************************************** desert ********************************************** */
                case Data.TerrainType.Desert:
                    if (distanceToRoad < terrainGenData.roadWidth) {
                        splatmapWeights.Add(0.7f);
                        splatmapWeights.Add(0.3f);
                        splatmapWeights.Add(0f);
                    } else if (distanceToRoad < 2 * terrainGenData.roadWidth) {
                        splatmapWeights.Add(1f - (distanceToRoad - terrainGenData.roadWidth) / (1 * terrainGenData.roadWidth));
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add((distanceToRoad - terrainGenData.roadWidth) / (1 * terrainGenData.roadWidth));
                    } else {
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(1f);
                    }
                    break;

                /* ********************************************** forest ********************************************** */
                case Data.TerrainType.Forest:
                    if (distanceToRoad < terrainGenData.roadWidth) {
                        splatmapWeights.Add(1f);
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(0f);
                    } else if (distanceToRoad < 2 * terrainGenData.roadWidth) {
                        splatmapWeights.Add(1f - (distanceToRoad - terrainGenData.roadWidth) / (1 * terrainGenData.roadWidth));
                        splatmapWeights.Add((distanceToRoad - terrainGenData.roadWidth) / (2 * terrainGenData.roadWidth));
                        splatmapWeights.Add((distanceToRoad - terrainGenData.roadWidth) / (2 * terrainGenData.roadWidth));
                    } else {
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(0.5f);
                        splatmapWeights.Add(0.5f);
                    }
                    break;

                /* ********************************************** default ********************************************** */
                default:
                    if (distanceToRoad < terrainGenData.roadWidth) {
                        splatmapWeights.Add(1f);
                        splatmapWeights.Add(0f);
                    } else if (distanceToRoad < 3 * terrainGenData.roadWidth) {
                        splatmapWeights.Add(1f - (distanceToRoad - terrainGenData.roadWidth) / (2 * terrainGenData.roadWidth));
                        splatmapWeights.Add((distanceToRoad - terrainGenData.roadWidth) / (2 * terrainGenData.roadWidth));
                    } else {
                        splatmapWeights.Add(0f);
                        splatmapWeights.Add(1f);
                    }
                    break;
            }
            // normalize
            float sum = 0f;
            for (int i = 0; i < splatmapWeights.Count; i++) {
                sum += splatmapWeights[i];
            }
            for (int i = 0; i < splatmapWeights.Count; i++) {
                splatmapWeights[i] /= sum;
            }
            return splatmapWeights.ToArray();
        }

        /// <summary>
        /// Add start/finish object to the map
        /// </summary>
        void PutStartFinish() {
            OrientedPoint op = controlPoints.GetCentralPoint(0);
            op.position += transform.position;
            Utils.Objects.RemoveObjectsByTagInParent("meta", gameObject);
            startFinish = Utils.Objects.PutObject("StartFinish", "meta", "Meta", op);
            startFinish.transform.parent = transform;
            Bounds b = startFinish.GetComponent<Renderer>().bounds;
            float width = Mathf.Sqrt(Mathf.Pow(b.extents.x, 2) + Mathf.Pow(b.extents.z, 2));
            startFinish.transform.localScale *= 2 * terrainGenData.roadWidth / width;
            Vector3 pos = op.position;
            pos.y += controlPoints.GetHeight(op.position.x, op.position.z) * 128;
            startFinish.transform.position = pos;
        }

        /// <summary>
        /// Add cars Utils.Objects to the map
        /// </summary>
        void PutCars(int n) {
            GameObject carPrefab = (GameObject)Resources.Load("Prefabs/SportCarAI");
            Utils.Objects.RemoveObjectsByTagInParent("car", gameObject);
            cars = new List<GameObject>();
            GameObject carsGroup = Utils.Objects.PutParentObject("car", "Cars");
            carsGroup.transform.parent = transform;
            List<OrientedPoint> equallySpaced = controlPoints.GetEquallySpacedPoints(60);
            for (int i = 0; i < n; i++) {
                OrientedPoint op = equallySpaced[equallySpaced.Count - i - 2];
                GameObject car = Utils.Objects.PutObject("SportCarAI", "car", "Car " + i, op);
                car.transform.parent = carsGroup.transform;
                cars.Add(car);
            }
        }

        /// <summary>
        /// Add checkpoints Utils.Objects to the map
        /// </summary>
        void PutCheckpoints() {
            GameObject checkpointPrefab = (GameObject)Resources.Load("Prefabs/StartFinish");
            Utils.Objects.RemoveObjectsByTagInParent("checkpoint", gameObject);
            checkpoints = new List<GameObject>();
            List<OrientedPoint> ops = controlPoints.GetEquallySpacedPoints(terrainGenData.numberOfCheckpoints);
            GameObject checkpointsGroup = Utils.Objects.PutParentObject("checkpoint", "Checkpoints");
            checkpointsGroup.transform.parent = transform;
            for (int i = 0; i < ops.Count; i++) {
                OrientedPoint op = ops[i];
                op.rotation *= Quaternion.Euler(0, 90, 0);
                GameObject chpt = Utils.Objects.PutObject("Checkpoint", "checkpoint", "Checkpoint " + i, op);
                Bounds b = chpt.GetComponent<Renderer>().bounds;
                float width = Mathf.Sqrt(Mathf.Pow(b.extents.x, 2) + Mathf.Pow(b.extents.z, 2));
                chpt.transform.localScale *= 2 * terrainGenData.roadWidth / width;
                Vector3 pos = op.position;
                pos.y += controlPoints.GetHeight(op.position.x, op.position.z);
                chpt.transform.position = transform.position + pos;
                chpt.transform.parent = checkpointsGroup.transform;
                chpt.GetComponent<MeshRenderer>().enabled = showCheckpoints;
                chpt.GetComponent<BoxCollider>().enabled = false;
                checkpoints.Add(chpt);
            }
        }

        /// <summary>
        /// Add borders to the map
        /// </summary>
        void PutBorders() {
            Utils.Objects.RemoveObjectsByTagInParent("border", gameObject);
            GameObject bordersGroup = Utils.Objects.PutParentObject("border", "Borders");
            bordersGroup.transform.parent = transform;
            Vector3 terrainSize = GetComponent<Terrain>().terrainData.size;
            Vector3 center = Vector3.zero;
            foreach (int xi in new int[] { -1, 0, 1 }) {
                foreach (int zi in new int[] { -1, 0, 1 }) {
                    if (xi != 0 || zi != 0) {
                        if (xi == 0 || zi == 0) {
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.name = "Border";
                            cube.tag = "border";
                            cube.transform.parent = bordersGroup.transform;
                            cube.transform.localScale = new Vector3(terrainSize.x, terrainSize.y, 2f);
                            cube.transform.position = center + new Vector3(xi, 0, zi) * terrainSize.x / 2;
                            cube.transform.rotation *= Quaternion.Euler(0, 90 * xi, 0);
                            cube.GetComponent<MeshRenderer>().enabled = showBorders;
                        }
                    }
                }
            }
        }
    }
}