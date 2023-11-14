using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeManager : MonoBehaviour
{
    private TargetTracker target;
    public Slope[] slopes;

    private void Start()
    {
        target = FindObjectOfType<TargetTracker>();
    }

    public int targetSlopeIndex
    {
        get
        {
            return target.slopeIndex;
        }
    }
    [System.Serializable]
    public class Slope
    {
        public Transform slopeStart, slopeEnd;
        public float heightLevel;
    }
}
