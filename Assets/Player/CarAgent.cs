using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityStandardAssets.CrossPlatformInput;


namespace RacingGameBot.Play {
    public class CarAgent : Agent {

        [SerializeField] public bool showGizmos = false;
        [SerializeField] public bool playMode = false;
        [SerializeField] public bool playableCar = false;

        private Terrains.TerrainLoader terrainLoader;
        private GameObject terrainGO;
        private int numberOfAllCheckpoints = 0;
        private int numberOfCheckpointsAlreadyHitInThisEpisode;
        private bool startingPositionRotationSet = false;
        private Vector3 startingPosition;
        private Quaternion startingRotation;
        private float prevForwardValue;
        private float prevSidewaysValue;
        private StatsRecorder statsRecorder;
        private Vector3 lastPosition;
        private Vector3 lastFixedPosition;
        private float startTime;
        private List<float> lastDistances;


        /// <summary>
        /// Load data from terrain on start
        /// </summary>
        void Start() {
            GameObject terrain = Utils.Objects.GetParentWithName(gameObject, "Terrain");
            terrainGO = Utils.Objects.GetChildWithName(terrain, "Loaded Terrain");
            terrainLoader = terrain.GetComponent<Terrains.TerrainLoader>();
            statsRecorder = Academy.Instance.StatsRecorder;
            numberOfAllCheckpoints = terrainLoader.checkpoints.Count;
            startTime = Time.realtimeSinceStartup;
            lastDistances = new List<float>();
        }

        /// <summary>
        /// Each frame check if car is stuck
        /// </summary>
        void FixedUpdate() {
            if (!playableCar) {
                float fixedFrameDistance = Vector3.Distance(lastFixedPosition, transform.position);
                lastFixedPosition = transform.position;
                lastDistances.Add(fixedFrameDistance);
                if (lastDistances.Count > 60) {
                    lastDistances.RemoveAt(0);
                    if (lastDistances.Sum() < 0.3f && terrainLoader != null && terrainLoader.controlPoints != null) {
                        Terrains.OrientedPoint nearestPosRot = terrainLoader.controlPoints.GetNearestBezierPoint(transform.position);
                        Vector3 pos = nearestPosRot.position;
                        pos.y = terrainLoader.controlPoints.GetHeight(
                            pos.x - terrainGO.transform.position.x,
                            pos.z - terrainGO.transform.position.z
                        ) * Utils.Variables.TERRAIN_HEIGHT;
                        pos.y += 1f;
                        transform.position = pos;
                        transform.rotation = nearestPosRot.rotation;
                        lastDistances = new List<float>();
                    }
                }
            }
        }

        /// <summary>
        /// Set car starting position in first episode,
        /// reset it at the beginning of each episode
        /// </summary>
        public override void OnEpisodeBegin() {
            if (startingPositionRotationSet) {
                transform.position = startingPosition;
                transform.rotation = startingRotation;
                lastPosition = transform.position;
            } else {
                startingPosition = transform.position;
                startingRotation = transform.rotation;
                startingPositionRotationSet = true;
                lastPosition = transform.position;
            }
            SetReward(0);
            numberOfCheckpointsAlreadyHitInThisEpisode = 0;
        }

        /// <summary>
        /// Collect observation data from environment
        /// * distance to road center
        /// * current input axis positions for forward and sideways movement
        /// * slope of terrain
        /// * velocity
        /// </summary>
        /// <param name="sensor">Car sensor</param>
        public override void CollectObservations(VectorSensor sensor) {
            float[] distanceAndTangent = GetDistanceToRoadCenterAndAngleToTangent();
            sensor.AddObservation(distanceAndTangent[0]); // distance to center of road
            sensor.AddObservation(CrossPlatformInputManager.GetAxis("Vertical")); // axes forward
            sensor.AddObservation(CrossPlatformInputManager.GetAxis("Horizontal")); // axes sideways
                                                                                    // slope
            float slopeForwardRadians = Mathf.Atan2(transform.forward.y, Mathf.Sqrt(Mathf.Pow(transform.forward.x, 2) + Mathf.Pow(transform.forward.z, 2)));
            float slopeForward = slopeForwardRadians / Mathf.PI;
            sensor.AddObservation(slopeForward);
            // velocity
            float velocityAngle = GetAngleXZBetweenForwardAndPoint(transform.position + GetComponent<Rigidbody>().velocity);
            float velocity = GetComponent<Rigidbody>().velocity.magnitude * (Mathf.Abs(velocityAngle) > 0.5 ? -1 : 1);
            sensor.AddObservation(velocity);
        }

        /// <summary>
        /// Move car based on inference results and assign rewards
        /// </summary>
        /// <param name="actions">Received actions</param>
        public override void OnActionReceived(ActionBuffers actions) {
            // string a = "";
            // foreach (var item in actions.ContinuousActions) a += $"{item} ";
            // foreach (var item in actions.DiscreteActions) a += $"{item} ";
            // Debug.Log($"{gameObject.name} actions {a}");
            MoveCarBasedOnDiscreteActions(actions.DiscreteActions[0],
                                            actions.DiscreteActions[1]);
            float[] distanceAndTangent = GetDistanceToRoadCenterAndAngleToTangent();
            float distanceToRoadCenter = distanceAndTangent[0]; // distance to center of road
            float angleToTangent = distanceAndTangent[1]; // angle between forward and tangent

            if (!playMode && distanceToRoadCenter > 2) {
                if (statsRecorder == null) {
                    statsRecorder = Academy.Instance.StatsRecorder;
                }
                statsRecorder.Add("VisitedCheckpoints", numberOfCheckpointsAlreadyHitInThisEpisode);
                EndEpisode();
                ReloadCar();
            }

            AddReward((0.5f - distanceToRoadCenter) * 0.1f); // positive for distanceToRoadCenter < 0.5
            AddReward((0.05f - Mathf.Abs(angleToTangent)) * 1f); // positive for angleToTangent < 0.05
            float frameDistance = Vector3.Distance(lastPosition, transform.position);
            AddReward((frameDistance - 0.1f) * 1f); // positive for distance bigger then 0.1 ~ 0.05
            AddReward(0.01f); // the longer the better
            lastPosition = transform.position;
        }

        /// <summary>
        /// Handle car collisions with environment objects,
        /// like other cars, checkpoints, finish, etc.
        /// </summary>
        /// <param name="other">Object with which car collided</param>
        public void OnTriggerEnter(Collider other) {
            if (other.name.Contains("Checkpoint ")) {
                int carNumber = int.Parse(gameObject.name.Split(' ')[1]);
                int checkpointNumber = int.Parse(other.name.Split(' ')[1]);
                if (numberOfAllCheckpoints != 0) {
                    if (checkpointNumber == numberOfCheckpointsAlreadyHitInThisEpisode + 1) {
                        numberOfCheckpointsAlreadyHitInThisEpisode++;
                        Debug.Log("Hit next checkpoint");
                        // if (playMode) other.enabled = false;
                        AddReward(1f);
                    } else if (numberOfCheckpointsAlreadyHitInThisEpisode != 0) {
                        if (playMode && playableCar && checkpointNumber > numberOfCheckpointsAlreadyHitInThisEpisode) {
                            GameObject.Find("UiHelper").GetComponent<Menu.GameMenu>().ShowMessage();
                        }
                        if (Mathf.Abs(checkpointNumber - numberOfCheckpointsAlreadyHitInThisEpisode) < 2 ||
                              numberOfAllCheckpoints - Mathf.Abs(checkpointNumber - numberOfCheckpointsAlreadyHitInThisEpisode) < 2) {
                            Debug.Log("Hit neighbor checkpoint out of order");
                            AddReward(-0.1f);
                        } else {
                            Debug.Log("Hit checkpoint out of order");
                            AddReward(-1f);
                            goto endEpisode;
                        }
                    }
                    if (numberOfCheckpointsAlreadyHitInThisEpisode == numberOfAllCheckpoints) {
                        Debug.Log("No more checkpoints");
                        AddReward(numberOfAllCheckpoints);
                        // try {
                        float endTime = Time.realtimeSinceStartup;
                        GameObject.Find("UiHelper").GetComponent<Menu.EndMenu>().Show(playableCar, endTime - startTime);
                        // } catch { }
                        goto endEpisode;
                    }
                }
                goto continueEpisode;
            } else if (other.name.Contains("Border") || other.name.Contains("BorderRoad")) {
                Debug.Log("Hit border");
                AddReward(-0.1f);
                goto endEpisode;
            } else if (other.name != "Loaded Terrain") {
                Debug.Log("Hit something other: " + other.name);
                AddReward(-0.1f);
                goto continueEpisode;
            }
        continueEpisode:
            return;
        endEpisode:
            if (!playMode) {
                if (statsRecorder == null) {
                    statsRecorder = Academy.Instance.StatsRecorder;
                }
                statsRecorder.Add("VisitedCheckpoints", numberOfCheckpointsAlreadyHitInThisEpisode);
                EndEpisode();
                ReloadCar();
            }
        }

        /// <summary>
        /// Read user inputs for manual steering
        /// </summary>
        /// <param name="actionsOut">Car actions</param>
        public override void Heuristic(in ActionBuffers actionsOut) {
            ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
            discreteActions[0] = 1 + Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
            discreteActions[1] = 1 + Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        }

        /// <summary>
        /// Move car based on received actions
        /// </summary>
        /// <param name="forwardMovement">Which way to move forward/backward</param>
        /// <param name="sidewaysMovement">Which way to move sideways</param>
        private void MoveCarBasedOnDiscreteActions(int forwardMovement, int sidewaysMovement) {
            float scaleV = 5f;
            float scaleH = 5f;
            /*
            0 - deceleration
            1 - nothing
            2 - acceleration
            */
            float forwardValue = (float)(forwardMovement - 1) / scaleV;
            /*
            0 - steer left
            1 - nothing
            2 - steer right
            */
            float sidewaysValue = (float)(sidewaysMovement - 1) / scaleH;

            float currForwardMovement = CrossPlatformInputManager.GetAxis("Vertical");
            float currSidewaysMovement = CrossPlatformInputManager.GetAxis("Horizontal");
            float nextForwardMovement = 0f;
            float nextSidewaysMovement = 0f;

            if (forwardValue == 0) { // no movement
                if (currForwardMovement > 0) nextForwardMovement = Mathf.Clamp(currForwardMovement - 0.1f, -1, 1);
                if (currForwardMovement < 0) nextForwardMovement = Mathf.Clamp(currForwardMovement + 0.1f, -1, 1);
                if (Mathf.Abs(nextForwardMovement) < 0.2f) nextForwardMovement = 0;
            } else if (forwardValue * prevForwardValue < 0) { // switch direction
                nextForwardMovement = Mathf.Clamp(forwardValue, -1, 1);
                if (nextForwardMovement > 0) nextForwardMovement = Mathf.Clamp(nextForwardMovement * 4, -1, 1);
                else GetComponent<Rigidbody>().velocity /= 2f;
            } else { nextForwardMovement = Mathf.Clamp(currForwardMovement + forwardValue, -1, 1); }                    // same direction

            if (sidewaysValue == 0) { // no movement
                if (currSidewaysMovement > 0) nextSidewaysMovement = Mathf.Clamp(currSidewaysMovement - 0.1f, -1, 1);
                if (currSidewaysMovement < 0) nextSidewaysMovement = Mathf.Clamp(currSidewaysMovement + 0.1f, -1, 1);
                if (Mathf.Abs(nextSidewaysMovement) < 0.2f) nextSidewaysMovement = 0;
            } else if (sidewaysValue * prevSidewaysValue < 0) { nextSidewaysMovement = Mathf.Clamp(sidewaysValue, -1, 1); } // switch direction
              else { nextSidewaysMovement = Mathf.Clamp(currSidewaysMovement + sidewaysValue, -1, 1); }                    // same direction

            prevForwardValue = forwardValue;
            prevSidewaysValue = sidewaysValue;

            if (nextForwardMovement < 0) {
                nextForwardMovement /= 3;
            }
            GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(nextSidewaysMovement, nextForwardMovement, nextForwardMovement, 0f);

            // check if flipped
            Vector3 environTop = Vector3.up;
            Vector3 carTop = transform.up;
            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(environTop.normalized, carTop.normalized), -1, 1)) / Mathf.PI;
            if (angle > 0.4f) {
                if (terrainLoader != null && terrainLoader.controlPoints != null) {
                    Terrains.OrientedPoint nearestBezierPosDst = terrainLoader.controlPoints.GetNearestBezierPoint(transform.position);
                    transform.rotation = nearestBezierPosDst.rotation;
                }
            }
        }

        /// <summary>
        /// Reload car, resetting it to starting position
        /// </summary>
        private void ReloadCar() {
            if (startingPositionRotationSet && terrainLoader != null && terrainGO != null) {
                transform.position = startingPosition;
                transform.rotation = startingRotation;
            }
        }

        /// <summary>
        /// Get angle between car forward direction and direction to some point
        /// </summary>
        /// <param name="point">Other direction end</param>
        /// <returns>Angle normalized in range [-1, 1]</returns>
        private float GetAngleXZBetweenForwardAndPoint(Vector3 point) {
            Vector3 forwardXZ = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 pointXZ = new Vector3(point.x, 0, point.z);
            Vector3 positionXZ = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 directionToPointXZ = pointXZ - positionXZ;
            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(forwardXZ.normalized, directionToPointXZ.normalized), -1, 1));
            float crossY = Vector3.Cross(forwardXZ, directionToPointXZ).y;
            float crossYSign = crossY >= 0 ? 1f : -1f;
            float radianANgle = angle * crossYSign; // angle in radians [-pi, pi]
            return radianANgle / Mathf.PI; // angle normalized [-1, 1]
        }

        /// <summary>
        /// Find nearest point of the road.
        /// Return distance to it and angle between forward and road tangent in this point
        /// </summary>
        /// <returns>List of distance and angle</returns>
        private float[] GetDistanceToRoadCenterAndAngleToTangent() {
            float distanceToRoadCenter = 0f;
            float angleToTangent = 0f;
            if (terrainLoader != null && terrainLoader.controlPoints != null) {
                Terrains.OrientedPoint nearestBezierPosDst = terrainLoader.controlPoints.GetNearestBezierPoint(transform.position);
                distanceToRoadCenter = nearestBezierPosDst.other / terrainLoader.terrainGenData.roadWidth;
                Vector3 nearestBezierPos = nearestBezierPosDst.position;
                Quaternion nearestBezierRot = nearestBezierPosDst.rotation;
                angleToTangent = GetAngleXZBetweenForwardAndPoint(transform.position + nearestBezierRot * Vector3.forward);
                if (showGizmos) {
                    Debug.DrawRay(transform.position, nearestBezierRot * Vector3.forward * 10, Color.red);
                    Debug.DrawRay(transform.position, transform.forward * 10, Color.green);
                    Debug.DrawLine(transform.position, nearestBezierPos, Color.blue);
                }
            }
            return new float[] { distanceToRoadCenter, angleToTangent };
        }
    }
}