using PathfindingLib.Jobs;
using PathfindingLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace NaturalSelectionLib.Comp
{
    internal class PathfindingLibComp
    {
        public static bool Execute(NavMeshAgent agent, Vector3 targetDestination, ref bool validpath, out float PathLength)
        {
            NaturalSelectionLib.LibraryLogger.LogMessage("PathfindingLib present. Switching to using PathfindingLib");
            var pooledJob = JobPools.GetFindPathJob();
            pooledJob.Job.Initialize(agent.GetPathOrigin(), targetDestination, agent);
            JobHandle previousJobHandle = pooledJob.Job.ScheduleByRef();

            if (pooledJob.Job.GetStatus().GetResult() != PathQueryStatus.InProgress)
            {
                NaturalSelectionLib.LibraryLogger.LogMessage("Calculated path.");
                if (pooledJob.Job.GetStatus().GetResult() == PathQueryStatus.Success)
                {
                    PathLength = pooledJob.Job.GetPathLength();
                    validpath = true;
                }
                NaturalSelectionLib.LibraryLogger.LogMessage("Released PathfindingLib job.");
                JobPools.ReleaseFindPathJob(pooledJob);
            }
            else
            {
                NaturalSelectionLib.LibraryLogger.LogError("PathfindingLib calculations is in progress");
                PathLength = -777.77f;
                return false;
            }
            PathLength = -777.77f;
            return true;
        }
    }
}
