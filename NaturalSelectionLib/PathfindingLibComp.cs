using NaturalSelectionLib.Tools;
using PathfindingLib.API.SmartPathfinding;
using PathfindingLib.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace NaturalSelectionLib.Comp
{

    static class PathfindingLibHelper
    {
        public static PathfindingCalculatorAsyncPathfindingLib ReturnPathfindingLibalculator(EnemyAI instance, List<Vector3> destinations)
        {
            return new PathfindingCalculatorAsyncPathfindingLib(instance, destinations);
        }
    }

    class PathfindingCalculatorAsyncPathfindingLib : PathfindingCalculator
    {
        SmartPathTask pathfindingTask = new SmartPathTask();
        internal PathfindingCalculatorAsyncPathfindingLib(EnemyAI instance, List<Vector3> destinations)
        {
            pathfindingTask.StartPathTask(instance.agent, instance.agent.GetPathOrigin(), destinations, 0);
        }
        public override bool CalculationnStatus(int index, out float pathLengthResult, out bool validPath)
        {
            pathLengthResult = -1f;
            validPath = false;
            if (!pathfindingTask.IsResultReady(index)) return true;

            if (pathfindingTask.PathSucceeded(index))
            {
                pathLengthResult = pathfindingTask.GetPathLength(index);
                validPath = pathfindingTask.PathSucceeded(index);
                return true;
            }
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            pathfindingTask.Dispose();
        }
    }
}
