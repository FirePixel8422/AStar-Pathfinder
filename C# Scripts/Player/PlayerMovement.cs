using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Percentage speed reduction \n85 = -85% speed on selected terrain")]
    public TerrainType[] walkableRegions;

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
            int terrainIndex = -1;
            for (int i = 0; i < walkableRegions.Length; i++)
            {
                if (Physics.RaycastAll(transform.position, Vector3.down, 3, walkableRegions[i].terrainLayer, QueryTriggerInteraction.Collide).Length != 0)
                {
                    terrainIndex = i;
                    break;
                }
            }
            float _moveSpeed = moveSpeed;
            if (terrainIndex != -1)
            {
                _moveSpeed = moveSpeed * (100 - walkableRegions[terrainIndex].terrainPenalty) / 100;
            }
            dir *= -_moveSpeed;
        }
        rb.velocity = new Vector3(dir.x, rb.velocity.y, dir.z);
    }
}