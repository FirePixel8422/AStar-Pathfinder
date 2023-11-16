using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class Node : IHeapItems<Node>
{
    public int layerId;

    public bool walkable;
    public Vector3 worldPos;

    public int2 gridPos;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public int2 parentIndex;

    private int heapIndex;

    public Node(bool _walkable, Vector3 _worldPos, int2 _gridPos, int _layerId, int _movementPenalty)
    {
        walkable = _walkable;
        worldPos = _worldPos;
        gridPos = _gridPos;
        layerId = _layerId;
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