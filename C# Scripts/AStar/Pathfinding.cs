using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[ExecuteInEditMode]
public class Pathfinding : MonoBehaviour
{
    [HideInInspector]
    public GridManager grid;
    public float targetMoveDistanceForPathUpdate = 1.5f;

    private void Start()
    {
        grid = GetComponent<GridManager>();
        grid.Init();
    }
    public void FindPath(Vector3 startPos, Vector3 targetPos, Agent agent)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap<Node> openNodes = new Heap<Node>(grid.MaxSize);
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
            foreach (Node neigbour in grid.GetNeigbours(currentNode))
            {
                if (!neigbour.walkable || closedNodes.Contains(neigbour))
                {
                    continue;
                }
                int neigbourDist = GetDistance(currentNode, neigbour);
                int newMovementCostToNeigbour = currentNode.gCost + neigbourDist + neigbour.movementPenalty / 10 * neigbourDist;
                if (newMovementCostToNeigbour < currentNode.gCost || !openNodes.Contains(neigbour))
                {
                    neigbour.gCost = newMovementCostToNeigbour;
                    neigbour.hCost = GetDistance(neigbour, targetNode);
                    neigbour.parent = currentNode;

                    if (!openNodes.Contains(neigbour))
                    {
                        openNodes.Add(neigbour);
                    }
                }
            }
        }
    }
    private void RetracePath(Node startNode, Node endNode, Agent agent)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        agent.path = path;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        if (distX > distZ)
        {
            return (14 + nodeA.movementPenalty / 10 * 14) * distZ + (10 + nodeA.movementPenalty) * (distX - distZ);
        }
        else
        {
            return (14 + nodeA.movementPenalty / 10 * 14) * distX + (10 + nodeA.movementPenalty) * (distZ - distX);
        }
    }
}
