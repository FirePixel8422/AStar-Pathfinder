using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private PathFinding pf;
    private Agent agent;

    public LayerMask[] walkableLayers;
    public float[] movementPenalty;

    public Transform target;
    private Vector3 oldTargetPos;

    public float moveSpeed = 6;
    public float distToStopMoving = 2;

    private void Start()
    {
        pf = FindObjectOfType<PathFinding>();
        agent = this;
    }

    private void Update()
    {
        if(target == null || pf.grid == null || pf.grid.MaxSize == 0)
        {
            return;
        }
        float updateRange = pf.targetMoveDistanceForPathUpdate * Mathf.Clamp((Vector3.Distance(transform.position, target.position) - pf.ignoredBaseUpdateRange) / pf.rangeForFasterPathUpdateSpeed * pf.grid.nodeSize, 1, float.MaxValue);
        if (Vector3.Distance(oldTargetPos, target.position) > updateRange || (Vector3.Distance(transform.position, target.position) < pf.targetMoveDistanceForPathUpdate * 2 && Vector3.Distance(oldTargetPos, target.position) > 0.01f))
        {
            pf.FindPath(transform.position, target.position, agent);
            oldTargetPos = target.position;
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
        float _moveSpeed = moveSpeed;
        if (walkableLayerIndex != -1)
        {
            _moveSpeed = moveSpeed * movementPenalty[walkableLayerIndex] / 100;
        }

        if (path != null && path.Count != 0 && Vector3.Distance(target.position, transform.position) > distToStopMoving)
        {
            Vector3 targetPos = path[0].worldPos; 
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);

            if(Vector3.Distance(transform.position, targetPos) < pf.grid.nodeSize * 1.25f && path.Count > 0)
            {
                path.RemoveAt(0);
            }
        }
    }
    public List<Node> path = new List<Node>();

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
    }
}
