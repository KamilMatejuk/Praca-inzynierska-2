using UnityEngine;

namespace RacingGameBot.Play {
    public class CameraManager : MonoBehaviour {

        [SerializeField] public Camera[] cameras;
        [SerializeField, HideInInspector] private int activeCameraIndex = 0;
        [SerializeField, HideInInspector] private bool active = false;

        /// <summary>
        /// Disable all cameras except first one
        /// </summary>
        void Start() {
            if (!active) return;
            foreach (Camera camera in cameras) {
                camera.enabled = false;
            }
        }

        /// <summary>
        /// Enable cameras in game object
        /// </summary>
        public void Activate() {
            active = true;
            cameras[0].enabled = true;
        }

        /// <summary>
        /// Switch to next camera in queue
        /// </summary>
        public void SwitchCamera() {
            if (!active || cameras.Length < 2) return;
            int nextIndex = (activeCameraIndex + 1) % cameras.Length;
            cameras[nextIndex].enabled = true;
            cameras[activeCameraIndex].enabled = false;
            activeCameraIndex = nextIndex;
        }
    }
}
