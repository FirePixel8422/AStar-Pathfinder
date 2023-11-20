using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    [HideInInspector]
    public GridManager grid;

    public float targetMoveDistanceForPathUpdate = 1.5f;
    public int ignoredBaseUpdateRange = 25;
    public int rangeForFasterPathUpdateSpeed = 30;

    private void Start()
    {
        grid = GetComponent<GridManager>();
        grid.Init();
    }
    public void FindPath(Vector3 startPos, Vector3 targetPos, AgentData agent)
    {
        int slopeIndex = agent.slopeIndex;
        bool slopeMode = agent.core.slopeTransition;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos, slopeIndex);
        Node targetNode = grid.NodeFromWorldPoint(targetPos, slopeIndex);

        Heap<Node> openNodes = new Heap<Node>(grid.gridFloors[slopeIndex].MaxSize);
        HashSet<Node> closedNodes = new HashSet<Node>();


        bool useExtraStatsFromAgent = false;
        int[] movPenalties = new int[3];

        int[] agentTerrainModifierLayerId = new int[agent.terrainlayers.Length];
        int[] agentTerrainModifier = new int[agent.terrainlayers.Length];

        if (agent.extraPenalty.Length != 0)
        {
            useExtraStatsFromAgent = true;
            agentTerrainModifier = agent.extraPenalty;
            for (int i = 0; i < agentTerrainModifierLayerId.Length; i++)
            {
                agentTerrainModifierLayerId[i] = (int)agent.terrainlayers[i] * 10;
            }
        }

        openNodes.Add(startNode);
        while (openNodes.Count > 0)
        {
            Node currentNode = openNodes.RemoveFirst();
            closedNodes.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode, agent);
                sw.Stop();
                print("Path Found in " + sw.ElapsedMilliseconds + " ms");
                return;
            }
            foreach (Node neigbour in grid.GetNeigbours(currentNode, slopeIndex))
            {
                if ((!neigbour.walkable && slopeMode == false) || closedNodes.Contains(neigbour))
                {
                    continue;
                }
                movPenalties[0] = neigbour.movementPenalty;
                movPenalties[1] = currentNode.movementPenalty;
                movPenalties[2] = targetNode.movementPenalty;

                if (useExtraStatsFromAgent == true)
                {
                    for (int i = 0; i < agentTerrainModifier.Length; i++)
                    {
                        if (agentTerrainModifierLayerId[i] == neigbour.layerId)
                        {
                            movPenalties[0] = neigbour.movementPenalty + agentTerrainModifier[i];
                        }
                        if (agentTerrainModifierLayerId[i] == currentNode.layerId)
                        {
                            movPenalties[1] = currentNode.movementPenalty + agentTerrainModifier[i];
                        }
                        if (agentTerrainModifierLayerId[i] == targetNode.layerId)
                        {
                            movPenalties[2] = targetNode.movementPenalty + agentTerrainModifier[i];
                        }
                    }
                }

                int2 currentNodeGridPos = currentNode.gridPos;

                int neigbourDist = GetDistance(movPenalties[1], movPenalties[0], currentNodeGridPos, neigbour.gridPos);
                int newMovementCostToNeigbour = currentNode.gCost + neigbourDist + movPenalties[0] / 10 * neigbourDist;

                if (newMovementCostToNeigbour < neigbour.gCost || !openNodes.Contains(neigbour))
                {
                    neigbour.gCost = newMovementCostToNeigbour;

                    neigbour.hCost = GetDistance(movPenalties[0], movPenalties[2], neigbour.gridPos, targetNode.gridPos);
                    neigbour.parentIndex = currentNodeGridPos;

                    if (!openNodes.Contains(neigbour))
                    {
                        openNodes.Add(neigbour);
                    }
                }
            }
        }
    }
    private void RetracePath(Node startNode, Node endNode, AgentData agent)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = grid.gridFloors[agent.slopeIndex].grid[currentNode.parentIndex.x, currentNode.parentIndex.y];
        }
        path.Reverse();

        agent.core.path = path;
    }

    private int GetDistance(int movPenaltyA, int movPenaltyB, int2 gridPosA, int2 gridPosB)
    {
        int distX = Mathf.Abs(gridPosA.x - gridPosB.x);
        int distZ = Mathf.Abs(gridPosA.y - gridPosB.y);

        if (distX > distZ)
        {
            return (14 + movPenaltyA / 10 * 14) * distZ + (10 + movPenaltyA) * (distX - distZ);
        }
        else
        {
            return (14 + movPenaltyA / 10 * 14) * distX + (10 + movPenaltyA) * (distZ - distX);
        }
    }
}
