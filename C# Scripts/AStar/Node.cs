using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector3 worldPos;

    public int gridX, gridZ;
    public int movementPenalty;

    public int heightLevelRequired;

    public Node parent;

    public int gCost;
    public int hCost;

    public Node(bool _walkable, Vector3 _worldPos, int _grixX, int _gridZ, int _movementPenalty, int _heightLevelRequired)
    {
        walkable = _walkable;
        worldPos = _worldPos;
        gridX = _grixX;
        gridZ = _gridZ;
        movementPenalty = _movementPenalty;
        heightLevelRequired = _heightLevelRequired;
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
            return HeapIndex;
        }
        set
        {
            HeapIndex = value;
        }
    }
    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
