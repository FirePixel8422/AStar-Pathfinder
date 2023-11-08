using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    public LayerMask unwalkableLayer;
    public Vector3 gridSize;

    public Node[,] grid;
    public bool recalculated;

    [Range(0.1f, 5)]
    public float nodeSize;
    [HideInInspector]
    public float halfNodeSize;
    public TerrainType[] walkableRegions;
    private LayerMask walkableLayers;
    private Dictionary<int, int> walkableRegionsDictionairy = new Dictionary<int, int>();

    [HideInInspector]
    public int gridSizeX, gridSizeZ;


    private void Start()
    {
        halfNodeSize = nodeSize / 2;
        gridSizeX = Mathf.RoundToInt(gridSize.x / nodeSize);
        gridSizeZ = Mathf.RoundToInt(gridSize.z / nodeSize);
        foreach (TerrainType region in walkableRegions)
        {
            walkableLayers.value |= region.terrainLayer.value;
            walkableRegionsDictionairy.Add((int)Mathf.Log(region.terrainLayer.value, 2), region.terrainPenalty);
        }
        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeZ];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.z / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++) 
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeSize + halfNodeSize) + Vector3.forward * (z * nodeSize + halfNodeSize);
                bool walkable = !Physics.CheckSphere(worldPoint, halfNodeSize, unwalkableLayer);

                int movementPenalty = 0;

                if(walkable == true)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if(Physics.Raycast(ray, out hit, 100, walkableLayers))
                    {
                        walkableRegionsDictionairy.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }
                }

                grid[x, z] = new Node(walkable, worldPoint, x, z, movementPenalty);
            }
        }
    }

    public List<Node> GetNeigbours(Node node)
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
                int checkX = node.gridX + x;
                int checkZ = node.gridZ + z;

                if(checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ) 
                {
                    neigbours.Add(grid[checkX, checkZ]);
                }
            }
        }
        return neigbours;
    }
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridSize.x / 2) / gridSize.x;
        float percentZ = (worldPosition.z + gridSize.z / 2) / gridSize.z;
        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);
        return grid[x, z];
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainLayer;
        public int terrainPenalty;
    }

    public List<Node> path = new List<Node>();

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.z));

        if(grid != null)
        {
            foreach(Node node in grid)
            {
                Gizmos.color = new Color(0, 0, 0, 0);
                if(path != null)
                {
                    if (path.Contains(node))
                    {
                        Gizmos.color = Color.black;
                    }
                }
                Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeSize * 0.9f));
            }
        }
    }
}
