using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ButtonActions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    [SerializeField, HideInInspector] private TMPro.TMP_Text text;
    [SerializeField, HideInInspector] private Color defaultColor = Color.white;
    [SerializeField, HideInInspector] private Color hoverColor = Color.yellow;
    [SerializeField, HideInInspector] private float defaultFontSize;
    [SerializeField, HideInInspector] private float hoverFontSize;
    [SerializeField, HideInInspector] private float clickFontSize;

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

    public void OnPointerEnter(PointerEventData eventData) {
        if (text != null) {
            text.color = hoverColor;
            text.fontSize = hoverFontSize;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (text != null) {
            text.color = defaultColor;
            text.fontSize = defaultFontSize;
        }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
        if (text != null) {
            text.color = hoverColor;
            text.fontSize = clickFontSize;
        }
    }

    private void OnEnable() => this.OnPointerExit(null);
    private void OnDisable() => this.OnPointerExit(null);
    private void OnDestroy() => this.OnPointerExit(null);
}