using UnityEngine;

namespace RacingGameBot.Play {
    public class CameraManager : MonoBehaviour {

        [SerializeField] public Camera[] cameras;
        [SerializeField, HideInInspector] private int activeCameraIndex = 0;

        /// <summary>
        /// Disable all cameras except first one
        /// </summary>
        void Start() {
            if (cameras.Length == 0) return;
            cameras[0].enabled = true;
            for (int i = 1; i < cameras.Length; i++) {
                cameras[i].enabled = false;
            }
        }

        /// <summary>
        /// Switch to next camera in queue
        /// </summary>
        public void SwitchCamera() {
            if (cameras.Length < 2) return;
            int nextIndex = (activeCameraIndex + 1) % cameras.Length;
            cameras[nextIndex].enabled = true;
            cameras[activeCameraIndex].enabled = false;
            activeCameraIndex = nextIndex;
        }
    }
}
