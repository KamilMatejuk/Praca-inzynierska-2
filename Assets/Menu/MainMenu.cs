using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour {

    [SerializeField, HideInInspector] public string filename;
    [SerializeField, HideInInspector] public List<TMPro.TMP_Dropdown.OptionData> options;
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject playMenu;

    /// <summary>
    /// Open play screen
    /// </summary>
    public void MenuOptionPlayGame() {
        if (filename.Length != 0) {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// Open level creation screen
    /// </summary>
    public void MenuOptionCreateLevel() {
        SceneManager.LoadScene("Create Level", LoadSceneMode.Single);
    }

    /// <summary>
    /// Quit game
    /// </summary>
    public void MenuOptionQuitGame() {
        Application.Quit();
    }

    /// <summary>
    /// Open play level choosing screen
    /// </summary>
    public void MenuOptionMoveToPlayMenu() {
        mainMenu.SetActive(false);
        playMenu.SetActive(true);
        GameObject dropdown = Objects.GetChildWithName(playMenu, "Dropdown");
        options = new List<TMPro.TMP_Dropdown.OptionData>();
        string path = Application.dataPath + "/Resources/SavedTerrains/";
        foreach (FileInfo file in new DirectoryInfo(path).GetFiles()) {
            if (!file.Name.Contains(".meta")) {
                options.Add(new TMPro.TMP_Dropdown.OptionData(file.Name));
            }
        }
        dropdown.GetComponent<TMPro.TMP_Dropdown>().options = options;
        MenuOptionChooseFilename(0);
    }

    /// <summary>
    /// Get back from level choosing screen to main screen
    /// </summary>
    public void MenuOptionMoveToMainMenu() {
        playMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    /// <summary>
    /// Select level name
    /// </summary>
    /// <param name="file">Number of file on the list</param>
    public void MenuOptionChooseFilename(int file) {
        try {
            filename = options[file].text.Replace(".data", "");
        } catch {}
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoadCallback;
    }

    /// <summary>
    /// Pass selected level filename to loaded game
    /// </summary>
    /// <param name="scene">Loaded scene</param>
    /// <param name="sceneMode">Scene mode</param>
    void OnSceneLoadCallback(Scene scene, LoadSceneMode sceneMode) {
        if (scene.name == "Game") {
            GameObject terrainGO = GameObject.Find("Terrain");
            if (terrainGO.GetComponent<TerrainLoader>().filename.Length == 0 && filename.Length != 0) {
                terrainGO.GetComponent<TerrainLoader>().filename = filename;
                Debug.Log("set filename " + filename + " " + terrainGO.GetComponent<TerrainLoader>().filename);
            }
            terrainGO.GetComponent<TerrainLoader>().LoadTerrain(filename, true);
        }
    }
}
