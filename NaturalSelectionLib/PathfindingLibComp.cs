using PathfindingLib.Jobs;
using PathfindingLib.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using NaturalSelectionLib;
using BepInEx.Logging;

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
    public float pathDistance = -777.77f;
    public bool validpath = false;
    public bool isCalculating = false;
    NavMeshAgent agent;
    Vector3 targetDestination;

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
            pathDistance = -777.77f;
            validpath = false;
            isCalculating = false;
            JobPools.ReleaseFindPathJob(pooledJob);
            yield break;
        }
    }
}
