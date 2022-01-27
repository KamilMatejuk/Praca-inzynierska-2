using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType { Basic, Forest, Desert, Mountains }

[CreateAssetMenu()]
public class TerrainGenData : UpdatableData {

    public TerrainType terrainType = TerrainType.Forest;
    [Range(5000f, 50000f)]  public float roadLength = 10000f;
    [Range(1f, 10f)]        public float roadWidth = 7f;
    [Range(0.05f, 0.25f)]   public float paddingPercent = 0.2f;
    [Range(3, 10)]          public int numberOfSegments = 4;
    [Range(0, 40)]          public int numberOfCheckpoints = 40;
    [Range(0f, 1f)]         public float terrainDetailsMain = 0.3f;
    [Range(0f, 0.15f)]      public float terrainDetailsMinor = 0.05f;
    [Range(0f, 0.1f)]       public float terrainDetailsTiny = 0f;
    [Range(0.5f, 5f)]       public float terrainScaleMain = 1.5f;
    [Range(0.1f, 10f)]      public float terrainScaleMinor = 5f;
    [Range(0.1f, 10f)]      public float terrainScaleTiny = 5f;
    [Range(-100f, 100f)]    public float offsetX = 0f;
    [Range(-100f, 100f)]    public float offsetY = 0f;
}
