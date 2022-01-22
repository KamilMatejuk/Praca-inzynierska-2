using UnityEngine;
using Unity.Barracuda;
using System.IO;


public static class Ai {

    // https://github.com/Unity-Technologies/ml-agents/blob/0.14.1/Project/Assets/ML-Agents/Examples/SharedAssets/Scripts/ModelOverrider.cs#L114-L128
    public static NNModel LoadModel1(string path) {
        byte[] model = null;
        try {
            model = File.ReadAllBytes(path);
        } catch (IOException) {
            Debug.Log($"Couldn't load file {path}");
            return null;
        }
        NNModel asset = ScriptableObject.CreateInstance<NNModel>();
        asset.modelData = ScriptableObject.CreateInstance<NNModelData>();
        asset.modelData.Value = model;
        string[] pathArr = path.Split('/');
        string name = pathArr[pathArr.Length - 1];
        asset.name = name.Split('.')[0];
        return asset;
    }

    // https://github.com/Unity-Technologies/barracuda-release/issues/76
    public static NNModel LoadModel2(string path) {
        // return ModelLoader.Load((NNModel)Resources.Load(path));
        return (NNModel)Resources.Load(path);
    }
}
