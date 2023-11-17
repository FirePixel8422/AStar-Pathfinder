using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetInfo : MonoBehaviour
{
    private GridManager grid;

    public int slopeIndex;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
    }
}
