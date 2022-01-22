using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Objects : MonoBehaviour {

    public static void RemoveAllObjectsByTag(string tag) {
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(tag)) {
            // Destroy(obj);
            DestroyImmediate(obj);
        }
    }

    public static void RemoveObjectsByTagInParent(string tag, GameObject parent) {
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(tag)) {
            if (obj != null && obj.transform.parent != null && obj.transform.parent == parent.transform) {
                // Destroy(obj);
                DestroyImmediate(obj);
            }
        }
    }

    public static GameObject PutParentObject(string tag, string name) {
        GameObject parent = new GameObject(name);
        parent.tag = tag;
        return parent;
    }

    public static GameObject PutObject(string prefabName, string tag, string name, OrientedPoint posrot, Vector3? size = null) {
        GameObject gameObjectPrefab = (GameObject) Resources.Load("Prefabs/" + prefabName);
        GameObject gameobject = GameObject.Instantiate(gameObjectPrefab, posrot.position, posrot.rotation);
        gameobject.name = name;
        gameobject.tag = tag;
        if (size != null) {
            gameobject.transform.localScale = (Vector3)size;
        }
        Vector3 pos = posrot.position;
        // pos.y += 1f + controlPoints.GetHeight(posrot.position.x, posrot.position.z) * 128;
        gameobject.transform.position = pos;
        return gameobject;
    }

    public static GameObject GetChildWithName(GameObject obj, string name) {
        if (obj == null) return null;
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null) {
            return childTrans.gameObject;
        }
        for (int i = 0; i < trans.childCount; i++) {
            childTrans = trans.GetChild(i);
            GameObject childGO = GetChildWithName(childTrans.gameObject, name);
            if (childGO != null) {
                return childGO;
            }
        }
        return null;
    }

    public static GameObject GetParentWithName(GameObject obj, string name) {
        if (obj == null || obj.transform.parent == null) {
            return null;
        }
        GameObject parent = obj.transform.parent.gameObject;
        if (parent == null) {
            return null;
        }
        if (parent.name == name) {
            return parent;
        }
        return GetParentWithName(parent, name);
    }

}