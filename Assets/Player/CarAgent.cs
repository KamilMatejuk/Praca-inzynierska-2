using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using UnityStandardAssets.CrossPlatformInput;

public class CarAgent : Agent {

    private TerrainLoader terrainLoader;
    private GameObject terrainGO;
    private int numberOfAllCheckpoints = 0;
    private int numberOfStartingCheckpoint = 0;
    private int numberOfCheckpointsAlreadyHitInThisEpisode;
    private bool startingPositionRotationSet = false;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private string logFileName;
    private float prevForwardValue;
    private float prevSidewaysValue;
    private StatsRecorder statsRecorder;
    private Vector3 lastPosition;

    [SerializeField]
    public bool showGizmos = false;

    [SerializeField]
    public bool playMode = false;

    void Start() {
        string timeString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        logFileName = "training-" + timeString + ".log";
        GameObject terrain = Objects.GetParentWithName(gameObject, "Terrain");
        terrainGO = Objects.GetChildWithName(terrain, "Loaded Terrain");
        terrainLoader = terrain.GetComponent<TerrainLoader>();
        statsRecorder = Academy.Instance.StatsRecorder;
        numberOfAllCheckpoints = terrainLoader.checkpoints.Count;
        float nearsetCheckpointDistance = float.PositiveInfinity;
        foreach (GameObject c in terrainLoader.checkpoints) {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < nearsetCheckpointDistance) {
                nearsetCheckpointDistance = d;
                numberOfStartingCheckpoint = int.Parse(c.name.Split(' ')[1]) - numberOfAllCheckpoints;
            }
        }
        // SetModel("in5-out1-f", Ai.LoadModel1("Assets/Resources/brain2.onnx"));
        // SetModel("in5-out1-f", Ai.LoadModel2("Assets/Resources/brain2.onnx"));
    }

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

    public override void OnActionReceived(ActionBuffers actions) {
        // string a = "";
        // foreach (var item in actions.ContinuousActions) a += item + " ";
        // foreach (var item in actions.DiscreteActions) a += item + " ";
        // Debug.Log("actions " + a);
        MoveCarBasedOnDiscreteActions(actions.DiscreteActions[0],
                                        actions.DiscreteActions[1]);
        float[] distanceAndTangent = GetDistanceToRoadCenterAndAngleToTangent();
        float distanceToRoadCenter = distanceAndTangent[0]; // distance to center of road
        float angleToTangent = distanceAndTangent[1]; // angle between forward and tangent

        if (distanceToRoadCenter > 2) {
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
        // Debug.Log(Logging.GetParamString<float>(distanceToRoadCenter, angleToTangent, frameDistance));
    }

    public void OnTriggerEnter(Collider other) {
        if (other.name.Contains("Checkpoint ")) {
            if (numberOfAllCheckpoints != 0) {
                int checkpointNumber = int.Parse(other.name.Split(' ')[1]);
                if (checkpointNumber == numberOfCheckpointsAlreadyHitInThisEpisode + 1) {
                    numberOfCheckpointsAlreadyHitInThisEpisode++;
                    Debug.Log("Hit next checkpoint");
                    AddReward(1f);
                } else if (Mathf.Abs(checkpointNumber - numberOfCheckpointsAlreadyHitInThisEpisode) < 2 ||
                          numberOfAllCheckpoints - Mathf.Abs(checkpointNumber - numberOfCheckpointsAlreadyHitInThisEpisode) < 2) {
                    Debug.Log("Hit neighbor checkpoint out of order");
                    AddReward(-0.1f);
                } else {
                    Debug.Log("Hit checkpoint out of order");
                    AddReward(-1f);
                    goto endEpisode;
                }
                if (numberOfCheckpointsAlreadyHitInThisEpisode == numberOfAllCheckpoints) {
                    Debug.Log("No more checkpoints");
                    AddReward(numberOfAllCheckpoints);
                    try {
                        int carNumber = int.Parse(gameObject.name.Split(' ')[1]);
                        GameObject.Find("UiHelper").GetComponent<EndMenu>().Show(carNumber == 1);
                    } catch { }
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
        if (statsRecorder == null) {
            statsRecorder = Academy.Instance.StatsRecorder;
        }
        statsRecorder.Add("VisitedCheckpoints", numberOfCheckpointsAlreadyHitInThisEpisode);
        EndEpisode();
        ReloadCar();
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 1 + Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        discreteActions[1] = 1 + Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
    }

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

        CrossPlatformInputManager.SetAxis("Vertical", nextForwardMovement);
        CrossPlatformInputManager.SetAxis("Horizontal", nextSidewaysMovement);
        // Debug.Log("axis " + nextForwardMovement + " " + nextSidewaysMovement + " " + GetComponent<Rigidbody>().velocity.magnitude);
    }

    private void ReloadCar() {
        if (startingPositionRotationSet && terrainLoader != null && terrainGO != null) {
            OrientedPoint posrot = new OrientedPoint(startingPosition, startingRotation);
            GameObject car = Objects.PutObject("SportCar", "car", gameObject.name, posrot);
            car.transform.parent = gameObject.transform.parent;
            car.transform.localScale = gameObject.transform.localScale;

            BehaviorParameters behaviorParameters = car.AddComponent<BehaviorParameters>();
            behaviorParameters.BehaviorName = "in5-out1-f";
            behaviorParameters.BrainParameters.VectorObservationSize = 5; // number of input values
            behaviorParameters.BrainParameters.NumStackedVectorObservations = 1;
            behaviorParameters.BrainParameters.ActionSpec = new ActionSpec(0, new int[] { 3, 3 }); // continuous outputs, descrete outputs
            // behaviorParameters.Model = Ai.LoadModel1("Assets/Resources/brain.onnx");

            RayPerceptionSensorComponent3D rs = car.AddComponent<RayPerceptionSensorComponent3D>();
            rs.DetectableTags = new List<string>() { "car", "border", "checkpoint" };
            rs.RaysPerDirection = 4;
            rs.MaxRayDegrees = 80;
            rs.RayLength = terrainLoader.terrainGenData.roadWidth * 5;
            rs.StartVerticalOffset = 0.5f;
            rs.EndVerticalOffset = 0.5f;
            car.AddComponent<CarAgent>();
            car.GetComponent<CarAgent>().showGizmos = showGizmos;
            car.AddComponent<DecisionRequester>();
            Destroy(gameObject);
        }
    }

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

    private float[] GetDistanceToRoadCenterAndAngleToTangent() {
        float distanceToRoadCenter = 0f;
        float angleToTangent = 0f;
        if (terrainLoader != null && terrainLoader.controlPoints != null) {
            OrientedPoint nearestBezierPosDst = terrainLoader.controlPoints.GetNearestBezierPoint(transform.position);
            distanceToRoadCenter = nearestBezierPosDst.other / terrainLoader.terrainGenData.roadWidth;
            Vector3 nearestBezierPos = nearestBezierPosDst.position;
            Quaternion nearestBezierRot = nearestBezierPosDst.rotation;
            angleToTangent = GetAngleXZBetweenForwardAndPoint(transform.position + nearestBezierRot * Vector3.forward);
            if (showGizmos) {
                Debug.DrawRay(transform.position, nearestBezierRot * Vector3.forward * 10, Color.green);
                Debug.DrawRay(transform.position, transform.forward * 10, Color.green);
                Debug.DrawLine(transform.position, nearestBezierPos, Color.green);
            }
        }
        return new float[] { distanceToRoadCenter, angleToTangent };
    }
}
