using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    //public Transform textObj;

    public LayerMask unwalkableLayer;
    public TerrainType[] walkableRegions;
    public GridFloor[] gridFloors;

    private Agent[] agents;

    private LayerMask walkableLayers;
    private Dictionary<int, int> walkableRegionsDictionairy = new Dictionary<int, int>();



    public void Init()
    {
        agents = FindObjectsOfType<Agent>();
        foreach (TerrainType region in walkableRegions)
        {
            walkableLayers.value |= region.terrainLayer.value;
            walkableRegionsDictionairy.Add((int)Mathf.Log(region.terrainLayer.value, 2), region.terrainPenalty);
        }
        CreateGridAsync();
    }

    public void CreateGridAsync()
    {
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

            Ray ray = new Ray();
            RaycastHit hit;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeSize + halfNodeSize) + Vector3.forward * (z * nodeSize + halfNodeSize);
                    bool walkable = !Physics.CheckSphere(worldPoint, halfNodeSize, unwalkableLayer);

                    int movementPenalty = 0;

                    if (walkable == false)
                    {
                        gridFloor.grid[x, z] = new Node(walkable, worldPoint, new int2(x, z), movementPenalty);
                        continue;
                    }

                    ray.origin = new Vector3(worldPoint.x, worldPoint.y + gridFloor.floorHeight + 2, worldPoint.z);
                    ray.direction = Vector3.down;

                    if (Physics.Raycast(ray, out hit, 100, walkableLayers))
                    {
                        walkableRegionsDictionairy.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                    gridFloor.grid[x, z] = new Node(walkable, worldPoint, new int2(x, z), movementPenalty);
                }
            }
        }
    }

    public List<Node> GetNeigbours(Node node, int slopeIndex)
    {
        List<Node> neigbours = new List<Node>();
        for (int x = -1; x <= 1 ; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if(x == 0 && z == 0)
                {
                    continue;
                }
                int checkX = node.gridPos.x + x;
                int checkZ = node.gridPos.y + z;

                if(checkX >= 0 && checkX < gridFloors[slopeIndex].gridSizeX && checkZ >= 0 && checkZ < gridFloors[slopeIndex].gridSizeZ) 
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
    public class TerrainType
    {
        public LayerMask terrainLayer;
        public int terrainPenalty;
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
            if (gridFloors[i0].grid != null && gridFloors[i0].drawGizmos == true)
            {
                List<Node> combinedNodes = new List<Node>();
                for (int i = 0; i < agents.Length; i++)
                {
                    for (int i2 = 0; i2 < agents[i].path.Count; i2++)
                    {
                        combinedNodes.Add(agents[i].path[i2]);
                    }
                }
                foreach (Node node in combinedNodes)
                {
                    Gizmos.color = new Color(0, 0, 0, 0);
                    if (combinedNodes != null && combinedNodes.Contains(node))
                    {
                        Gizmos.color = gridFloors[i0].gizmoColor;
                    }
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (gridFloors[i0].nodeSize * 0.9f));
                }
            }
        }
    }
}
