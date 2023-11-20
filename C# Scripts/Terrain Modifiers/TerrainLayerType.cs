using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainLayerType
{
    public TerrainType terrainType = TerrainType.unwalkable;
    public int terrainPenalty;
}
