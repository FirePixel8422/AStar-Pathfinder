using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCore : MonoBehaviour
{
    private PathFinding pf;
    private SlopeManager sm;
    [HideInInspector]
    public AgentData agent;

    public LayerMask walkableObjectLayer;

    [Header("Percentage speed reduction \n85 = -85% speed on selected terrain")]
    public TerrainLayerType[] walkableRegions;

    public List<Node> path = new List<Node>();

    public TargetInfo target;
    private Vector3 oldTargetPos;

    public float moveSpeed = 6;
    public float distToStopMoving = 2;

    private int oldTargetSlopeIndex;
    private int goingToSlope;

    private Vector3 slopeEndPos;
    [HideInInspector]
    public bool slopeTransition;

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
            Debug.LogWarning("No target: assign or fix with code on: " + gameObject.name);
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
        if ((((Vector3.Distance(oldTargetPos, targetPos) > updateRange || (Vector3.Distance(agentPos, targetPos) < pf.targetMoveDistanceForPathUpdate * 2 && Vector3.Distance(oldTargetPos, targetPos) > 0.01f)) && slopeIndex == targetSlopeIndex) || (oldTargetSlopeIndex != targetSlopeIndex && goingToSlope == 0)) && slopeTransition == false)
        {
            if (targetSlopeIndex > slopeIndex)
            {
                targetPos = sm.slopes[slopeIndex].slopeStart.position;
                slopeEndPos = sm.slopes[slopeIndex].slopeEnd.position;
                oldTargetSlopeIndex = targetSlopeIndex;
                goingToSlope = 1;
            }
            else if (targetSlopeIndex < slopeIndex)
            {
                targetPos = sm.slopes[slopeIndex - 1].slopeEnd.position;
                slopeEndPos = sm.slopes[slopeIndex - 1].slopeStart.position;
                oldTargetSlopeIndex = targetSlopeIndex;
                goingToSlope = - 1;
            }
            else
            {
                oldTargetPos = targetPos;
            }
            pf.FindPath(agentPos, targetPos, agent);
        }

        float _moveSpeed = 0;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2, walkableObjectLayer))
        {
            TerrainObject floor = hit.collider.gameObject.GetComponent<TerrainObject>();
            if (floor != null)
            {
                _moveSpeed = moveSpeed / 100 * (100 - walkableRegions[(int)floor.terrainType].terrainPenalty);
            }
        }

        if (path.Count != 0 && Vector3.Distance(targetPos, agentPos) > distToStopMoving)
        {
            Vector3 pathTargetPos = new Vector3(path[0].worldPos.x, 0, path[0].worldPos.z);
            Vector3 newPos = Vector3.MoveTowards(agentPos, pathTargetPos, _moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
            agentPos = new Vector3(transform.position.x, 0, transform.position.z);

            if (Vector3.Distance(agentPos, pathTargetPos) < pf.grid.gridFloors[slopeIndex].nodeSize * 1.25f && path.Count > 0)
            {
                path.RemoveAt(0);
                if (path.Count == 0 && goingToSlope != 0)
                {
                    if(slopeTransition == false)
                    {
                        slopeTransition = true;
                        agent.slopeIndex += goingToSlope;
                        pf.FindPath(agentPos, slopeEndPos, agent);
                    }
                    else
                    {
                        slopeTransition = false;
                        goingToSlope = 0;
                        if (slopeIndex == targetSlopeIndex)
                        {
                            pf.FindPath(agentPos, target.transform.position, agent);
                        }
                    }
                }
            }
        }
    }
}
