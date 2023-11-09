using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector3 worldPos;

    public int gridX, gridZ;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public Node parent;
    
    private int heapIndex;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridZ, int _movementPenalty)
    {
        walkable = _walkable;
        worldPos = _worldPos;
        gridX = _gridX;
        gridZ = _gridZ;
        movementPenalty = _movementPenalty;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}