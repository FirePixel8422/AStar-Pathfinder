using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using System;

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
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos, slopeIndex);
        Node targetNode = grid.NodeFromWorldPoint(targetPos, slopeIndex);

        Heap<Node> openNodes = new Heap<Node>(grid.gridFloors[slopeIndex].MaxSize);
        HashSet<Node> closedNodes = new HashSet<Node>();

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
                if (!neigbour.walkable || closedNodes.Contains(neigbour))
                {
                    continue;
                }
                int additionalMovPenalty = 0;

                if(agent.terrainLayer.Count > 0)
                {
                    int index = agent.terrainLayer.FindIndex(terrainLayer => terrainLayer == neigbour.layerId);
                    additionalMovPenalty = index != -1 ? agent.extraPenalty.Length != 0 ? agent.extraPenalty[index] : 0 : 0;

                    index = agent.terrainLayer.FindIndex(terrainLayer => terrainLayer == currentNode.layerId);
                    additionalMovPenalty = index != -1 ? agent.extraPenalty.Length != 0 ? agent.extraPenalty[index] : 0 : 0;

                    index = agent.terrainLayer.FindIndex(terrainLayer => terrainLayer == targetNode.layerId);
                    additionalMovPenalty = index != -1 ? agent.extraPenalty.Length != 0 ? agent.extraPenalty[index] : 0 : 0;
                }

                int neigbourMovPenalty = neigbour.movementPenalty + additionalMovPenalty;

                int2 neigbourGridPos = neigbour.gridPos;

                int2 currentNodeGridPos = currentNode.gridPos;

                int neigbourDist = GetDistance(currentNode.movementPenalty + additionalMovPenalty, neigbourMovPenalty, currentNodeGridPos, neigbourGridPos);
                int newMovementCostToNeigbour = currentNode.gCost + neigbourDist + neigbourMovPenalty / 10 * neigbourDist;

                if (newMovementCostToNeigbour < neigbour.gCost || !openNodes.Contains(neigbour))
                {
                    neigbour.gCost = newMovementCostToNeigbour;

                    neigbour.hCost = GetDistance(neigbourMovPenalty, targetNode.movementPenalty + additionalMovPenalty, neigbourGridPos, targetNode.gridPos);
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
    /*private int GetDistance(int movPenaltyA, int movPenaltyB, int2 gridPosA, int2 gridPosB)
    {
        int distX = Math.Abs(gridPosA.x - gridPosB.x);
        int distZ = Math.Abs(gridPosA.y - gridPosB.y);

        int straightCost = 10;
        int diagonalCost = 14;

        int minDist = Math.Min(distX, distZ);
        int maxDist = Math.Max(distX, distZ);

        int diagonalSteps = minDist;
        int straightSteps = maxDist - minDist;

        return diagonalCost * diagonalSteps + straightCost * straightSteps + (movPenaltyA / 10) * diagonalCost * minDist;
    }*/
}
