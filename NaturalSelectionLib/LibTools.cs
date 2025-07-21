using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaturalSelectionLib.Comp;


namespace NaturalSelectionLib.Tools;
internal abstract class PathfindingCalculator : IDisposable
{
    internal static PathfindingCalculator Create(EnemyAI instance, List<Vector3> destinations)
    {
        if (Chainloader.PluginInfos.ContainsKey("Zaggy1024.PathfindingLib") && !NaturalSelectionLib.usePathfindingLib)
        {
            return PathfindingLibHelper.ReturnPathfindingLibalculator(instance, destinations);
        }
        else return new PathfindingCalculatorAsync(instance, destinations);
    }

    public abstract bool CalculationnStatus(int index, out float pathLengthResult, out bool validPath);

    public virtual void Dispose()
    {

    }
}
class PathfindingCalculatorAsync(EnemyAI instance, List<Vector3> destinations) : PathfindingCalculator
{
    NavMeshPath path = new NavMeshPath();
    public override bool CalculationnStatus(int index, out float pathLengthResult, out bool validPath)
    {
        pathLengthResult = -1f;
        validPath = false;

        instance.agent.CalculatePath(destinations[index], path);
        if (path.status != NavMeshPathStatus.PathComplete) return true;

        pathLengthResult = 0f;
        for(int i = 1; i < path.corners.Length; i++)
        {
            validPath = true;
            pathLengthResult += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return true;
    }

}
