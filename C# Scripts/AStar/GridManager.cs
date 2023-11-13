using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Transform textObj;
    public bool drawGizmos = true;

    public LayerMask unwalkableLayer;
    public Vector3 gridSize;

    public Agent[] agents;
    public Color gizmoColor = Color.black; 

    public Node[,] grid;

    [Range(0.1f, 5)]
    public float nodeSize;
    [HideInInspector]
    public float halfNodeSize;

    public TerrainType[] walkableRegions;
    private LayerMask walkableLayers;
    private Dictionary<int, int> walkableRegionsDictionairy = new Dictionary<int, int>();

    [HideInInspector]
    public int gridSizeX, gridSizeZ;
     

    public void Init()
    {
        agents = FindObjectsOfType<Agent>();
        halfNodeSize = nodeSize / 2;
        gridSizeX = Mathf.RoundToInt(gridSize.x / nodeSize);
        gridSizeZ = Mathf.RoundToInt(gridSize.z / nodeSize);
        foreach (TerrainType region in walkableRegions)
        {
            walkableLayers.value |= region.terrainLayer.value;
            walkableRegionsDictionairy.Add((int)Mathf.Log(region.terrainLayer.value, 2), region.terrainPenaltyMultiplier);
        }
        StartCoroutine(CreateGridAsync());
    }
    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeZ;
        }
    }

    public IEnumerator CreateGridAsync()
    {
        grid = new Node[gridSizeX, gridSizeZ];
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
                    grid[x, z] = new Node(walkable, worldPoint, new int2(x, z), movementPenalty);
                    continue;
                }

                ray.origin = worldPoint + Vector3.up * 50;
                ray.direction = Vector3.down;

                if (Physics.Raycast(ray, out hit, 100, walkableLayers))
                {
                    walkableRegionsDictionairy.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                grid[x, z] = new Node(walkable, worldPoint, new int2(x, z), movementPenalty);

                yield return null;
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
                int checkX = node.gridPos.x + x;
                int checkZ = node.gridPos.y + z;

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
        public int terrainPenaltyMultiplier;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.z));

        if (grid != null && drawGizmos == true)
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
                    Gizmos.color = gizmoColor;
                }
                Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeSize * 0.9f));
            }
        }
    }
}
