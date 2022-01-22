using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool gameIsPaused = false;

    public GameObject pauseMenu;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (gameIsPaused) {
                ResumeGame();
            } else {
                PauseGame();
            }
        }
    }

    public void ResumeGame() {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    public void PauseGame() {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void QuitGame() {
        ResumeGame();
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
    }
}
