using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Parser {

    /// <summary>
    /// Parse string to Vector3
    /// </summary>
    /// <param name="sVector">Vector3 in string format</param>
    public static Vector3 Vector3Parse(string sVector) {
        if (sVector.StartsWith("(") && sVector.EndsWith(")")) {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }
        string[] sArray = sVector.Split(',');
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
        return result;
    }

    /// <summary>
    /// Parse string to Quaternion
    /// </summary>
    /// <param name="sQuaternion">Quaternion in string format</param>
    public static Quaternion QuaternionParse(string sQuaternion) {
        if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")")) {
            sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
        }
        string[] sArray = sQuaternion.Split(',');
        Quaternion result = new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));
        return result;
    }

    /// <summary>
    /// Parse string to OrientedPoint
    /// </summary>
    /// <param name="sOP">OrientedPoint in string format</param>
    public static OrientedPoint OrientedPointParse(string sOP) {
        if (sOP.StartsWith("OP(pos: ") && sOP.EndsWith(")")) {
            int rotIndex = sOP.IndexOf(", rot: ");
            string sPosition = sOP.Substring(8, rotIndex - 8);
            string sRotation = sOP.Substring(rotIndex + 7, sOP.Length - sPosition.Length - 16);
            return new OrientedPoint(Vector3Parse(sPosition), QuaternionParse(sRotation));
        }
        return new OrientedPoint(Vector3.zero);
    }

    /// <summary>
    /// Parse Color to string
    /// </summary>
    /// <param name="color">Color</param>
    public static string ColorToString(Color color) {
        int r = (int)color.r*255;
        int g = (int)color.g*255;
        int b = (int)color.b*255;
        return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }

    /// <summary>
    /// Parse string to Color
    /// </summary>
    /// <param name="sColor">Color in string format</param>
    public static Color ColorParse(string sColor) {
        Color color = Color.black;
        ColorUtility.TryParseHtmlString(sColor, out color);
        return color;
    }

    /// <summary>
    /// Convert Quaternion to Vector3, by removing 'w' component
    /// </summary>
    /// <param name="quaternion">Quaternion</param>
    public static Vector3 ToVector3(Quaternion quaternion) {
        return new Vector3(quaternion.x, quaternion.y, quaternion.z);
    }

}
