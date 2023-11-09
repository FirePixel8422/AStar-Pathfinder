using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    public LayerMask[] walkableLayers;
    public float[] movementPenalty;

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
        int walkableLayerIndex = -1;
        for (int i = 0; i < walkableLayers.Length; i++)
        {
            if(Physics.CheckSphere(transform.position, 0.5f, walkableLayers[i]))
            {
                walkableLayerIndex = i;
                break;
            }
        }
        float _moveSpeed = moveSpeed;
        if (walkableLayerIndex != -1)
        {
            _moveSpeed = moveSpeed * movementPenalty[walkableLayerIndex] / 100;
        }

        rb.velocity = -dir.normalized * _moveSpeed;
    }
}