using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public LayerMask walkableObjectLayer;

    public TerrainLayerType[] walkableRegions;
    public GridFloor[] gridFloors;

    private AgentCore[] agents;

    public Color[] nodeLayerColors;


    public void Init()
    {
        agents = FindObjectsOfType<AgentCore>();
        CreateGridAsync();
    }

    public void CreateGridAsync()
    {
        int movementPenalty;
        int layerId;
        int highestLayerPriority;

        for (int i = 0; i < gridFloors.Length; i++)
        {
            GridFloor gridFloor = gridFloors[i];

            float nodeSize = gridFloor.nodeSize;
            float halfNodeSize = nodeSize / 2;

            Vector3 gridSize = gridFloor.gridSize;

            gridFloor.gridSizeX = Mathf.RoundToInt(gridSize.x / nodeSize);
            gridFloor.gridSizeZ = Mathf.RoundToInt(gridSize.z / nodeSize);

            int gridSizeX = gridFloor.gridSizeX;
            int gridSizeZ = gridFloor.gridSizeZ;

            gridFloor.grid = new Node[gridSizeX, gridSizeZ];
            Vector3 worldBottomLeft = gridFloor.gridPosition - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.z / 2;


            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    movementPenalty = 0;
                    layerId = -10;
                    highestLayerPriority = int.MaxValue;

                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeSize + halfNodeSize) + Vector3.forward * (z * nodeSize + halfNodeSize);

                    Collider[] data = Physics.OverlapSphere(worldPoint, nodeSize * 0.75f, walkableObjectLayer);
                    if (data.Length == 0)
                    {
                        gridFloor.grid[x, z] = new Node(false, worldPoint, new int2(x, z), -10, movementPenalty);
                        continue;
                    }
                    TerrainObject[] terrainObjects = new TerrainObject[data.Length];
                    for (int i2 = 0; i2 < data.Length; i2++)
                    {
                        terrainObjects[i2] = data[i2].gameObject.GetComponent<TerrainObject>();
                        if (terrainObjects[i2] == null || terrainObjects[i2].gridFloorId != i)
                        {
                            terrainObjects[i2] = null;
                        }
                    }
                    for (int i2 = 0; i2 < terrainObjects.Length; i2++)
                    {
                        if (terrainObjects[i2] == null)
                        {
                            continue;
                        }
                        if (terrainObjects[i2].terrainType == TerrainType.custom && terrainObjects[i2].customSettings.overridePriority * 10 < highestLayerPriority)
                        {
                            highestLayerPriority = terrainObjects[i2].customSettings.overridePriority;
                            layerId = i2;
                        }
                        else if (walkableRegions[(int)terrainObjects[i2].terrainType].priority * 10 < highestLayerPriority)
                        {
                            highestLayerPriority = walkableRegions[(int)terrainObjects[i2].terrainType].priority * 10;
                            layerId = i2;
                        }
                    }

                    if (layerId == -10)
                    {
                        gridFloor.grid[x, z] = new Node(false, worldPoint, new int2(x, z), -10, 0);
                        continue;
                    }


                    if (terrainObjects[layerId].terrainType == TerrainType.custom)
                    {
                        gridFloor.grid[x, z] = new Node(true, worldPoint, new int2(x, z), layerId, terrainObjects[layerId].customSettings.overrideTerrainPenalty);
                    }
                    else if (terrainObjects[layerId].terrainType == TerrainType.unwalkable)
                    {
                        gridFloor.grid[x, z] = new Node(false, worldPoint, new int2(x, z), (int)terrainObjects[layerId].terrainType * 10, 0);
                    }
                    else
                    {
                        for (int i2 = 0; i2 < walkableRegions.Length; i2++)
                        {
                            if (walkableRegions[i2].terrainType == terrainObjects[layerId].terrainType)
                            {
                                movementPenalty = walkableRegions[i2].terrainPenalty;
                            }
                        }
                        gridFloor.grid[x, z] = new Node(true, worldPoint, new int2(x, z), (int)terrainObjects[layerId].terrainType * 10, movementPenalty);
                    }
                }
            }
        }
    }

    public List<Node> GetNeigbours(Node node, int slopeIndex)
    {
        List<Node> neigbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                {
                    continue;
                }
                int checkX = node.gridPos.x + x;
                int checkZ = node.gridPos.y + z;

                if (checkX >= 0 && checkX < gridFloors[slopeIndex].gridSizeX && checkZ >= 0 && checkZ < gridFloors[slopeIndex].gridSizeZ)
                {
                    neigbours.Add(gridFloors[slopeIndex].grid[checkX, checkZ]);
                }
            }
        }
        return neigbours;
    }
    public Node NodeFromWorldPoint(Vector3 worldPosition, int slopeIndex)
    {
        float percentX = (worldPosition.x + gridFloors[slopeIndex].gridSize.x / 2) / gridFloors[slopeIndex].gridSize.x;
        float percentZ = (worldPosition.z + gridFloors[slopeIndex].gridSize.z / 2) / gridFloors[slopeIndex].gridSize.z;
        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridFloors[slopeIndex].gridSizeX - 1) * percentX);
        int z = Mathf.RoundToInt((gridFloors[slopeIndex].gridSizeZ - 1) * percentZ);
        return gridFloors[slopeIndex].grid[x, z];
    }

    [System.Serializable]
    public class GridFloor
    {
        public Node[,] grid;

        public bool drawPathGizmos = true;
        public bool drawNodeColorGizmos = false;
        public Color pathNodesColor = Color.black;

        public Vector3 gridSize;
        public Vector3 gridPosition;

        [Range(0.15f, 5)]
        public float nodeSize;

        [HideInInspector]
        public int gridSizeX, gridSizeZ;

        public int MaxSize
        {
            get
            {
                return gridSizeX * gridSizeZ;
            }
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < gridFloors.Length; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(gridFloors[i].gridPosition, new Vector3(gridFloors[i].gridSize.x, 0.5f, gridFloors[i].gridSize.z));
        }
        if (Application.isPlaying == false)
        {
            return;
        }
        
        for (int i = 0; i < gridFloors.Length; i++)
        {
            if (gridFloors[i].nodeSize < .25f)
            {
                Debug.Log("Not a good idea, to many nodes to draw");
                gridFloors[i].drawNodeColorGizmos = false;
                return;
            }
            if (gridFloors[i].drawNodeColorGizmos == true)
            {
                for (int i2 = 0; i2 < gridFloors[i].grid.GetLength(0); i2++)
                {
                    for (int i3 = 0; i3 < gridFloors[i].grid.GetLength(1); i3++)
                    {
                        Gizmos.color = new Color(0, 0, 0, 0);
                        if (gridFloors[i].grid[i2, i3].layerId / 10 != -1)
                        {
                            Gizmos.color = nodeLayerColors[gridFloors[i].grid[i2, i3].layerId / 10];
                        }
                        Gizmos.DrawCube(gridFloors[i].grid[i2, i3].worldPos, Vector3.one * gridFloors[i].nodeSize * 0.9f);
                    }
                }
            }
        }

        for (int i = 0; i < agents.Length; i++)
        {
            int slopeIndex = agents[i].agent.slopeIndex;
            if (gridFloors.Length <= slopeIndex)
            {
                Debug.LogError("Cant draw gizmos, index is out of bounds \nAgent.slopeIndex > gridFloors.Length");
                return;
            }

            if (gridFloors.Length != 0 && gridFloors[slopeIndex].drawPathGizmos == true)
            {
                Gizmos.color = gridFloors[slopeIndex].pathNodesColor;
                for (int i2 = 0; i2 < agents[i].path.Count; i2++)
                {
                    Gizmos.DrawCube(agents[i].path[i2].worldPos, Vector3.one * (gridFloors[slopeIndex].nodeSize * 0.9f));
                }
            }
        }
    }
}
