using UnityEngine;
using UnityEditor;

namespace RacingGameBot.Editors {
    [CustomEditor(typeof(Data.UpdatableData), true)]
    public class UpdatableDataEditor : Editor {

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();
            Data.UpdatableData data = (Data.UpdatableData)target;

            if (GUILayout.Button("Update")) {
                data.NotifyOfUpdatedValues();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
