using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCore : MonoBehaviour
{
    private PathFinding pf;
    private SlopeManager sm;
    [HideInInspector]
    public AgentData agent;

    [Header("Percentage speed reduction \n85 = -85% speed on selected terrain")]
    public TerrainType[] walkableRegions;

    private int oldTargetSlopeIndex;
    public int goingToSlope;

    public TargetInfo target;
    private Vector3 oldTargetPos;

    public float moveSpeed = 6;
    public float distToStopMoving = 2;

    private void Start()
    {
        pf = FindObjectOfType<PathFinding>();
        sm = FindObjectOfType<SlopeManager>();
        agent = GetComponent<AgentData>();
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }
        int slopeIndex = agent.slopeIndex;
        if (pf.grid.gridFloors.Length <= slopeIndex)
        {
            Debug.LogError("slopeIndex bigger then amount of gridFloors in grid");
            return;
        }
        int targetSlopeIndex = target.slopeIndex;
        Vector3 agentPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = new Vector3(target.transform.position.x, 0, target.transform.position.z);

        float updateRange = pf.targetMoveDistanceForPathUpdate * Mathf.Clamp((Vector3.Distance(agentPos, targetPos) - pf.ignoredBaseUpdateRange) / pf.rangeForFasterPathUpdateSpeed * pf.grid.gridFloors[slopeIndex].nodeSize, 1, float.MaxValue);
        if (((Vector3.Distance(oldTargetPos, targetPos) > updateRange || (Vector3.Distance(agentPos, targetPos) < pf.targetMoveDistanceForPathUpdate * 2 && Vector3.Distance(oldTargetPos, targetPos) > 0.01f)) && slopeIndex == targetSlopeIndex) || (oldTargetSlopeIndex != targetSlopeIndex && goingToSlope == 0))
        {
            if (targetSlopeIndex > slopeIndex)
            {
                targetPos = sm.slopes[slopeIndex].slopeStart.position;
                oldTargetSlopeIndex = targetSlopeIndex;
                goingToSlope = 1;
            }
            else if (targetSlopeIndex < slopeIndex)
            {
                targetPos = sm.slopes[slopeIndex - 1].slopeEnd.position;
                oldTargetSlopeIndex = targetSlopeIndex;
                goingToSlope = - 1;
            }
            else
            {
                oldTargetPos = targetPos;
            }
            pf.FindPath(agentPos, targetPos, agent);
        }

        int terrainIndex = -1;
        Ray ray = new Ray(transform.position, Vector3.down);
        for (int i = 0; i < walkableRegions.Length; i++)
        {
            if (Physics.Raycast(ray, 2, walkableRegions[i].terrainLayer))
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

        if (path.Count != 0 && Vector3.Distance(targetPos, agentPos) > distToStopMoving)
        {
            Vector3 pathTargetPos = new Vector3(path[0].worldPos.x, 0, path[0].worldPos.z);
            Vector3 newPos = Vector3.MoveTowards(agentPos, pathTargetPos, _moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);

            if (Vector3.Distance(agentPos, pathTargetPos) < pf.grid.gridFloors[slopeIndex].nodeSize * 1.25f && path.Count > 0)
            {
                path.RemoveAt(0);
                if (path.Count == 0 && goingToSlope != 0)
                {
                    agent.slopeIndex += goingToSlope;
                    goingToSlope = 0;
                    if(slopeIndex == targetSlopeIndex)
                    {
                        pf.FindPath(agentPos, target.transform.position, agent);
                    }
                }
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
