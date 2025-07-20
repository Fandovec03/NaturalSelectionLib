using BepInEx.Bootstrap;
using PathfindingLib.Jobs;
using PathfindingLib.Utilities;
using System.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using static UnityEngine.UI.GridLayoutGroup;

namespace NaturalSelectionLib.Comp;
public class PathFindingHandler
{
    public PathFindingHandler(NavMeshAgent agent, Vector3 targetDestination)
    {
        this.agent = agent;
        this.targetDestination = targetDestination;
    }

    PooledFindPathJob pooledJob = JobPools.GetFindPathJob();
    JobHandle previousJobHandle;
    public float pathDistance = -1f;
    public bool validpath = false;
    public bool isCalculating = false;
    NavMeshAgent agent;
    Vector3 targetDestination;
    NavMeshPath path = new NavMeshPath();

    public IEnumerator CalculatePathCoroutine()
    {
        pooledJob.Job.Initialize(agent.GetPathOrigin(), targetDestination, agent);
        previousJobHandle = pooledJob.Job.ScheduleByRef();
        while (pooledJob.Job.GetStatus().GetResult() == PathQueryStatus.InProgress)
        {
            isCalculating = true;
            yield return null;
        }

        if (pooledJob.Job.GetStatus().GetResult() == PathQueryStatus.Success)
        {
            pathDistance = pooledJob.Job.GetPathLength();
            validpath = true;
            isCalculating = false;
            JobPools.ReleaseFindPathJob(pooledJob);
            yield break;
        }
        else
        {
            pathDistance = -1f;
            validpath = false;
            isCalculating = false;
            JobPools.ReleaseFindPathJob(pooledJob);
            yield break;
        }
    }
}
