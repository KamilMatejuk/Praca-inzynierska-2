using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour {

    public GameObject endMenu;
    public GameObject winningText;

    public void Show(bool won) {
        if (won) {
            winningText.GetComponent<TMPro.TMP_Text>().text = "You won";
        } else {
            winningText.GetComponent<TMPro.TMP_Text>().text = "You lose";
        }
        endMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void QuitGame() {
        endMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
    }
}
