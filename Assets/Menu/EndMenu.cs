using UnityEngine;
using UnityEngine.SceneManagement;

namespace RacingGameBot.Menu {
    public class EndMenu : MonoBehaviour {

        public GameObject endMenu;
        public GameObject winningText;
        public GameObject timeText;

        /// <summary>
        /// Show menu at the end of race
        /// </summary>
        /// <param name="won">Whether user won or lost</param>
        /// <param name="time">Time since start</param>
        public void Show(bool won, float time) {
            string status = won ? "won" : "lost";
            winningText.GetComponent<TMPro.TMP_Text>().text = $"You {status}";
            time -= GetComponent<Menu.PauseMenu>().timeInStoppedState;
            int minutes = Mathf.FloorToInt(time / 60f);
            time -= minutes * 60f;
            int seconds = Mathf.FloorToInt(time);
            time -= seconds;
            int milliseconds = Mathf.FloorToInt(time * 1000f);
            timeText.GetComponent<TMPro.TMP_Text>().text = $"Time: {minutes}:{seconds.ToString("00")}.{milliseconds.ToString("00")}";
            endMenu.SetActive(true);
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Quit game and return to main menu
        /// </summary>
        public void QuitGame() {
            endMenu.SetActive(false);
            Time.timeScale = 1f;
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }
    }
}
