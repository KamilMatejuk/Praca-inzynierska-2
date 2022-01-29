using UnityEngine;
using UnityEngine.EventSystems;

namespace RacingGameBot.Menu {

    public class ButtonActions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

        [SerializeField, HideInInspector] private TMPro.TMP_Text text;
        [SerializeField, HideInInspector] private Color defaultColor = Color.white;
        [SerializeField, HideInInspector] private Color hoverColor = Color.yellow;
        [SerializeField, HideInInspector] private float defaultFontSize;
        [SerializeField, HideInInspector] private float hoverFontSize;
        [SerializeField, HideInInspector] private float clickFontSize;

        /// <summary>
        /// Get reference to button text
        /// </summary>
        private void Start() {
            Transform trans = gameObject.transform;
            for (int i = 0; i < trans.childCount; i++) {
                GameObject childGO = trans.GetChild(i).gameObject;
                if (childGO != null) {
                    TMPro.TMP_Text childText = childGO.GetComponent<TMPro.TMP_Text>();
                    if (childText != null) {
                        text = childText;
                        defaultFontSize = text.fontSize;
                        hoverFontSize = defaultFontSize * 1.05f;
                        clickFontSize = defaultFontSize * 1.1f;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Change text size and color on hover enter
        /// </summary>
        /// <param name="eventData">Hover event data</param>
        public void OnPointerEnter(PointerEventData eventData) {
            if (text != null) {
                text.color = hoverColor;
                text.fontSize = hoverFontSize;
            }
        }

        /// <summary>
        /// Change text size and color on hover exit
        /// </summary>
        /// <param name="eventData">Hover event data</param>
        public void OnPointerExit(PointerEventData eventData) {
            if (text != null) {
                text.color = defaultColor;
                text.fontSize = defaultFontSize;
            }
        }

        /// <summary>
        /// Change text size and color on click
        /// </summary>
        /// <param name="eventData">Click event data</param>
        public void OnPointerClick(PointerEventData eventData) {
            if (text != null) {
                text.color = hoverColor;
                text.fontSize = clickFontSize;
            }
        }

        /// <summary>
        /// Reset text to stating values at the beginning and end of its existence
        /// </summary>
        private void OnEnable() => this.OnPointerExit(null);
        private void OnDisable() => this.OnPointerExit(null);
        private void OnDestroy() => this.OnPointerExit(null);
    }
}
