using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class EnemyAI : MonoBehaviour
{
    public bool pause = false;

    private GridManager grid;
    public Pathfinding pf;

    public LayerMask[] walkableLayers;
    public float[] movementPenalty;

    public float moveSpeed = 6;
    public float distToStopMoving = 2;
    public int nodeIndex;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
        if(pf == null)
        {
            pf = FindObjectOfType<Pathfinding>();
        }
    }

    private void Update()
    {
        if(pause == true)
        {
            return;
        }
        if (grid.recalculated == true)
        {
            grid.recalculated = false;
            nodeIndex = 0;
        }
        int walkableLayerIndex = -1;
        for (int i = 0; i < walkableLayers.Length; i++)
        {
            if (Physics.CheckSphere(transform.position, 0.5f, walkableLayers[i]))
            {
                walkableLayerIndex = i;
                break;
            }
        }
        float _moveSpeed = 0;
        if (walkableLayerIndex != -1)
        {
            _moveSpeed = moveSpeed * movementPenalty[walkableLayerIndex] / 100;
        }

        if (grid.path != null && grid.path.Count != 0 && Vector3.Distance(pf.target.position, transform.position) > distToStopMoving)
        {
            Vector3 targetPos = grid.path[nodeIndex].worldPos; 
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);

            if(Vector3.Distance(transform.position, targetPos) < grid.nodeSize && nodeIndex + 1 < grid.path.Count)
            {
                nodeIndex += 1;
            }
        }
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying && pause == false)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
    }
}
