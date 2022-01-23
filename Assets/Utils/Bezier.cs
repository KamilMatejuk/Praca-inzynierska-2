using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier {

    /// <summary>
    /// Get point at t% along quadratic bezier curve
    /// </summary>
    /// <param name="a">Bezier curve start</param>
    /// <param name="b">Bezier curve control 1</param>
    /// <param name="c">Bezier curve control 2</param>
    /// <param name="t">Percent of curve distance</param>
    /// <returns>Point at t% along quadratic bezier curve</returns>
    public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t) {
        Vector3 p0 = Vector3.Lerp(a, b, t);
        Vector3 p1 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(p0, p1, t);
    }

    /// <summary>
    /// Get point at t% along cubic bezier curve
    /// </summary>
    /// <param name="a">Bezier curve start</param>
    /// <param name="b">Bezier curve control 1</param>
    /// <param name="c">Bezier curve control 2</param>
    /// <param name="d">Bezier curve end</param>
    /// <param name="t">Percent of curve distance</param>
    /// <returns>Point at t% along cubic bezier curve</returns>
    public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
        Vector3 p0 = EvaluateQuadratic(a, b, c, t);
        Vector3 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector3.Lerp(p0, p1, t);
    }

    /// <summary>
    /// Evaluate length of bezier curve
    /// </summary>
    /// <param name="a">Bezier curve start</param>
    /// <param name="b">Bezier curve control 1</param>
    /// <param name="c">Bezier curve control 2</param>
    /// <param name="d">Bezier curve end</param>
    /// <returns>Evaluated length</returns>
    public static float EvaluateCubicLength(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
        float controlNetLength = Vector3.Distance(a, b) + Vector3.Distance(b, c) + Vector3.Distance(c, d);
        float estimatedCurveLength = Vector3.Distance(a, d) + controlNetLength / 2f;
        return estimatedCurveLength;
    }

    /// <summary>
    /// Get oriented point at t% along bezier curve
    /// </summary>
    /// <param name="a">Bezier curve start</param>
    /// <param name="b">Bezier curve control 1</param>
    /// <param name="c">Bezier curve control 2</param>
    /// <param name="d">Bezier curve end</param>
    /// <param name="t">Percent of curve distance</param>
    /// <returns>Oriented point at t% along bezier curve</returns>
    public static OrientedPoint GetBezierOrientedPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
        Vector3 e = Vector3.Lerp(a, b, t);
        Vector3 f = Vector3.Lerp(b, c, t);
        Vector3 g = Vector3.Lerp(c, d, t);
        Vector3 h = Vector3.Lerp(e, f, t);
        Vector3 i = Vector3.Lerp(f, g, t);
        Vector3 pos = Vector3.Lerp(h, i, t);
        Vector3 tan = (i - h).normalized;
        return new OrientedPoint(pos, tan);
    }

    /// <summary>
    /// Calculate what is the nearest point on bezier cubic curve
    /// </summary>
    /// <param name="bezierA">Bezier curve start</param>
    /// <param name="bezierB">Bezier curve control 1</param>
    /// <param name="bezierC">Bezier curve control 2</param>
    /// <param name="bezierD">Bezier curve end</param>
    /// <param name="target">From what point to calculate distance</param>
    /// <returns>Quaternion where xyz is point and w is distance</returns>
    public static OrientedPoint GetNearestBezierPoint1(Vector3 bezierA, Vector3 bezierB, Vector3 bezierC, Vector3 bezierD, Vector3 target) {
        float stepsPerSegment = 100;
        float minDistance = Mathf.Infinity;
        OrientedPoint closestPoint = Bezier.GetBezierOrientedPoint(bezierA, bezierB, bezierC, bezierD, 0);
        float foundT = 0f;
        for (float j = 0; j < stepsPerSegment; j++) {
            float t = j * 1.0f / stepsPerSegment;
            OrientedPoint op = Bezier.GetBezierOrientedPoint(bezierA, bezierB, bezierC, bezierD, t);
            op.position.y = 0; // compare only in 2D, ignore height
            // float distance = Vector3.Distance(op.position, target);
            float distance = (op.position - target).sqrMagnitude;
            if (distance < minDistance) {
                minDistance = distance;
                closestPoint = op;
                foundT = t;
            }
        }
        return new OrientedPoint(closestPoint.position, closestPoint.rotation, Mathf.Sqrt(minDistance));
    }

    public static OrientedPoint GetNearestBezierPoint(Vector3 bezierA, Vector3 bezierB, Vector3 bezierC, Vector3 bezierD, Vector3 target) {
        // doesn't work
        // https://hal.inria.fr/file/index/docid/518379/filename/Xiao-DiaoChen2007c.pdf
        Vector2 a = new Vector2(bezierA.x, bezierA.z);
        Vector2 b = new Vector2(bezierB.x, bezierB.z);
        Vector2 c = new Vector2(bezierC.x, bezierC.z);
        Vector2 d = new Vector2(bezierD.x, bezierD.z);
        Vector2 tar = new Vector2(target.x, target.z);
        Vector2 B(float t) {
            return Mathf.Pow(1-t, 3)*a + 3*t*Mathf.Pow(1-t, 2)*b + 3*t*t*(1-t)*c + t*t*t*d;
        }
        Vector2 Bd(float t) {
            return t*t*(-3*a+9*b-9*c+3*d) + t*(6*a-12*b+6*c) + (-3*a+3*b);
            // return -3*Mathf.Pow(1-t, 2)*a + 3*Mathf.Pow(1-t, 2)*b - 6*t*(1-t)*b - 3*t*t*c + 6*t*(1-t)*c + 3*t*t*d; 
        }
        float Minimize(float t) {
            return Vector2.Dot((tar - B(t)).normalized, Bd(t).normalized) * (tar - B(t)).magnitude;
        }
        float bisection(float a, float b, float delta, float epsilon, float maxit) {
            if (maxit < 1) {
                return (a + b) / 2;
            }
            float u = Minimize(a);
            float v = Minimize(b);
            float e = (b - a)/2;
            float c = a + e;
            float w = Minimize(c);
            if (u*v < 0) {
                if (Mathf.Abs(e) < delta || Mathf.Abs(w) < epsilon) {
                    return c;
                }
                if (u*w < 0) {
                    return bisection(a, c, delta, epsilon, maxit - 1);
                } else {
                    return bisection(c, b, delta, epsilon, maxit - 1);
                }
            } else {
                float left = bisection(a, c, delta, epsilon, maxit/2);
                float right = bisection(c, b, delta, epsilon, maxit/2);
                if (Mathf.Abs(Minimize(left)) < Mathf.Abs(Minimize(right))) {
                    return left;
                } else {
                    return right;
                }
            }
        }
        float foundT = bisection(0, 1, 10e-5f, 10e-5f, 10);
        OrientedPoint closestPoint = Bezier.GetBezierOrientedPoint(bezierA, bezierB, bezierC, bezierD, foundT);
        Vector2 p = new Vector2(closestPoint.position.x, closestPoint.position.z);
        return new OrientedPoint(closestPoint.position, closestPoint.rotation, Vector2.Distance(tar, p));
    }

}