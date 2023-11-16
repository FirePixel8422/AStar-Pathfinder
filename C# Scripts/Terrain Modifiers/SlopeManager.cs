using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeManager : MonoBehaviour
{
    public Slope[] slopes;


    [System.Serializable]
    public class Slope
    {
        public Transform slopeStart, slopeEnd;
        public float heightLevel;
    }
}
