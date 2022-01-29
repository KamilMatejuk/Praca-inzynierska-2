using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Objects : MonoBehaviour {

    /// <summary>
    /// Remove all objects by tag
    /// </summary>
    /// <param name="tag">Object tag</param>
    public static void RemoveAllObjectsByTag(string tag) {
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(tag)) {
            // Destroy(obj);
            DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// Remove all objects by tag, which are children of some object
    /// </summary>
    /// <param name="tag">Object tag</param>
    /// <param name="parent">Parent object</param>
    public static void RemoveObjectsByTagInParent(string tag, GameObject parent) {
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(tag)) {
            if (obj != null && obj.transform.parent != null && obj.transform.parent == parent.transform) {
                // Destroy(obj);
                DestroyImmediate(obj);
            }
        }
    }

    /// <summary>
    /// Create empty object with tag and name
    /// </summary>
    /// <param name="tag">Object tag</param>
    /// <param name="name">Object name</param>
    /// <returns>Created GameObject</returns>
    public static GameObject PutParentObject(string tag, string name) {
        GameObject parent = new GameObject(name);
        parent.tag = tag;
        return parent;
    }

    /// <summary>
    /// Create object from prefab
    /// </summary>
    /// <param name="prefabName">Name of prefab asset</param>
    /// <param name="tag">Object tag</param>
    /// <param name="name">Object name</param>
    /// <param name="posrot">Position and rotation of object</param>
    /// <param name="size">Size of object</param>
    /// <returns>Created GameObject</returns>
    public static GameObject PutObject(string prefabName, string tag, string name, OrientedPoint posrot, Vector3? size = null) {
        GameObject gameObjectPrefab = (GameObject) Resources.Load("Prefabs/" + prefabName);
        GameObject gameobject = GameObject.Instantiate(gameObjectPrefab, posrot.position, posrot.rotation);
        gameobject.name = name;
        gameobject.tag = tag;
        if (size != null) {
            gameobject.transform.localScale = (Vector3)size;
        }
        Vector3 pos = posrot.position;
        gameobject.transform.position = pos;
        return gameobject;
    }

    /// <summary>
    /// Get object by name, which is below some object in hierarchy
    /// </summary>
    /// <param name="obj">Parent object</param>
    /// <param name="name">Object name</param>
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

    /// <summary>
    /// Get object by name, which is above some object in hierarchy
    /// </summary>
    /// <param name="obj">Child object</param>
    /// <param name="name">Object name</param>
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