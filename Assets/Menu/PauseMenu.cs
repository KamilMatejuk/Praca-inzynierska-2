using UnityEngine;
using UnityEngine.SceneManagement;


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
