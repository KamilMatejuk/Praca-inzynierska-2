using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car {
    [RequireComponent(typeof(CarController))]
    public class CarUserControl : MonoBehaviour {
        private CarController m_Car; // the car controller we want to use
        public bool playableCar = false;


        private void Awake() {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate() {
            return;
            // pass the input to the car!
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
#if !MOBILE_INPUT
            float handbrake = Input.GetAxis("Jump");
            m_Car.Move(h, v, v, handbrake);
#else
            m_Car.Move(h, v, v, 0f);
#endif
        }
    }
}
