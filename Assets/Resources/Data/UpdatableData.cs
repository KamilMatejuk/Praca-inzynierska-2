using UnityEngine;

namespace RacingGameBot.Data {
    public class UpdatableData : ScriptableObject {

        public static event System.Action OnValuesUpdated;
        public bool autoUpdate = true;

        protected virtual void OnValidate() {
            #if UNITY_EDITOR
                if (autoUpdate) {
                    UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
                }
        #endif
        }

        public void NotifyOfUpdatedValues() {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
                if (OnValuesUpdated != null) {
                    OnValuesUpdated();
                }
            #endif
        }
    }
}
