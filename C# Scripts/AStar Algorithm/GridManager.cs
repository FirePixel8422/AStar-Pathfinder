using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    //public Transform textObj;

    public LayerMask unwalkableLayer;
    public int unwalkableLayerId;

    public TerrainType[] walkableRegions;
    public GridFloor[] gridFloors;

    private AgentCore[] agents;

    public Color[] layerColors;

    private LayerMask walkableLayers;
    private Dictionary<int, int> walkableRegionsDictionairy = new Dictionary<int, int>();


    public void Init()
    {
        agents = FindObjectsOfType<AgentCore>();
        unwalkableLayerId = Mathf.RoundToInt(Mathf.Log(unwalkableLayer.value, 2));

        foreach (TerrainType region in walkableRegions)
        {
            walkableLayers.value |= region.terrainLayer.value;
            walkableRegionsDictionairy.Add((int)Mathf.Log(region.terrainLayer.value, 2), region.terrainPenalty);
        }
        CreateGridAsync();
    }

    public void CreateGridAsync()
    {
        int movementPenalty = 0;
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
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.z / 2;


            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeSize + halfNodeSize) + Vector3.forward * (z * nodeSize + halfNodeSize);

                    int layerId = -1;
                    var data = Physics.OverlapSphere(new Vector3(worldPoint.x, worldPoint.y + gridFloor.floorHeight + 0.15f, worldPoint.z), nodeSize / 2, walkableLayers + unwalkableLayer, QueryTriggerInteraction.Collide);
                    if (data.Length != 0)
                    {
                        float highestYPos = data[0].transform.position.y;
                        layerId = data[0].gameObject.layer;
                        for (int i2 = 0; i2 < data.Length; i2++)
                        {
                            if(highestYPos < data[i2].transform.position.y)
                            {
                                highestYPos = data[i2].transform.position.y;
                                layerId = data[i2].gameObject.layer;
                            }
                        }
                    }

                    if (layerId == unwalkableLayerId || layerId == -1)
                    {
                        gridFloor.grid[x, z] = new Node(false, worldPoint, new int2(x, z), layerId, movementPenalty);
                    }
                    else
                    {
                        walkableRegionsDictionairy.TryGetValue(layerId, out movementPenalty);
                        gridFloor.grid[x, z] = new Node(true, worldPoint, new int2(x, z), layerId, movementPenalty);
                    }
                    print(layerId);
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

        public bool drawGizmos = true;
        public Color gizmoColor = Color.black;

        public Vector3 gridSize;
        public float floorHeight;

        [Range(0.25f, 5)]
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
        for (int i0 = 0; i0 < gridFloors.Length; i0++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y + gridFloors[i0].floorHeight, transform.position.z), new Vector3(gridFloors[i0].gridSize.x, 0.5f, gridFloors[i0].gridSize.z));
        }
        if (Application.isPlaying == false)
        {
            return;
        }
        for (int i = 0; i < gridFloors[0].grid.GetLength(0); i++)
        {
            for (int i2 = 0; i2 < gridFloors[0].grid.GetLength(1); i2++)
            {
                Gizmos.color = layerColors[gridFloors[0].grid[i, i2].layerId];
                Gizmos.DrawCube(gridFloors[0].grid[i, i2].worldPos, Vector3.one * gridFloors[0].nodeSize * 0.9f);
            }
        }

        /*for (int i = 0; i < agents.Length; i++)
        {
            int slopeIndex = agents[i].agent.slopeIndex;
            if (gridFloors.Length <= slopeIndex)
            {
                Debug.LogError("Cant draw gizmos, index is out of bounds \nAgent.slopeIndex > gridFloors.Length");
                return;
            }
            if (gridFloors.Length != 0 && gridFloors[slopeIndex].drawGizmos == true)
            {
                Vector3 height = new Vector3(0, gridFloors[slopeIndex].floorHeight, 0);
                Gizmos.color = gridFloors[slopeIndex].gizmoColor;
                for (int i2 = 0; i2 < agents[i].path.Count; i2++)
                {
                    Gizmos.DrawCube(agents[i].path[i2].worldPos + height, Vector3.one * (gridFloors[slopeIndex].nodeSize * 0.9f));
                }
            }
        }*/
    }
}
