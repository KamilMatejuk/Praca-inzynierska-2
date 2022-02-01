using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;

namespace RacingGameBot.Terrains {
    public class TerrainLoader : MonoBehaviour {

        [SerializeField, HideInInspector] public string basePath;
        [SerializeField, HideInInspector] public Data.TerrainGenData terrainGenData;
        [SerializeField, HideInInspector] public Loop controlPoints;
        [SerializeField, HideInInspector] public GameObject startFinish;
        [SerializeField, HideInInspector] public List<GameObject> checkpoints;
        [SerializeField, HideInInspector] public List<GameObject> cars;
        [SerializeField, HideInInspector] public GameObject terrainGO;
        [SerializeField, HideInInspector] public Vector3 terrainDataSize;
        [SerializeField] public bool showCheckpoints = false;
        [SerializeField] public bool showBorders = false;
        [SerializeField] public bool showGizmos = false;
        [SerializeField] public bool playMode = false;
        [SerializeField] public string filename;

        /// <summary>
        /// Save generated terrain into file
        /// </summary>
        /// <param name="terrainData">Data of Unity Terrain object</param>
        /// <param name="terrainGenData">Terrain parameters</param>
        /// <param name="controlPoints">Loop points</param>
        /// <param name="meta">position of finish object</param>
        /// <param name="cars">list of positions of cars</param>
        /// <param name="carSize">size of car</param>
        /// <param name="checkpoints">list of positions of checkpoints</param>
        /// <param name="checkpointSize">size of checkpoint</param>
        /// <param name="filename">name od save file</param>
        public static void SaveTerrain(TerrainData terrainData, Data.TerrainGenData terrainGenData, OrientedPoint[] controlPoints, Vector3 meta,
                                       OrientedPoint[] cars, Vector3 carSize, OrientedPoint[] checkpoints, Vector3 checkpointSize, string filename) {
            StreamWriter sw = File.CreateText(Application.dataPath + "/Resources/SavedTerrains/" + filename + ".data");

            // terrain gen data
            sw.WriteLine(terrainGenData.terrainType);
            sw.WriteLine(terrainGenData.roadLength);
            sw.WriteLine(terrainGenData.roadWidth);
            sw.WriteLine(terrainGenData.numberOfSegments);
            sw.WriteLine(terrainGenData.numberOfCheckpoints);
            sw.WriteLine(terrainGenData.terrainDetailsMain);
            sw.WriteLine(terrainGenData.terrainDetailsMinor);
            sw.WriteLine(terrainGenData.terrainDetailsTiny);
            sw.WriteLine(terrainGenData.terrainScaleMain);
            sw.WriteLine(terrainGenData.terrainScaleMinor);
            sw.WriteLine(terrainGenData.terrainScaleTiny);
            sw.WriteLine(terrainGenData.offsetX);
            sw.WriteLine(terrainGenData.offsetY);

            // terrain size
            sw.WriteLine(terrainData.size);
            // terrain height map
            for (int xi = 0; xi < terrainData.size.x; xi++) {
                for (int zi = 0; zi < terrainData.size.z; zi++) {
                    float h = terrainData.GetHeight(zi, xi) / terrainData.size.y;
                    sw.Write(h.ToString() + "|");
                }
            }
            sw.Write("\n");

            // terrain texture layer names
            sw.WriteLine(terrainData.terrainLayers.Length);
            foreach (TerrainLayer layer in terrainData.terrainLayers) {
                sw.WriteLine(layer.name + "|" + layer.tileSize.x);
            }

            // terrain alpha map
            sw.WriteLine(new Vector3(terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers));
            float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            for (int xi = 0; xi < terrainData.alphamapWidth; xi++) {
                for (int yi = 0; yi < terrainData.alphamapHeight; yi++) {
                    for (int li = 0; li < terrainData.alphamapLayers; li++) {
                        sw.Write(splatmapData[yi, xi, li].ToString() + "|");
                    }
                }
            }
            sw.Write("\n");

            // loop
            sw.WriteLine(controlPoints.Length);
            foreach (OrientedPoint item in controlPoints) {
                sw.Write(item.ToString() + "|");
            }
            sw.Write("\n");

            // meta size
            sw.WriteLine(meta);

            // cars
            sw.WriteLine(cars.Length);
            foreach (OrientedPoint item in cars) {
                sw.Write(item.ToString() + "|");
            }
            sw.Write("\n");
            sw.WriteLine(carSize);

            // checkpoints
            sw.WriteLine(checkpoints.Length);
            foreach (OrientedPoint item in checkpoints) {
                sw.Write(item.ToString() + "|");
            }
            sw.Write("\n");
            sw.WriteLine(checkpointSize);

            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// Load saved terrain from file
        /// </summary>
        public void LoadTerrain(string filename, bool _playMode = true) {
            playMode = _playMode;
            if (filename.Length == 0) {
                Debug.Log("Couldn't load terrain, no filename given");
            } else {
                Debug.Log($"Loading {filename} in playmode: {_playMode}");
                LoadData(filename);
                LoadObjects(filename);
            }
        }

        /// <summary>
        /// Load Terrain object from file
        /// </summary>
        public void LoadData(string filename) {
            StreamReader sw = new StreamReader(Application.dataPath + "/Resources/SavedTerrains/" + filename + ".data");

            // terrain gen data
            terrainGenData = ScriptableObject.CreateInstance<Data.TerrainGenData>();
            terrainGenData.terrainType = (Data.TerrainType)Enum.Parse(typeof(Data.TerrainType), sw.ReadLine(), true);
            terrainGenData.roadLength = float.Parse(sw.ReadLine());
            terrainGenData.roadWidth = float.Parse(sw.ReadLine());
            terrainGenData.numberOfSegments = int.Parse(sw.ReadLine());
            terrainGenData.numberOfCheckpoints = int.Parse(sw.ReadLine());
            terrainGenData.terrainDetailsMain = float.Parse(sw.ReadLine());
            terrainGenData.terrainDetailsMinor = float.Parse(sw.ReadLine());
            terrainGenData.terrainDetailsTiny = float.Parse(sw.ReadLine());
            terrainGenData.terrainScaleMain = float.Parse(sw.ReadLine());
            terrainGenData.terrainScaleMinor = float.Parse(sw.ReadLine());
            terrainGenData.terrainScaleTiny = float.Parse(sw.ReadLine());
            terrainGenData.offsetX = float.Parse(sw.ReadLine());
            terrainGenData.offsetY = float.Parse(sw.ReadLine());

            Utils.Objects.RemoveObjectsByTagInParent("loaded-terrain", gameObject);
            terrainGO = Terrain.CreateTerrainGameObject(new TerrainData());
            terrainGO.name = "Loaded Terrain";
            terrainGO.tag = "loaded-terrain";
            terrainGO.transform.position = gameObject.transform.position;
            terrainGO.transform.parent = gameObject.transform;
            gameObject.name = "Terrain";

            // terrain size
            TerrainData terrainData = new TerrainData();
            terrainDataSize = Utils.Parser.Vector3Parse(sw.ReadLine());
            terrainData.heightmapResolution = Mathf.FloorToInt(terrainDataSize.x + 1);
            terrainData.size = terrainDataSize;

            // terrain height map
            string[] terrainHeightMap = sw.ReadLine().Split('|');
            int tempIndex = 0;
            float[,] heights = new float[Mathf.FloorToInt(terrainData.size.x), Mathf.FloorToInt(terrainData.size.z)];
            for (int xi = 0; xi < terrainData.size.x; xi++) {
                for (int zi = 0; zi < terrainData.size.z; zi++) {
                    heights[xi, zi] = float.Parse(terrainHeightMap[tempIndex]);
                    tempIndex++;
                }
            }
            terrainData.SetHeights(0, 0, heights);

            // terrain texture layer names
            List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
            string numberOfLayers = sw.ReadLine();
            for (int i = 0; i < int.Parse(numberOfLayers); i++) {
                string[] textureArray = sw.ReadLine().Split('|');
                string textureName = textureArray[0];
                float textureSize = float.Parse(textureArray[1]);
                if (textureName.StartsWith("texture_")) {
                    terrainLayers.Add(TerrainGenerator.GetTerrainLayerTexture(textureName.Substring(8, textureName.Length - 8), textureSize));
                } else if (textureName.StartsWith("color_")) {
                    string colorString = textureName.Substring(6, textureName.Length - 6);
                    Color color = Utils.Parser.ColorParse(colorString);
                    terrainLayers.Add(TerrainGenerator.GetTerrainLayerColor(color));
                }
            }
            terrainData.terrainLayers = terrainLayers.ToArray();

            // terrain alpha map
            Vector3 alphamapSize = Utils.Parser.Vector3Parse(sw.ReadLine());
            float[,,] splatmapData = new float[Mathf.FloorToInt(alphamapSize.x), Mathf.FloorToInt(alphamapSize.y), Mathf.FloorToInt(alphamapSize.z)];
            string[] terrainAlphaMap = sw.ReadLine().Split('|');
            tempIndex = 0;
            for (int xi = 0; xi < alphamapSize.x; xi++) {
                for (int yi = 0; yi < alphamapSize.y; yi++) {
                    for (int li = 0; li < alphamapSize.z; li++) {
                        splatmapData[yi, xi, li] = float.Parse(terrainAlphaMap[tempIndex]);
                        tempIndex++;
                    }
                }
            }
            terrainData.alphamapResolution = Utils.Variables.TERRAIN_SIZE * 4;
            terrainData.SetAlphamaps(0, 0, splatmapData);

            // loop
            List<OrientedPoint> controlPointsList = new List<OrientedPoint>();
            tempIndex = 0;
            string numberOfPoints = sw.ReadLine();
            string[] controlPointsString = sw.ReadLine().Split('|');
            for (int i = 0; i < int.Parse(numberOfPoints); i++) {
                controlPointsList.Add(Utils.Parser.OrientedPointParse(controlPointsString[tempIndex]));
                tempIndex++;
            }
            controlPoints = new Loop(gameObject.transform.position, controlPointsList, terrainGenData);

            // terrain trees
            int[] bigTreeInstances = { };
            int[] smallTreeInstances = { };
            int[] mainBackgroundTreeInstances = { };
            int[] otherBackgroundTreeInstances = { };
            float backgroundTreeRandomMinBound = 0.995f;
            float backgroundTreeOtherOverMainRandomMinBound = 0.7f;
            float roadTreeBigOverSmallRandomMinBound = 0.35f;
            if (terrainGenData.terrainType == Data.TerrainType.Forest) {
                TreePrototype[] tps = Enumerable.Repeat(new TreePrototype(), 19).Select(x => new TreePrototype()).ToArray();
                tps[0].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Tree");
                tps[1].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Plants1");
                tps[2].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Plants2");
                tps[3].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Plants3");
                tps[4].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Plants4");
                tps[5].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Plants5");
                tps[6].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock1");
                tps[7].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock2");
                tps[8].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock3");
                tps[9].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock4");
                tps[10].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock5");
                tps[11].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock6");
                tps[12].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock7");
                tps[13].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock8");
                tps[14].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock9");
                tps[15].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock10");
                tps[16].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock11");
                tps[17].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock12");
                tps[18].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/TreeLog");
                terrainData.treePrototypes = tps;
                bigTreeInstances = new int[] { 6, 7, 10, 11, 12, 13, 14, 15, 16, 17 };
                smallTreeInstances = new int[] { 1, 2, 3, 4, 5, 8, 9, 18 };
                mainBackgroundTreeInstances = new int[] { 0 };
                otherBackgroundTreeInstances = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
                backgroundTreeRandomMinBound = 0.995f;
                backgroundTreeOtherOverMainRandomMinBound = 0.7f;
                roadTreeBigOverSmallRandomMinBound = 0.35f;
            } else if (terrainGenData.terrainType == Data.TerrainType.Desert) {
                TreePrototype[] tps = Enumerable.Repeat(new TreePrototype(), 14).Select(x => new TreePrototype()).ToArray();
                tps[0].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/PalmTree");
                tps[1].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock3");
                tps[2].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock4");
                tps[3].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Stones1");
                tps[4].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Stones2");
                tps[5].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Stones3");
                tps[6].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Sphinx");
                tps[7].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/PyramidStepped");
                tps[8].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/PyramidSmooth");
                tps[9].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Obelisk");
                tps[10].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/ColumnRound");
                tps[11].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/ColumnSquare");
                tps[12].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock3");
                tps[13].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock4");
                terrainData.treePrototypes = tps;
                bigTreeInstances = new int[] { 9, 10, 11 };
                smallTreeInstances = new int[] { 1, 2, 3, 4, 5, 12, 13 };
                mainBackgroundTreeInstances = new int[] { 0, 3, 4, 5 };
                otherBackgroundTreeInstances = new int[] { 1, 2, 6, 7, 8, 9, 10, 11 };
                backgroundTreeRandomMinBound = 0.999f;
                backgroundTreeOtherOverMainRandomMinBound = 0.15f;
                roadTreeBigOverSmallRandomMinBound = 0.95f;
            } else if (terrainGenData.terrainType == Data.TerrainType.Mountains) {
                TreePrototype[] tps = Enumerable.Repeat(new TreePrototype(), 22).Select(x => new TreePrototype()).ToArray();
                tps[0].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock1");
                tps[1].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock2");
                tps[2].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock3");
                tps[3].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock4");
                tps[4].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock5");
                tps[5].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock6");
                tps[6].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock7");
                tps[7].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock8");
                tps[8].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock9");
                tps[9].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock10");
                tps[10].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock11");
                tps[11].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock12");
                tps[12].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock13");
                tps[13].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock14");
                tps[14].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock15");
                tps[15].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock16");
                tps[16].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock17");
                tps[17].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/SnowRock18");
                tps[18].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock9");
                tps[19].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock10");
                tps[20].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock11");
                tps[21].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Rock12");
                terrainData.treePrototypes = tps;
                bigTreeInstances = new int[] { 6, 7, 8, 9, 10, 11 };
                smallTreeInstances = new int[] { 0, 1, 2, 3, 4, 5 };
                mainBackgroundTreeInstances = new int[] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
                otherBackgroundTreeInstances = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                backgroundTreeRandomMinBound = 0.995f;
                backgroundTreeOtherOverMainRandomMinBound = 0.4f;
                roadTreeBigOverSmallRandomMinBound = 0.3f;
            }

            int GetRandomFromArray(int[] arr) => arr.Length == 0 ? 0 : arr[Mathf.FloorToInt(UnityEngine.Random.value * arr.Count())];
            // draw random trees
            List<TreeInstance> treeInstances = new List<TreeInstance>();
            for (int xi = 0; xi < alphamapSize.x; xi++) {
                for (int yi = 0; yi < alphamapSize.y; yi++) {
                    if (splatmapData[yi, xi, 0] == 0 && UnityEngine.Random.value > backgroundTreeRandomMinBound) {
                        TreeInstance ti = new TreeInstance();
                        ti.position = new Vector3(xi / alphamapSize.x, -0.1f, yi / alphamapSize.y);
                        ti.widthScale = 0.9f + (UnityEngine.Random.value / 5f);
                        ti.heightScale = 0.9f + (UnityEngine.Random.value / 5f);
                        ti.color = Color.white;
                        ti.lightmapColor = Color.white;
                        ti.prototypeIndex = GetRandomFromArray(mainBackgroundTreeInstances);
                        if (UnityEngine.Random.value > backgroundTreeOtherOverMainRandomMinBound) {
                            ti.prototypeIndex = GetRandomFromArray(otherBackgroundTreeInstances);
                        }
                        treeInstances.Add(ti);
                    }
                }
            }
            // draw across road
            List<OrientedPoint> equallySpacedPoints = controlPoints.GetEquallySpacedPoints(200);
            for (int i = 0; i < equallySpacedPoints.Count; i++) {
                OrientedPoint start = equallySpacedPoints[(i + 0) % equallySpacedPoints.Count];
                OrientedPoint end = equallySpacedPoints[(i + 1) % equallySpacedPoints.Count];
                foreach (Vector3 offset in new Vector3[] { Vector3.left, Vector3.right }) {
                    Vector3 s = start.LocalToWorldPosition(offset * terrainGenData.roadWidth * 2f * (1.1f + (UnityEngine.Random.value / 5f)));
                    Vector3 e = end.LocalToWorldPosition(offset * terrainGenData.roadWidth * 2f * (1.1f + (UnityEngine.Random.value / 5f)));
                    List<float> displacements = new List<float>();
                    List<int> prototypes = new List<int>();
                    if (UnityEngine.Random.value > roadTreeBigOverSmallRandomMinBound) {
                        // with 50% probability add one big element
                        displacements.Add(0.25f + (UnityEngine.Random.value * 0.5f));
                        prototypes.Add(GetRandomFromArray(bigTreeInstances));
                    } else {
                        // else add 8 small elements
                        for (int j = 0; j < 4; j++) {
                            displacements.Add((j / 4f) + (UnityEngine.Random.value / 4f));
                            prototypes.Add(GetRandomFromArray(smallTreeInstances));
                        }
                    }
                    for (int j = 0; j < prototypes.Count; j++) {
                        Vector3 pos = (e + (e - s) * displacements[j]) - terrainGO.transform.position;
                        TreeInstance ti = new TreeInstance();
                        ti.position = new Vector3(pos.x / Utils.Variables.TERRAIN_SIZE, (pos.y / Utils.Variables.TERRAIN_HEIGHT) - 0.1f, pos.z / Utils.Variables.TERRAIN_SIZE);
                        ti.widthScale = 0.9f + (UnityEngine.Random.value / 5f);
                        ti.heightScale = 0.9f + (UnityEngine.Random.value / 5f);
                        ti.color = Color.white;
                        ti.lightmapColor = Color.white;
                        ti.prototypeIndex = prototypes[j];
                        treeInstances.Add(ti);
                    }
                }
            }
            terrainData.SetTreeInstances(treeInstances.ToArray(), true);

            terrainGO.GetComponent<Terrain>().terrainData = terrainData;
            terrainGO.GetComponent<TerrainCollider>().terrainData = terrainData;
            terrainGO.GetComponent<TerrainCollider>().sharedMaterial = (PhysicMaterial)Resources.Load<PhysicMaterial>("Materials/TerrainPhysicMaterial");
        }

        /// <summary>
        /// Load other objects (cars, borders, checkpoints, etc) from file
        /// </summary>
        public void LoadObjects(string filename) {
            StreamReader sw = new StreamReader(Application.dataPath + "/Resources/SavedTerrains/" + filename + ".data");

            // terrain gen data
            for (int i = 0; i < 13; i++) { sw.ReadLine(); }
            // terrain size
            sw.ReadLine();
            sw.ReadLine();
            // terrain texture layer names
            string numberOfLayers = sw.ReadLine();
            for (int i = 0; i < int.Parse(numberOfLayers); i++) { sw.ReadLine(); }
            // terrain alpha map
            sw.ReadLine();
            sw.ReadLine();
            // loop
            sw.ReadLine();
            sw.ReadLine();

            // meta
            Vector3 metaSize = Utils.Parser.Vector3Parse(sw.ReadLine());
            Utils.Objects.RemoveObjectsByTagInParent("meta", terrainGO);
            OrientedPoint posrot = controlPoints.GetCentralPoint(0);
            startFinish = Utils.Objects.PutObject("StartFinish", "meta", "Meta", posrot, metaSize);
            startFinish.GetComponent<BoxCollider>().isTrigger = true;
            startFinish.transform.parent = terrainGO.transform;
            Vector3 pos = posrot.position;
            pos.y = controlPoints.GetHeight(
                pos.x - terrainGO.transform.position.x,
                pos.z - terrainGO.transform.position.z
            ) * Utils.Variables.TERRAIN_HEIGHT + (metaSize.y / 4);
            startFinish.transform.position = pos;

            // cars read
            string numberOfCars = sw.ReadLine();
            string[] carsString = sw.ReadLine().Split('|');
            Vector3 carSize = Utils.Parser.Vector3Parse(sw.ReadLine());

            // checkpoints
            checkpoints = new List<GameObject>();
            string numberOfCheckpoints = sw.ReadLine();
            string[] checkpointsString = sw.ReadLine().Split('|');
            Vector3 checkpointSize = Utils.Parser.Vector3Parse(sw.ReadLine());
            int tempIndex = 0;
            Utils.Objects.RemoveObjectsByTagInParent("checkpoint", terrainGO);
            GameObject checkpointsParent = Utils.Objects.PutParentObject("checkpoint", "Checkpoints");
            checkpointsParent.transform.parent = terrainGO.transform;
            for (int i = 0; i < int.Parse(numberOfCheckpoints); i++) {
                posrot = Utils.Parser.OrientedPointParse(checkpointsString[tempIndex]);
                GameObject checkpoint = Utils.Objects.PutObject("Checkpoint", "checkpoint", "Checkpoint " + (i + 1), posrot, checkpointSize * 1.5f);
                checkpoint.transform.parent = checkpointsParent.transform;
                checkpoint.transform.position += terrainGO.transform.position + new Vector3(Utils.Variables.TERRAIN_SIZE / 2, 0, Utils.Variables.TERRAIN_SIZE / 2);
                checkpoint.GetComponent<MeshRenderer>().enabled = showCheckpoints;
                checkpoint.GetComponent<BoxCollider>().enabled = true;
                checkpoint.GetComponent<BoxCollider>().isTrigger = true;
                checkpoints.Add(checkpoint);
                tempIndex++;
            }

            // borders
            Utils.Objects.RemoveObjectsByTagInParent("border", terrainGO);
            GameObject bordersGroup = Utils.Objects.PutParentObject("border", "Borders");
            bordersGroup.transform.parent = terrainGO.transform;
            Vector3 terrainSize = terrainGO.GetComponent<Terrain>().terrainData.size;
            Vector3 center = terrainSize / 2;
            foreach (int xi in new int[] { -1, 0, 1 }) {
                foreach (int zi in new int[] { -1, 0, 1 }) {
                    if (xi != 0 || zi != 0) {
                        if (xi == 0 || zi == 0) {
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.name = "Border";
                            cube.tag = "border";
                            cube.transform.parent = bordersGroup.transform;
                            cube.transform.localScale = new Vector3(terrainSize.x, terrainSize.y, 1f);
                            cube.transform.position = center + new Vector3(xi, 0, zi) * terrainSize.x / 2;
                            cube.transform.rotation *= Quaternion.Euler(0, 90 * xi, 0);
                            cube.transform.localPosition += terrainGO.transform.localPosition;
                            cube.transform.position += terrainGO.transform.position;
                            cube.GetComponent<BoxCollider>().isTrigger = !playMode;
                            cube.GetComponent<MeshRenderer>().enabled = showBorders;
                        }
                    }
                }
            }
            // border alongside road
            float blockHeight = Utils.Variables.TERRAIN_HEIGHT * 0.4f;
            float blockWidth = 1f;
            foreach (Vector3 offset in new Vector3[] { Vector3.left, Vector3.right }) {
                List<Vector3> outerPoints = controlPoints.GetOffsetLoop(offset * terrainGenData.roadWidth * 2f);
                for (int i = 0; i < outerPoints.Count; i++) {
                    Vector3 start = outerPoints[(i + 0) % outerPoints.Count];
                    Vector3 end = outerPoints[(i + 1) % outerPoints.Count];
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cube.name = "Border";
                    cube.tag = "border";
                    cube.transform.parent = bordersGroup.transform;
                    cube.transform.localScale = new Vector3(blockWidth, blockHeight, (end - start).magnitude);
                    pos = start + (end - start) * 0.5f;
                    pos.y = controlPoints.GetHeight(
                        pos.x - terrainGO.transform.position.x,
                        pos.z - terrainGO.transform.position.z
                    ) * Utils.Variables.TERRAIN_HEIGHT;
                    cube.transform.position = pos;
                    Vector3 forwardXZ = new Vector3(transform.forward.x, 0, transform.forward.z);
                    Vector3 positionXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 directionToPointXZ = end - start;
                    float angle = Mathf.Acos(Vector3.Dot(forwardXZ.normalized, directionToPointXZ.normalized));
                    float crossY = Vector3.Cross(forwardXZ, directionToPointXZ).y;
                    float crossYSign = crossY >= 0 ? 1f : -1f;
                    float radianANgle = angle * crossYSign; // angle in radians [-pi, pi]
                    cube.transform.rotation *= Quaternion.Euler(0, radianANgle * 180 / Mathf.PI, 0); // angle normalized to degrees [-180, 180]
                    cube.GetComponent<CapsuleCollider>().isTrigger = !playMode;
                    cube.GetComponent<MeshRenderer>().enabled = showBorders;
                }
            }

            // light
            GameObject lightGO = new GameObject("Light");
            lightGO.transform.parent = terrainGO.transform;
            Light light = lightGO.AddComponent<Light>();
            light.color = Color.white;
            light.type = LightType.Directional;
            light.intensity = 0.05f;
            lightGO.transform.localPosition = new Vector3(Utils.Variables.TERRAIN_SIZE / 2, Utils.Variables.TERRAIN_HEIGHT * 2, Utils.Variables.TERRAIN_SIZE / 2);

            // cars
            cars = new List<GameObject>();
            tempIndex = 0;
            Utils.Objects.RemoveObjectsByTagInParent("car", terrainGO);
            GameObject carsParent = Utils.Objects.PutParentObject("car", "Cars");
            carsParent.transform.parent = terrainGO.transform;
            for (int i = 0; i < int.Parse(numberOfCars); i++) {
                posrot = Utils.Parser.OrientedPointParse(carsString[tempIndex]);
                GameObject car = Utils.Objects.PutObject("SportCarAI", "car", "Car " + (i + 1), posrot, carSize);
                car.transform.parent = carsParent.transform;
                pos = posrot.position + terrainGO.transform.position + new Vector3(Utils.Variables.TERRAIN_SIZE / 2, 0, Utils.Variables.TERRAIN_SIZE / 2);
                pos.y = controlPoints.GetHeight(
                    pos.x - terrainGO.transform.position.x,
                    pos.z - terrainGO.transform.position.z
                ) * Utils.Variables.TERRAIN_HEIGHT;
                pos -= posrot.rotation * Vector3.forward * 7f;
                pos.y += 5f;
                car.transform.position = pos;
                car.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
                car.GetComponent<Play.CarAgent>().showGizmos = showGizmos;
                car.GetComponent<Play.CarAgent>().playMode = playMode;
                cars.Add(car);
                tempIndex++;
            }
            cars[cars.Count - 1].GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
        }
    }
}
