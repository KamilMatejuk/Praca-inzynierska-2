using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MultiTraining : MonoBehaviour {

    [SerializeField, HideInInspector]
    private int spacing = Mathf.FloorToInt(Variables.TERRAIN_SIZE * 1.2f);

    [SerializeField]
    public bool playMode = false;
    [SerializeField]
    public bool showGizmos = false;

    [SerializeField]
    public bool eachInput = false;
    [SerializeField]
    public bool eachOutput = false;
    [SerializeField]
    public bool eachOutsideRoad = false;

    [SerializeField]
    public string filenames = "simple1,simple3,simple4,simple5";

    [SerializeField]
    public int numberOfInstances = 1;

    void Start() {
        if (filenames == null || filenames == "") {
            Debug.LogWarning("Filename not specified");
            return;
        }
        // foreach(InputType input in InputType.GetValues(typeof(InputType))) {
        //     foreach(OutputType output in OutputType.GetValues(typeof(OutputType))) {
        //         foreach(string outside in new string[]{"f", "t"}) {
        //             for (int i = 0; i < 4; i++){
        //                 int posZ = 0;
        //                 if (eachOutsideRoad) posZ = (int)output * 2 + (outside == "t" ? 1 : 0);
        //                 else posZ = (int)output;
        //                 Vector3 pos = new Vector3((int)input + i, 0, posZ);
        //                 LoadTerrain(input, output, outside, pos * spacing);
        //             }
        //             if (!eachOutsideRoad) break;
        //         }
        //         if (!eachOutput) break;
        //     }
        //     if (!eachInput) break;
        // }
        int fileIndex = 0;
        foreach(string file in filenames.Split(',')) {
            for (int i = 0; i < numberOfInstances; i++) {
                Vector3 pos = new Vector3(fileIndex, 0, i);
                LoadTerrain(pos * spacing, file);
            }
            fileIndex++;
        }
    }

    private void LoadTerrain(Vector3 position, string filename) {
        string configName = "in5-out1-f";
        // terrain
        GameObject terrain = new GameObject("Terrain " + configName);
        terrain.transform.parent = transform;
        terrain.transform.position = position - new Vector3(Variables.TERRAIN_SIZE / 2, 0, Variables.TERRAIN_SIZE / 2);
        TerrainLoader terrainLoader = terrain.AddComponent<TerrainLoader>();
        terrainLoader.filename = filename;
        terrainLoader.playMode = playMode;
        terrainLoader.showGizmos = showGizmos;
        terrain.GetComponent<TerrainLoader>().LoadTerrain(filename, false);
        // light
        GameObject lightGO = new GameObject("Light");
        lightGO.transform.parent = terrain.transform;
        Light light = lightGO.AddComponent<Light>();
        light.color = Color.white;
        light.type = LightType.Directional;
        light.intensity = 5;
        lightGO.transform.localPosition = new Vector3(spacing/2, spacing, spacing/2);
    }

}
