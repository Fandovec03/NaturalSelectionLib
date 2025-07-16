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

namespace NaturalSelectionLib.Comp
{
    public class PathFindingHandler
    {
        public float pathDistance = -777.77f;
        public bool validpath = false;
        public bool isCalculating = false;
        public IEnumerator CalculatePathCoroutine(NavMeshAgent agent, Vector3 targetDestination)
        {
            while (!TryCalculatePath(agent, targetDestination, out validpath, out pathDistance))
            {
                isCalculating = true;
                yield return null;
            }
        }
        
        public bool TryCalculatePath(NavMeshAgent agent, Vector3 targetDestination, out bool validpath, out float pathDistance)
        {
            PathfindingCalculation pathfindingCalculation = new PathfindingCalculation(agent, targetDestination);
            return pathfindingCalculation.CalculatePath(out validpath, out pathDistance);
        }
    }

    internal class PathfindingCalculation : PathfindingOper
    {
        PooledFindPathJob pooledJob;

        public PathfindingCalculation(NavMeshAgent agent, Vector3 targetDestination)
        {
            pooledJob = JobPools.GetFindPathJob();
            pooledJob.Job.Initialize(agent.GetPathOrigin(), targetDestination, agent);
            JobHandle previousJobHandle = pooledJob.Job.ScheduleByRef();
        }

        public override void Dispose()
        {
            if (pooledJob != null)
            {
                NaturalSelectionLib.LibraryLogger.LogMessage("Job disposed");
                JobPools.ReleaseFindPathJob(pooledJob);
            }
        }

        public override bool HasDisposed()
        {
            return pooledJob == null;
        }


        public bool CalculatePath(out bool validpath, out float PathLength)
        {
            PathLength = -777.77f;
            validpath = false;
            NaturalSelectionLib.LibraryLogger.LogMessage("PathfindingLib present. Switching to using PathfindingLib");

            if (pooledJob.Job.GetStatus().GetResult() == PathQueryStatus.InProgress)
            {
                NaturalSelectionLib.LibraryLogger.LogError("PathfindingLib calculations is in progress");
                PathLength = -777.77f;
                return false;
            }

            NaturalSelectionLib.LibraryLogger.LogMessage("Calculated path.");
            if (pooledJob.Job.GetStatus().GetResult() == PathQueryStatus.Success)
            {
                PathLength = pooledJob.Job.GetPathLength();
                validpath = true;
            }
            NaturalSelectionLib.LibraryLogger.LogMessage("Released PathfindingLib job.");
            return true;
        }
    }

    public abstract class PathfindingOper : IDisposable
    {
       public abstract void Dispose();

        public abstract bool HasDisposed();

        ~PathfindingOper()
        {
            Dispose();
        } 
    }
}
