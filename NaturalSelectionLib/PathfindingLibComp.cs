using BepInEx.Bootstrap;
using PathfindingLib.API.SmartPathfinding;
using PathfindingLib.Jobs;
using PathfindingLib.Utilities;
using System;
using System.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using static UnityEngine.UI.GridLayoutGroup;

namespace NaturalSelectionLib.Comp;
public static class PathfindingLibHandler
{
    //PooledFindPathJob pooledJob = JobPools.GetFindPathJob();
    //JobHandle previousJobHandle;
    //public float pathDistance = -1f;
    //public bool validpath = false;
    //public bool isCalculating = false;
    //NavMeshAgent agent;
    //Vector3 targetDestination;
    //NavMeshPath path = new NavMeshPath();

    public static bool CalculatePathCoroutine(NavMeshAgent agent, Vector3 targetDestination, out bool validPath, out float pathDistance)
    {
        validPath = false;
        pathDistance = -1f;
        if (!agent.isActiveAndEnabled || !agent.enabled) return false;

        SmartPathTask pathfindingTask = new SmartPathTask();
        pathfindingTask.StartPathTask(agent, agent.GetPathOrigin(), targetDestination, 0);
        while (!pathfindingTask.IsResultReady(0))
        {
            return false;
        }
        pathDistance = pathfindingTask.GetPathLength(0);
        validPath = pathfindingTask.PathSucceeded(0);
        return true;
        /*
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
        }*/
    }
}
