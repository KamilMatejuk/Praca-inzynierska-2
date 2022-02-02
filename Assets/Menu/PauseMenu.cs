using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


namespace RacingGameBot.Menu {
    public class PauseMenu : MonoBehaviour {

        public static bool gameIsPaused = false;
        public GameObject pauseMenu;

        public float timeInStoppedState = 0;
        private float stopStateStarted;

        /// <summary>
        /// Show / hide menu when user clicked Escape
        /// </summary>
        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (gameIsPaused) {
                    ResumeGame();
                } else {
                    PauseGame();
                }
            } else if (Input.GetKeyDown(KeyCode.C) && !gameIsPaused) {
                try {
                    List<GameObject> cars = GameObject.Find("Terrain").GetComponent<Terrains.TerrainLoader>().cars;
                    GameObject car = cars[cars.Count - 1];
                    car.GetComponent<Play.CameraManager>().SwitchCamera();
                } catch { }
            }
        }

        /// <summary>
        /// Hide menu and resume game
        /// </summary>
        public void ResumeGame() {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            gameIsPaused = false;
            timeInStoppedState += Time.realtimeSinceStartup - stopStateStarted;
        }

        /// <summary>
        /// Show menu and pause game
        /// </summary>
        public void PauseGame() {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            gameIsPaused = true;
            stopStateStarted = Time.realtimeSinceStartup;

        }

        /// <summary>
        /// Quit game and open main menu
        /// </summary>
        public void QuitGame() {
            ResumeGame();
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }
    }
}
