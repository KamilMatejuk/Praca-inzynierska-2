using UnityEngine;
using UnityEngine.SceneManagement;

namespace RacingGameBot.Menu {
    public class EndMenu : MonoBehaviour {

        public GameObject endMenu;
        public GameObject winningText;

        /// <summary>
        /// Show menu at the end of race
        /// </summary>
        /// <param name="won">Whether user won or lost</param>
        public void Show(bool won) {
            if (won) {
                winningText.GetComponent<TMPro.TMP_Text>().text = "You won";
            } else {
                winningText.GetComponent<TMPro.TMP_Text>().text = "You lose";
            }
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
