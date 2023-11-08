using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

//[ExecuteInEditMode]
public class Pathfinding : MonoBehaviour
{
    public GridManager grid;

    public Transform seeker, target;
    private Vector3 oldTargetPos;
    public float targetMoveDistanceForPathUpdate = 2;

    private void Awake()
    {
        grid = GetComponent<GridManager>();
    }
    private void Update()
    {
        /*if(Vector3.Distance(oldTargetPos, target.position) > targetMoveDistanceForPathUpdate || (Vector3.Distance(seeker.position, target.position) < targetMoveDistanceForPathUpdate * 2 && Vector3.Distance(oldTargetPos, target.position) > 0.01f))
        {
            FindPath(seeker.position, target.position);
        }*/
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindPath(seeker.position, target.position);
        }
    }
    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        oldTargetPos = targetPos;
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap<Node> openNodes = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedNodes = new HashSet<Node>();

        openNodes.Add(startNode);
        while(openNodes.Count > 0)
        {
            Node currentNode = openNodes.RemoveFirst();
            closedNodes.Add(currentNode);

            if(currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                grid.recalculated = true;
                sw.Stop();
                print("Path Found " + sw.ElapsedMilliseconds + " ms");
                return;
            }
            foreach (Node neigbour in grid.GetNeigbours(currentNode))
            {
                if(!neigbour.walkable || closedNodes.Contains(neigbour))
                {
                    continue;
                }
                int neigbourDist = GetDistance(currentNode, neigbour);
                int newMovementCostToNeigbour = currentNode.gCost + neigbourDist + neigbour.movementPenalty / 10 * neigbourDist;
                if (newMovementCostToNeigbour < currentNode.gCost || !openNodes.Contains(neigbour)){
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
    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
        grid.recalculated = true;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        if(distX > distZ)
        {
            return 14 * distZ + 10 * (distX - distZ);
        }
        else
        {
            return 14 * distX + 10 * (distZ - distX);
        }
    }
}
