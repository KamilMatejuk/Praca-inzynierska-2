using System.Collections;
using UnityEngine;


namespace RacingGameBot.Menu {
    public class GameMenu : MonoBehaviour {

        public GameObject gameMenu;

        /// <summary>
        /// Notify user of missed checkpoint
        /// </summary>
        public void ShowMessage() {
            StartCoroutine(ShowAndHide());
        }

        /// <summary>
        /// Show missed checkpoint info, wait 5 seconds and hide it
        /// </summary>
        private IEnumerator ShowAndHide() {
            gameMenu.SetActive(true);
            yield return new WaitForSeconds(5);
            gameMenu.SetActive(false);
        }
    }
}
