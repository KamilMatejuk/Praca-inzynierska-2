using UnityEngine;


public class OrientedPoint {
    public Vector3 position;
    public Quaternion rotation;
    public float other;

    public OrientedPoint(Vector3 position) {
        this.position = position;
        this.rotation = Quaternion.identity;
    }

    public OrientedPoint(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }

    public OrientedPoint(Vector3 position, Quaternion rotation, float other) {
        this.position = position;
        this.rotation = rotation;
        this.other = other;
    }

    public OrientedPoint(Vector3 position, Vector3 forward) {
        this.position = position;
        this.rotation = Quaternion.LookRotation(forward);
    }

    override public string ToString() {
        return "OP(pos: " + position.ToString() + ", rot: " + rotation.ToString() + ")";
    }

    public Vector3 LocalToWorldPosition(Vector3 localSpaceposition) {
        return position + rotation * localSpaceposition;
    }

    public Vector3 LocalToWorldVec(Vector3 localSpaceposition) {
        return rotation * localSpaceposition;
    }
}
