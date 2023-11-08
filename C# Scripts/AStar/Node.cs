using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public bool walkable;
    public Vector3 worldPos;

    public int gridX, gridZ;
    public int movementPenalty;

    public Node parent;

    public int gCost;
    public int hCost;

    public Node(bool _walkable, Vector3 _worldPos, int _grixX, int _gridZ, int _movementPenalty)
    {
        walkable = _walkable;
        worldPos = _worldPos;
        gridX = _grixX;
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
}
