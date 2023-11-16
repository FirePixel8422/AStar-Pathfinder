using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentData : MonoBehaviour
{
    [HideInInspector]
    public AgentCore core;

    public List<LayerMask> terrainLayerMasks;

    [HideInInspector]
    public List<int> terrainLayer;
    public int[] extraPenalty;

    public int slopeIndex;

    private void Start()
    {
        for (int i = 0; i < terrainLayerMasks.Count; i++)
        {
            terrainLayer[i] = Mathf.RoundToInt(Mathf.Log(terrainLayerMasks[i].value, 2));
        }
        core = GetComponent<AgentCore>();
    }
}



