using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;

public class TerrainLoader : MonoBehaviour {

    [SerializeField, HideInInspector] public string basePath;
    [SerializeField, HideInInspector] public TerrainGenData terrainGenData;
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

    public static void SaveTerrain(TerrainData terrainData, TerrainGenData terrainGenData, OrientedPoint[] controlPoints, Vector3 meta,
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

    public void LoadData(string filename) {
        StreamReader sw = new StreamReader(Application.dataPath + "/Resources/SavedTerrains/" + filename + ".data");

        // terrain gen data
        terrainGenData = ScriptableObject.CreateInstance<TerrainGenData>();
        terrainGenData.terrainType = (TerrainType)Enum.Parse(typeof(TerrainType), sw.ReadLine(), true);
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

        Objects.RemoveObjectsByTagInParent("loaded-terrain", gameObject);
        terrainGO = Terrain.CreateTerrainGameObject(new TerrainData());
        terrainGO.name = "Loaded Terrain";
        terrainGO.tag = "loaded-terrain";
        terrainGO.transform.position = gameObject.transform.position;
        terrainGO.transform.parent = gameObject.transform;
        gameObject.name = "Terrain";

        // terrain size
        TerrainData terrainData = new TerrainData();
        terrainDataSize = Parser.Vector3Parse(sw.ReadLine());
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
                Color color = Parser.ColorParse(colorString);
                terrainLayers.Add(TerrainGenerator.GetTerrainLayerColor(color));
            }
        }
        // terrainData.terrainLayers = terrainLayers.ToArray();
        terrainData.SetTerrainLayersRegisterUndo(terrainLayers.ToArray(), "terrain-layers");

        // terrain alpha map
        Vector3 alphamapSize = Parser.Vector3Parse(sw.ReadLine());
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
        terrainData.alphamapResolution = Variables.TERRAIN_SIZE * 4;
        terrainData.SetAlphamaps(0, 0, splatmapData);

        // terrain trees
        if (terrainGenData.terrainType == TerrainType.Desert || terrainGenData.terrainType == TerrainType.Forest) {
            TreePrototype[] tps = new TreePrototype[] { new TreePrototype(), new TreePrototype() };
            tps[0].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/PalmTree");
            tps[1].prefab = (GameObject)Resources.Load<GameObject>("Prefabs/Tree");
            terrainData.treePrototypes = tps;
            List<TreeInstance> treeInstances = new List<TreeInstance>();
            for (int xi = 0; xi < alphamapSize.x; xi++) {
                for (int yi = 0; yi < alphamapSize.y; yi++) {
                    if (splatmapData[yi, xi, 0] == 0 && UnityEngine.Random.value > 0.995f) {
                        TreeInstance ti = new TreeInstance();
                        ti.position = new Vector3(xi / alphamapSize.x, 0, yi / alphamapSize.y);
                        ti.widthScale = 1f;
                        ti.heightScale = 1f;
                        ti.color = Color.white;
                        ti.lightmapColor = Color.white;
                        if (terrainGenData.terrainType == TerrainType.Desert) {
                            ti.prototypeIndex = 0;
                        } else if (terrainGenData.terrainType == TerrainType.Forest) {
                            ti.prototypeIndex = 1;
                        }
                        treeInstances.Add(ti);
                    }
                }
            }
            terrainData.SetTreeInstances(treeInstances.ToArray(), true);
        }

        terrainGO.GetComponent<Terrain>().terrainData = terrainData;
        terrainGO.GetComponent<TerrainCollider>().terrainData = terrainData;
        terrainGO.GetComponent<TerrainCollider>().sharedMaterial = (PhysicMaterial)Resources.Load<PhysicMaterial>("Materials/TerrainPhysicMaterial");

        // loop
        List<OrientedPoint> controlPointsList = new List<OrientedPoint>();
        tempIndex = 0;
        string numberOfPoints = sw.ReadLine();
        string[] controlPointsString = sw.ReadLine().Split('|');
        for (int i = 0; i < int.Parse(numberOfPoints); i++) {
            controlPointsList.Add(Parser.OrientedPointParse(controlPointsString[tempIndex]));
            tempIndex++;
        }
        controlPoints = new Loop(gameObject.transform.position, controlPointsList, terrainGenData);
    }

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
        Vector3 metaSize = Parser.Vector3Parse(sw.ReadLine());
        Objects.RemoveObjectsByTagInParent("meta", terrainGO);
        OrientedPoint posrot = controlPoints.GetCentralPoint(0);
        posrot.position.y += metaSize.y / 2;
        startFinish = Objects.PutObject("StartFinish", "meta", "Meta", posrot, metaSize);
        startFinish.GetComponent<BoxCollider>().isTrigger = true;
        startFinish.transform.parent = terrainGO.transform;

        // cars read
        string numberOfCars = sw.ReadLine();
        string[] carsString = sw.ReadLine().Split('|');
        Vector3 carSize = Parser.Vector3Parse(sw.ReadLine());

        // checkpoints
        checkpoints = new List<GameObject>();
        string numberOfCheckpoints = sw.ReadLine();
        string[] checkpointsString = sw.ReadLine().Split('|');
        Vector3 checkpointSize = Parser.Vector3Parse(sw.ReadLine());
        int tempIndex = 0;
        Objects.RemoveObjectsByTagInParent("checkpoint", terrainGO);
        GameObject checkpointsParent = Objects.PutParentObject("checkpoint", "Checkpoints");
        checkpointsParent.transform.parent = terrainGO.transform;
        for (int i = 0; i < int.Parse(numberOfCheckpoints); i++) {
            posrot = Parser.OrientedPointParse(checkpointsString[tempIndex]);
            GameObject checkpoint = Objects.PutObject("Checkpoint", "checkpoint", "Checkpoint " + (i + 1), posrot, checkpointSize);
            checkpoint.transform.parent = checkpointsParent.transform;
            // checkpoint.transform.localPosition += terrainGO.transform.localPosition;
            checkpoint.transform.position += terrainGO.transform.position + new Vector3(Variables.TERRAIN_SIZE / 2, 0, Variables.TERRAIN_SIZE / 2);
            checkpoint.GetComponent<MeshRenderer>().enabled = showCheckpoints;
            checkpoint.GetComponent<BoxCollider>().enabled = true;
            checkpoint.GetComponent<BoxCollider>().isTrigger = true;
            // checkpoint.layer = 2; // Ignore Raycast Layer
            checkpoints.Add(checkpoint);
            tempIndex++;
        }

        // borders
        Objects.RemoveObjectsByTagInParent("border", terrainGO);
        GameObject bordersGroup = Objects.PutParentObject("border", "Borders");
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

                        cube.GetComponent<BoxCollider>().isTrigger = true;
                        cube.GetComponent<MeshRenderer>().enabled = showBorders;
                    }
                }
            }
        }
        // border alongside road
        int details = 100;
        List<OrientedPoint> equallySpacedPoints = controlPoints.GetEquallySpacedPoints(details);
        float blockHeight = terrainSize.y * 0.4f;
        float blockWidth = 1f;
        GameObject roadBordersGroup = Objects.PutParentObject("border", "Borders");
        roadBordersGroup.transform.parent = bordersGroup.transform;
        void AddBorderSegment(Vector3 offset, int i) {
            Vector3 curr = equallySpacedPoints[(i + 0) % equallySpacedPoints.Count].LocalToWorldPosition(offset * terrainGenData.roadWidth * 2f);
            Vector3 next = equallySpacedPoints[(i + 1) % equallySpacedPoints.Count].LocalToWorldPosition(offset * terrainGenData.roadWidth * 2f);
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "BorderRoad";
            cube.tag = "border";
            cube.transform.parent = roadBordersGroup.transform;
            cube.transform.localScale = new Vector3(blockWidth, blockHeight, (next - curr).magnitude);
            cube.transform.position = next + (next - curr) * 0.5f;
            Vector3 forwardXZ = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 positionXZ = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 directionToPointXZ = next - curr;
            float angle = Mathf.Acos(Vector3.Dot(forwardXZ.normalized, directionToPointXZ.normalized));
            float crossY = Vector3.Cross(forwardXZ, directionToPointXZ).y;
            float crossYSign = crossY >= 0 ? 1f : -1f;
            float radianANgle = angle * crossYSign; // angle in radians [-pi, pi]
            cube.transform.rotation *= Quaternion.Euler(0, radianANgle * 180 / Mathf.PI, 0); // angle normalized to degrees [-180, 180]
            cube.GetComponent<BoxCollider>().isTrigger = true;
            cube.GetComponent<MeshRenderer>().enabled = showBorders;
        }
        for (int i = 0; i < equallySpacedPoints.Count; i++) {
            AddBorderSegment(Vector3.left, i);
            AddBorderSegment(Vector3.right, i);
        }

        // light
        GameObject lightGO = new GameObject("Light");
        lightGO.transform.parent = terrainGO.transform;
        Light light = lightGO.AddComponent<Light>();
        light.color = Color.white;
        light.type = LightType.Directional;
        light.intensity = 1;
        lightGO.transform.localPosition = new Vector3(Variables.TERRAIN_SIZE / 2, Variables.TERRAIN_HEIGHT * 2, Variables.TERRAIN_SIZE / 2);

        // cars
        cars = new List<GameObject>();
        tempIndex = 0;
        Objects.RemoveObjectsByTagInParent("car", terrainGO);
        GameObject carsParent = Objects.PutParentObject("car", "Cars");
        carsParent.transform.parent = terrainGO.transform;
        for (int i = 0; i < int.Parse(numberOfCars); i++) {
            posrot = Parser.OrientedPointParse(carsString[tempIndex]);
            GameObject car = Objects.PutObject("SportCar", "car", "Car " + (i + 1), posrot, carSize);
            car.transform.parent = carsParent.transform;
            car.transform.position += terrainGO.transform.position + new Vector3(Variables.TERRAIN_SIZE / 2, 5, Variables.TERRAIN_SIZE / 2);
            car.transform.position -= posrot.rotation * Vector3.forward * 5;
            // behaviour parameters
            BehaviorParameters behaviorParameters = car.AddComponent<BehaviorParameters>();
            string name = "in5-out1-f";
            behaviorParameters.BehaviorName = name;
            behaviorParameters.BrainParameters.VectorObservationSize = 5; // number of input values
            behaviorParameters.BrainParameters.NumStackedVectorObservations = 1;
            behaviorParameters.BrainParameters.ActionSpec = new ActionSpec(0, new int[] { 3, 3 }); // continuous outputs, descrete outputs
            // behaviorParameters.Model = Ai.LoadModel1("Assets/Resources/brain.onnx");
            // behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
            if (playMode && i == int.Parse(numberOfCars) - 1) {
                // first car is playable
                GameObject cameraGO = Objects.GetChildWithName(car, "Play Camera");
                Camera camera = cameraGO.GetComponent<Camera>();
                camera.enabled = true;
                behaviorParameters.BehaviorType = BehaviorType.HeuristicOnly;
            } else {
                RayPerceptionSensorComponent3D rs = car.AddComponent<RayPerceptionSensorComponent3D>();
                rs.DetectableTags = new List<string>() { "car", "border", "checkpoint" };
                rs.RaysPerDirection = 4;
                rs.MaxRayDegrees = 80;
                if (terrainGenData != null) {
                    rs.RayLength = terrainGenData.roadWidth * 5;
                }
                rs.StartVerticalOffset = 0.5f;
                rs.EndVerticalOffset = 0.5f;
            }

            car.AddComponent<CarAgent>();
            car.GetComponent<CarAgent>().showGizmos = showGizmos;
            car.AddComponent<DecisionRequester>();
            cars.Add(car);
            tempIndex++;
        }


        // sprawdzić czy działa zczytywanie odpowiedniej wysokości
    }
}
