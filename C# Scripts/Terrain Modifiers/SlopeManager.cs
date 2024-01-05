using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeManager : MonoBehaviour
{
    public SlopeData[] slopes;


    [System.Serializable]
    public class SlopeData
    {
        public Transform slopeStart, slopeEnd;
        public float heightLevel;
    }
}
