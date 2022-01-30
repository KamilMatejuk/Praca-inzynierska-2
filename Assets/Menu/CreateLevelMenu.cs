using UnityEngine.SceneManagement;
using UnityEngine;
using System;


namespace RacingGameBot.Menu {
    public class CreateLevelMenu : MonoBehaviour {

        [SerializeField, HideInInspector] private int seed = 0;
        [SerializeField, HideInInspector] private string filename;
        [SerializeField, HideInInspector] private int prevComplexity;
        [SerializeField, HideInInspector] private Vector3 cameraDir;
        [SerializeField] public GameObject terrainGO;
        [SerializeField] public GameObject cameraGO;
        [SerializeField] public GameObject panelGO;
        [SerializeField] public GameObject loadingGO;

        /// <summary>
        /// Generate staring terrain preview
        /// </summary>
        void Start() {
            cameraDir = new Vector3(165, 155, 215);
            Rotate(0);
            terrainGO.GetComponent<Terrain>().terrainData.heightmapResolution = Utils.Variables.TERRAIN_SIZE + 1;
            terrainGO.GetComponent<Terrain>().terrainData.size = new Vector3(Utils.Variables.TERRAIN_SIZE, Utils.Variables.TERRAIN_HEIGHT, Utils.Variables.TERRAIN_SIZE);
            terrainGO.transform.position = new Vector3(Utils.Variables.TERRAIN_SIZE / -2, 0, Utils.Variables.TERRAIN_SIZE / -2);
            terrainGO.AddComponent<Terrains.TerrainGenerator>();
            Data.TerrainGenData terrainGenData = ScriptableObject.CreateInstance<Data.TerrainGenData>();
            terrainGenData.terrainType = Data.TerrainType.Forest;
            terrainGenData.numberOfCheckpoints = 40;
            terrainGO.GetComponent<Terrains.TerrainGenerator>().seed = seed;
            terrainGO.GetComponent<Terrains.TerrainGenerator>().generateOnChanges = true;
            terrainGO.GetComponent<Terrains.TerrainGenerator>().showRoadBezier = true;
            terrainGO.GetComponent<Terrains.TerrainGenerator>().terrainGenData = terrainGenData;
            terrainGO.GetComponent<Terrains.TerrainGenerator>().GenerateTerrain();
            terrainGO.GetComponent<Terrains.TerrainGenerator>().RemoveTerrainTextures();
        }

        /// <summary>
        /// Rotate terrain preview
        /// </summary>
        /// <param name="angle">Angle</param>
        public void Rotate(float angle) {
            cameraGO.transform.position = Quaternion.Euler(0, angle, 0) * cameraDir;
            cameraGO.transform.rotation = Quaternion.Euler(30, angle - 155, 0);
        }

        /// <summary>
        /// Change road complexity
        /// </summary>
        /// <param name="value">Complexity</param>
        public void MenuOptionChangeComplexity(float value) {
            int complexity = Mathf.RoundToInt(value);
            if (complexity != prevComplexity) {
                terrainGO.GetComponent<Terrains.TerrainGenerator>().terrainGenData.numberOfSegments = complexity;
                terrainGO.GetComponent<Terrains.TerrainGenerator>().GenerateLoop();
                terrainGO.GetComponent<Terrains.TerrainGenerator>().ShowRoadBezier();
                prevComplexity = complexity;
            }
        }

        /// <summary>
        /// Pass parameters changes into terrain object
        /// </summary>
        public void MenuOptionChangeDetailsMain(float value) => MenuOptionChangeOneValue<float>(value, "terrainDetailsMain");
        public void MenuOptionChangeDetailsMinor(float value) => MenuOptionChangeOneValue<float>(value, "terrainDetailsMinor");
        public void MenuOptionChangeDetailsTiny(float value) => MenuOptionChangeOneValue<float>(value, "terrainDetailsTiny");
        public void MenuOptionChangeScaleMain(float value) => MenuOptionChangeOneValue<float>(value, "terrainScaleMain");
        public void MenuOptionChangeScaleMinor(float value) => MenuOptionChangeOneValue<float>(value, "terrainScaleMinor");
        public void MenuOptionChangeScaleTiny(float value) => MenuOptionChangeOneValue<float>(value, "terrainScaleTiny");
        public void MenuOptionChangeOffsetX(float value) => MenuOptionChangeOneValue<float>(value, "offsetX");
        public void MenuOptionChangeOffsetY(float value) => MenuOptionChangeOneValue<float>(value, "offsetY");
        public void MenuOptionChangeTerrainType(int value) => MenuOptionChangeOneValue<Data.TerrainType>((Data.TerrainType)value, "terrainType");

        /// <summary>
        /// Change road generation seed
        /// </summary>
        /// <param name="value">Seed</param>
        public void MenuOptionChangeSeed(string value) {
            try {
                seed = int.Parse(value);
                terrainGO.GetComponent<Terrains.TerrainGenerator>().seed = seed;
                terrainGO.GetComponent<Terrains.TerrainGenerator>().GenerateTerrain();
            } catch { }
        }

        /// <summary>
        /// Change save filename
        /// </summary>
        /// <param name="value">Complexity</param>
        public void MenuOptionSetFilename(string value) {
            if (value.Length > 0) {
                filename = value.Split('.')[0];
            }
        }

        /// <summary>
        /// Save generated terrain
        /// </summary>
        public void Save() {
            panelGO.SetActive(true);
            if (filename == null || filename.Length == 0) {
                int seconds = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                filename = "terrain-" + seconds;
            }
            terrainGO.GetComponent<Terrains.TerrainGenerator>().filename = filename;
            StartCoroutine(terrainGO.GetComponent<Terrains.TerrainGenerator>().GenerateTerrainTextures(SaveCallbackUpdateTime, SaveCallbackEnd));
        }

        /// <summary>
        /// Update saving progress
        /// </summary>
        public void SaveCallbackUpdateTime(int percentage) {
            loadingGO.GetComponent<TMPro.TMP_Text>().text = $"Saving ...\n({percentage}%)";
        }

        /// <summary>
        /// Open main menu after finished saving
        /// </summary>
        public void SaveCallbackEnd() {
            terrainGO.GetComponent<Terrains.TerrainGenerator>().Save();
            Back();
        }

        /// <summary>
        /// Come back to main menu
        /// </summary>
        public void Back() {
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }

        /// <summary>
        /// Change terrain parameter by name
        /// </summary>
        /// <param name="value">New value</param>
        /// <param name="name">Field name</param>
        private void MenuOptionChangeOneValue<T>(T value, string name) {
            Data.TerrainGenData obj = terrainGO.GetComponent<Terrains.TerrainGenerator>().terrainGenData;
            typeof(Data.TerrainGenData).GetField(name).SetValue(obj, value);
            terrainGO.GetComponent<Terrains.TerrainGenerator>().GenerateTerrain();
        }
    }
}
