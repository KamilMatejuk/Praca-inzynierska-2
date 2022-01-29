using UnityEngine;

public class MultiTraining : MonoBehaviour {

    [SerializeField] public bool playMode = false;
    [SerializeField] public bool showGizmos = false;
    [SerializeField] public string filenames = "";
    [SerializeField] public int numberOfInstances = 1;

    /// <summary>
    /// Load multiple terrains at once, based on supplied filenames
    /// </summary>
    void Start() {
        int spacing = Mathf.FloorToInt(Variables.TERRAIN_SIZE * 1.1f);
        if (filenames == null || filenames == "") {
            Debug.LogWarning("Filename not specified");
            return;
        }
        int fileIndex = 0;
        foreach(string file in filenames.Split(',')) {
            for (int i = 0; i < numberOfInstances; i++) {
                Vector3 position = new Vector3(fileIndex, 0, i) * spacing;
                GameObject terrain = new GameObject("Terrain");
                terrain.transform.parent = transform;
                terrain.transform.position = position - new Vector3(Variables.TERRAIN_SIZE / 2, 0, Variables.TERRAIN_SIZE / 2);
                TerrainLoader terrainLoader = terrain.AddComponent<TerrainLoader>();
                terrainLoader.filename = file;
                terrainLoader.playMode = playMode;
                terrainLoader.showGizmos = showGizmos;
                terrain.GetComponent<TerrainLoader>().LoadTerrain(file, false);
            }
            fileIndex++;
        }
    }
}
