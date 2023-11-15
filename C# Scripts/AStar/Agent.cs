using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private PathFinding pf;
    private SlopeManager sm;
    private Agent agent;

    public LayerMask[] walkableLayers;
    public float[] movementPenalty;

    public int slopeIndex;

    public Transform target;
    private Vector3 oldTargetPos;

    public float moveSpeed = 6;
    public float distToStopMoving = 2;

    private void Start()
    {
        pf = FindObjectOfType<PathFinding>();
        sm = FindObjectOfType<SlopeManager>();
        agent = this;
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }
        Vector3 agentPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = target.position;

        float updateRange = pf.targetMoveDistanceForPathUpdate * Mathf.Clamp((Vector3.Distance(agentPos, target.position) - pf.ignoredBaseUpdateRange) / pf.rangeForFasterPathUpdateSpeed * pf.grid.gridFloors[slopeIndex].nodeSize, 1, float.MaxValue);
        if (Vector3.Distance(oldTargetPos, target.position) > updateRange || (Vector3.Distance(agentPos, target.position) < pf.targetMoveDistanceForPathUpdate * 2 && Vector3.Distance(oldTargetPos, target.position) > 0.01f))
        {
            if (sm.targetSlopeIndex > agent.slopeIndex)
            {
                targetPos = sm.slopes[agent.slopeIndex].slopeStart.position;
            }
            if (sm.targetSlopeIndex < agent.slopeIndex)
            {
                targetPos = sm.slopes[agent.slopeIndex].slopeEnd.position;
            }
            pf.FindPath(agentPos, targetPos, agent);
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

        if (path.Count != 0 && Vector3.Distance(targetPos, agentPos) > distToStopMoving)
        {
            Vector3 pathTargetPos = new Vector3(path[0].worldPos.x, 0, path[0].worldPos.z);
            Vector3 newPos = Vector3.MoveTowards(agentPos, pathTargetPos, _moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);

            if (Vector3.Distance(agentPos, pathTargetPos) < pf.grid.gridFloors[slopeIndex].nodeSize * 1.25f && path.Count > 0)
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
