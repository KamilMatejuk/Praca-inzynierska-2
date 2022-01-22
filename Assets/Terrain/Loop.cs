using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Loop {

    [SerializeField, HideInInspector] public float minDistanceBetweenPoints = 0.1f;
    [SerializeField, HideInInspector] public List<OrientedPoint> points;
    [SerializeField, HideInInspector] public TerrainGenData terrainGenData;

    /// <summary>
    /// Select height for each point
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="z">Z position</param>
    /// <param name="detailsMain">How much wavy is main terrain</param>
    /// <param name="detailsMinor">How much wavy are bigger details</param>
    /// <param name="detailsTiny">How much wavy are smaller details</param>
    /// <returns></returns>
    // public static float PerlinNoise(float x, float z, float detailsMain, float detailsMinor, float detailsTiny) {
    //     return detailsMain * Mathf.PerlinNoise(10f + x/2, 10f + z/2) +
    //            detailsMinor * Mathf.PerlinNoise(x, z) +
    //            detailsTiny * Mathf.PerlinNoise(x*2, z*2);
    // }

    public float GetHeight(float x, float y) {
        // return PerlinNoise(
        //     (x + terrainGenData.offsetX) * terrainGenData.scale,
        //     (y + terrainGenData.offsetY) * terrainGenData.scale,
        //     terrainGenData.terrainDetailsMain,
        //     terrainGenData.terrainDetailsMinor,
        //     terrainGenData.terrainDetailsTiny
        // );
        x += terrainGenData.offsetX;
        y += terrainGenData.offsetY;
        x /= 100;
        y /= 100;
        return terrainGenData.terrainDetailsMain * Mathf.PerlinNoise(
                10f + x * terrainGenData.terrainScaleMain / 2,
                10f + y * terrainGenData.terrainScaleMain / 2
            ) + terrainGenData.terrainDetailsMinor * Mathf.PerlinNoise(
                x * terrainGenData.terrainScaleMinor,
                y * terrainGenData.terrainScaleMinor
            ) + terrainGenData.terrainDetailsTiny * Mathf.PerlinNoise(
                x * terrainGenData.terrainScaleTiny * 2,
                y * terrainGenData.terrainScaleTiny * 2
            );
    }

    /// <summary>
    /// Create simple circular loop of size (1, 1)
    /// </summary>
    public Loop() {
        points = GenerateLoopBezier(new List<Vector3> { Vector3.left, Vector3.forward, Vector3.right, Vector3.back });
        PostprocessLoop();
    }

    /// <summary>
    /// Create loop from received data
    /// </summary>
    public Loop(Vector3 parentPosition, List<OrientedPoint> _points, TerrainGenData _terrainGenData) {
        terrainGenData = _terrainGenData;
        points = _points;
        for (int i = 0; i < points.Count; i++) {
            points[i].position += parentPosition;
        }
    }

    /// <summary>
    /// Create loop based of additional params
    /// </summary>
    /// <param name="_terrainGenData">Other params</param>
    public Loop(Vector3 parentPosition, TerrainGenData _terrainGenData) {
        terrainGenData = _terrainGenData;
        points = GenerateLoopBezier(SelectRandomPoints());
        // scale to size
        float estimatedLength = GetEstimatedLength();
        float scale = Mathf.Sqrt(terrainGenData.roadLength / estimatedLength);
        for (int i = 0; i < points.Count; i++) {
            points[i].position *= scale;
        }
        PostprocessLoop();
        for (int i = 0; i < points.Count; i++) {
            points[i].position += parentPosition;
        }
    }

    private List<Vector3> SelectRandomPoints() {
        List<Vector3> positions = new List<Vector3>();
        // generate positions
        int stuckCounter = 0;
        int maxStuckCounter = 10;
        while (positions.Count < terrainGenData.numberOfSegments && stuckCounter < maxStuckCounter) {
            float x = (Random.value - 0.5f) * 2f;
            float z = (Random.value - 0.5f) * 2f;
            Vector3 pos = new Vector3(x, 0f, z);
            bool tooClose = false;
            for (int i = 0; i < positions.Count; i++) {
                if (Vector3.Distance(pos, positions[i]) < minDistanceBetweenPoints) {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose) {
                positions.Add(pos);
                stuckCounter = 0;
            } else {
                stuckCounter++;
            }
        }
        if (stuckCounter >= maxStuckCounter) {
            Debug.Log("Finished after being stuck for " + stuckCounter.ToString() + " iterations. Generated " + positions.Count + " positions.");
        }
        // order positions (sort by angle between point and forward)
        float __angleBetweenPointAndRay(Vector3 p, Ray r) {
            Vector3 fromCenterToPoint = (r.origin - p).normalized;
            Vector3 fromCenterRay = r.direction.normalized;
            float angle = Mathf.Acos(Vector3.Dot(fromCenterRay, fromCenterToPoint));
            float crossY = Vector3.Cross(fromCenterRay, fromCenterToPoint).y;
            float crossYSign = crossY >= 0 ? 1f : -1f;
            return angle * crossYSign;
        }
        Ray ray = new Ray(Vector3.zero, Vector3.forward);
        positions.Sort((p1, p2) => __angleBetweenPointAndRay(p1, ray).CompareTo(__angleBetweenPointAndRay(p2, ray)));
        return positions;
    }

    private void PostprocessLoop() {
        // get borders
        List<float> borders = GetBorder();
        // calculate size and padding
        float size = Mathf.Max(borders[1] - borders[0], borders[3] - borders[2]);
        float padding = 1f;
        if (terrainGenData != null) {
            padding = terrainGenData.paddingPercent * Variables.TERRAIN_SIZE;
        }
        float minX = borders[0] - padding;
        float maxX = borders[1] + padding;
        float minZ = borders[2] - padding;
        float maxZ = borders[3] + padding;
        size = Mathf.Max(maxX - minX, maxZ - minZ);

        // scale to fit square Variables.TERRAIN_SIZE x Variables.TERRAIN_SIZE
        float scaleX = (Variables.TERRAIN_SIZE - 2 * padding) / (borders[1] - borders[0]);
        float scaleZ = (Variables.TERRAIN_SIZE - 2 * padding) / (borders[3] - borders[2]);
        Vector3 scale = new Vector3(scaleX, 1f, scaleZ);
        for (int i = 0; i < points.Count; i++) {
            points[i].position = Vector3.Scale(points[i].position, scale);
        }
        // // scale to fit square of nearest pow 2
        // mapSize = Mathf.CeilToInt(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(size) / Mathf.Log(2))));
        // float scaleX = (mapSize - 2 * padding) / (borders[1] - borders[0]);
        // float scaleZ = (mapSize - 2 * padding) / (borders[3] - borders[2]);
        // Vector3 scale = new Vector3(scaleX, 1f, scaleZ);
        // for (int i = 0; i < points.Count; i++) {
        //     points[i].position = Vector3.Scale(points[i].position, scale);
        // }
        
        // update borders
        borders = GetBorder();
        minX = borders[0] - padding;
        maxX = borders[1] + padding;
        minZ = borders[2] - padding;
        maxZ = borders[3] + padding;
        // move start to (- Variables.TERRAIN_SIZE / 2, - Variables.TERRAIN_SIZE / 2)
        for (int i = 0; i < points.Count; i++) {
            points[i].position -= new Vector3(minX - (Variables.TERRAIN_SIZE / 2), 0, minZ - (Variables.TERRAIN_SIZE / 2));
        }
        // update heights
        if (terrainGenData != null) {
            for (int i = 0; i < points.Count; i++) {
                points[i].position.y = GetHeight(points[i].position.x, points[i].position.z) * Variables.TERRAIN_SIZE * 0.2f;
            }
        }
        // update rotations
        for (int i = 0; i < NumberOfSegments; i++) {
            List<OrientedPoint> bezierPoints = GetSegmentBezierPoints(i);
            OrientedPoint op = Bezier.GetBezierOrientedPoint(bezierPoints[0].position,
                                                             bezierPoints[1].position,
                                                             bezierPoints[2].position,
                                                             bezierPoints[3].position, 0f);
            points[LoopIndex(3 * i + 1)].rotation = op.rotation;
        }
    }

    public int NumberOfSegments {
        get {
            return points.Count / 3;
        }
    }

    /// <summary>
    /// Get starting point of each segment
    /// </summary>
    /// <param name="i">Number of segment from created loop</param>
    /// <returns>Starting point</returns>
    public OrientedPoint GetCentralPoint(int i) {
        OrientedPoint p = points[LoopIndex(3 * i + 1)];
        return new OrientedPoint(new Vector3(p.position.x, p.position.y, p.position.z),
                                 new Quaternion(p.rotation.x, p.rotation.y, p.rotation.z, p.rotation.w));
        // return points[LoopIndex(3 * i + 1)];
    }

    /// <summary>
    /// Get bezier points for ith segment
    /// </summary>
    /// <param name="i">Number of segment from created loop</param>
    /// <returns>List of 2 end points and 2 controls</returns>
    public List<OrientedPoint> GetSegmentBezierPoints(int i) {
        return new List<OrientedPoint>(){
            points[LoopIndex(3 * i + 1)], // start
            points[LoopIndex(3 * i + 2)], // control 1
            points[LoopIndex(3 * i + 3)], // control 2
            points[LoopIndex(3 * i + 4)], // end
        };
    }
    /// <summary>
    /// Calculate what is the nearest point on created loop
    /// </summary>
    /// <param name="target">From what point to calculate distance</param>
    /// <returns>Quaternion where xyz is point and w is distance</returns>
    public OrientedPoint GetNearestBezierPoint(Vector3 target) {
        OrientedPoint closestPointAndDistance = new OrientedPoint(Vector3.zero, Quaternion.identity, Mathf.Infinity);
        for (int i = 0; i < NumberOfSegments; i++) {
            List<OrientedPoint> bezierPoints = GetSegmentBezierPoints(i);
            OrientedPoint closestPointAndDistancePerSegment = Bezier.GetNearestBezierPoint(bezierPoints[0].position,
                                                                              bezierPoints[1].position,
                                                                              bezierPoints[2].position,
                                                                              bezierPoints[3].position,
                                                                              target);
            if (closestPointAndDistancePerSegment.other < closestPointAndDistance.other) {
                closestPointAndDistance = closestPointAndDistancePerSegment;
            }
        }
        return closestPointAndDistance;
    }

    /// <summary>
    /// Generate bezier curves into loop, just from start positions
    /// </summary>
    /// <param name="positions">Start positions for each segment</param>
    /// <returns>All points required for bezier loop (all ends and all controls)</returns>
    private List<OrientedPoint> GenerateLoopBezier(List<Vector3> positions) {
        List<OrientedPoint> points = new List<OrientedPoint>();
        // create control points
        for (int i = 0; i < positions.Count; i++) {
            int iPrev = (i - 1 + positions.Count) % positions.Count;
            int iNext = (i + 1 + positions.Count) % positions.Count;
            Vector3 pos = positions[i];
            Vector3 posPrev = positions[iPrev];
            Vector3 posNext = positions[iNext];
            Vector3 offsetPrev = posPrev - pos;
            Vector3 offsetNext = posNext - pos;
            Vector3 dir = (offsetNext.normalized - offsetPrev.normalized).normalized;
            points.Add(new OrientedPoint(pos - dir * offsetPrev.magnitude * 0.5f));
            points.Add(new OrientedPoint(pos));
            points.Add(new OrientedPoint(pos + dir * offsetNext.magnitude * 0.5f));
        }
        return points;
    }

    /// <summary>
    /// Find minimal and maximal values of created loop
    /// </summary>
    /// <returns>minX, maxX, minZ, maxZ</returns>
    public List<float> GetBorder() {
        float minX = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float minZ = Mathf.Infinity;
        float maxZ = -Mathf.Infinity;
        // go over whole road
        for (int i = 0; i < NumberOfSegments; i++) {
            List<OrientedPoint> bezierPoints = GetSegmentBezierPoints(i);
            for (float t = 0; t < 1; t += 0.1f) {
                Vector3 p = Bezier.EvaluateCubic(bezierPoints[0].position,
                                                 bezierPoints[1].position,
                                                 bezierPoints[2].position,
                                                 bezierPoints[3].position, t);
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.z);
                maxZ = Mathf.Max(maxZ, p.z);
            }
        }
        return new List<float>(){minX, maxX, minZ, maxZ};
    }

    /// <summary>
    /// Make sure index is always in the boundaries
    /// </summary>
    /// <param name="i">Index</param>
    /// <returns>Looped index</returns>
    public int LoopIndex(int i) {
        return (i + points.Count) % points.Count;
    }

    /// <summary>
    /// Make sure index is always in the boundaries of segments
    /// </summary>
    /// <param name="i">Index</param>
    /// <returns>Looped index</returns>
    public int LoopCentralIndex(int i) {
        return (i + NumberOfSegments) % NumberOfSegments;
    }

    public List<OrientedPoint> GetEquallySpacedPoints(int n) {
        if (n <= 0) {
            return new List<OrientedPoint>();
        }
        List<OrientedPoint> equallySpacedPoints = new List<OrientedPoint>();
        float spacing = GetEstimatedLength() / (float)n;
        int segmentIndex = 0;
        float distanceToNext = spacing;
        float movedInSegment = 0f;
        while (equallySpacedPoints.Count < n) {
            if (distanceToNext > GetEstimatedSegmentLength(segmentIndex) - movedInSegment) {
                distanceToNext -= GetEstimatedSegmentLength(segmentIndex) - movedInSegment;
                segmentIndex = LoopCentralIndex(segmentIndex + 1);
                movedInSegment = 0f;
            } else {
                float segmentLength = GetEstimatedSegmentLength(segmentIndex);
                float t = (movedInSegment + distanceToNext) / segmentLength;
                List<OrientedPoint> bezierPoints = GetSegmentBezierPoints(segmentIndex);
                equallySpacedPoints.Add(Bezier.GetBezierOrientedPoint(bezierPoints[0].position,
                                                                      bezierPoints[1].position,
                                                                      bezierPoints[2].position,
                                                                      bezierPoints[3].position, t));
                distanceToNext = spacing;
                movedInSegment = t * segmentLength;
            }
        }
        return equallySpacedPoints;
    }

    public void Translate(Vector3 pos) {
        for (int i = 0; i < points.Count; i++) {
            points[i].position += pos;
        }
    }

    /// <summary>
    /// Estimate length of created loop
    /// </summary>
    /// <returns>Length</returns>
    private float GetEstimatedLength() {
        float len = 0f;
        for (int i = 0; i < NumberOfSegments; i++) {
            len += GetEstimatedSegmentLength(i);
        }
        return len;
    }

    /// <summary>
    /// Estimate length of ith segment
    /// </summary>
    /// <param name="i">Number of segment from created loop</param>
    /// <returns>Length</returns>
    private float GetEstimatedSegmentLength(int i) {
        List<OrientedPoint> bezierPoints = GetSegmentBezierPoints(i);
        return Bezier.EvaluateCubicLength(bezierPoints[0].position,
                                          bezierPoints[1].position,
                                          bezierPoints[2].position,
                                          bezierPoints[3].position);
    }
}
