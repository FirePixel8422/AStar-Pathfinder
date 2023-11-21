using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public LayerMask walkableObjectLayer;

    [Header("Percentage speed reduction \n85 = -85% speed on selected terrain")]
    public TerrainLayerType[] walkableRegions;

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
        if (dir != Vector3.zero)
        {
            float _moveSpeed = 0;
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2, walkableObjectLayer))
            {
                TerrainObject floor = hit.collider.gameObject.GetComponent<TerrainObject>();
                if (floor != null)
                {
                    _moveSpeed = moveSpeed / 100 * (100 - walkableRegions[(int)floor.terrainType].terrainPenalty);
                }
            }
            dir *= -_moveSpeed;
        }
        rb.velocity = new Vector3(dir.x, rb.velocity.y, dir.z);
    }
}