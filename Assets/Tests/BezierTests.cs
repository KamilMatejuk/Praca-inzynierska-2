using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BezierTests : MonoBehaviour {

    Loop controlPoints;
    public GameObject sphere;
    
    void OnValidate() {
        UnityEngine.Random.InitState(10);
        controlPoints = new Loop(Vector3.zero, ScriptableObject.CreateInstance<TerrainGenData>());
    }

    void OnDrawGizmos() {
        #if UNITY_EDITOR
            if (controlPoints != null) {
                for (int i = 0; i < controlPoints.NumberOfSegments; i++) {
                    List<OrientedPoint> bezierPoints = controlPoints.GetSegmentBezierPoints(i);
                    Handles.DrawBezier(bezierPoints[0].position,
                                        bezierPoints[3].position,
                                        bezierPoints[1].position,
                                        bezierPoints[2].position, Color.red, null, 2f);
                    Gizmos.DrawSphere(bezierPoints[0].position, 2f);
                }
                if (sphere != null) {
                    Vector3 pos = sphere.transform.position;
                    Vector3 nearest = controlPoints.GetNearestBezierPoint(pos).position;
                    Handles.DrawBezier(pos, nearest, pos, nearest, Color.red, null, 2f);
                }
            }
        #endif
    }
}
