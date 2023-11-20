using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Percentage speed reduction \n85 = -85% speed on selected terrain")]
    public TerrainLayerType[] walkableRegions;

    public LayerMask walkableObjectLayer;

    public float moveSpeed = 3;
    public int degree;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            dir.z += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dir.x -= 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dir.z -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dir.x += 1;
        }
        if(dir != Vector3.zero)
        {
            int highestLayerPriority = 0;
            int layerId = 0;
            float _moveSpeed = 0;

            Collider[] data = Physics.OverlapSphere(new Vector3(transform.position.x, transform.position.y - transform.lossyScale.y, transform.position.z), .5f, walkableObjectLayer);
            if (data.Length == 0)
            {
                dir *= -_moveSpeed;
            }
            TerrainObject[] terrainObjects = new TerrainObject[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                terrainObjects[i] = data[i].gameObject.GetComponent<TerrainObject>();
            }
            for (int i = 0; i < terrainObjects.Length; i++)
            {
                if (terrainObjects[2] == null)
                {
                    continue;
                }
                if (terrainObjects[i].terrainType == TerrainType.custom && terrainObjects[i].customSettings.overridePriority * 10 < highestLayerPriority)
                {
                    highestLayerPriority = terrainObjects[i].customSettings.overridePriority;
                    layerId = i;
                }
                else if (walkableRegions[(int)terrainObjects[i].terrainType].priority * 10 < highestLayerPriority)
                {
                    highestLayerPriority = walkableRegions[(int)terrainObjects[i].terrainType].priority * 10;
                    layerId = i;
                }
            }
            for (int i2 = 0; i2 < walkableRegions.Length; i2++)
            {
                if (walkableRegions[i2].terrainType == terrainObjects[layerId].terrainType)
                {
                    _moveSpeed = moveSpeed / 100 * (100 - walkableRegions[i2].terrainPenalty);
                }
            }
        }
        rb.velocity = new Vector3(dir.x, rb.velocity.y, dir.z);
    }
}