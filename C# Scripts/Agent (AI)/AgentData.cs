using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentData : MonoBehaviour
{
    [HideInInspector]
    public AgentCore core;

    public TerrainType[] terrainlayers;
    public int[] extraPenalty;

    public int slopeIndex;

    private void Start()
    {
        core = GetComponent<AgentCore>();
    }
}



