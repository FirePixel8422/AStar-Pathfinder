using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainObject : MonoBehaviour
{
    public TerrainType terrainType = TerrainType.unwalkable;
    public int gridFloorId;
    public CustomSettings customSettings;


    [System.Serializable]
    public class CustomSettings
    {
        [Header("Unwalkable = 10, wet_concrete = 20, lower = higher priority")]
        public int overridePriority;
        public int overrideTerrainPenalty;
    }
}

public enum TerrainType
{
    custom,
    unwalkable,
    wet_Concrete,
    water,
    grass,
    mud
}
