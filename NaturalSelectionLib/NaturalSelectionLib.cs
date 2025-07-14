using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace NaturalSelectionLib
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class NaturalSelectionLib : BaseUnityPlugin
    {
        public static bool debugLibrary = false;
        public static bool debugSpam = false;
        public static ManualLogSource LibraryLogger = new ManualLogSource("NaturalSelectionLib");
        public static Dictionary<Type, List<EnemyAI>> globalEnemyLists = new Dictionary<Type, List<EnemyAI>>();

        public static string ReturnVersion()
        {
            return MyPluginInfo.PLUGIN_VERSION;
        }

        public static void UpdateListInsideDictionrary(Type instanceType, ref List<EnemyAI> list)
        {
            if (!globalEnemyLists.ContainsKey(instanceType))
            {
                globalEnemyLists.Add(instanceType, list);
                if (debugSpam && debugLibrary) LibraryLogger.LogInfo("/updateListInsideDictionary/ created new list for " + instanceType);
            }
            else
            {
                if (globalEnemyLists[instanceType].SequenceEqual(list))
                {
                    if (debugSpam && debugLibrary) LibraryLogger.LogInfo("/updateListInsideDictionary/ Sequence in " + instanceType + " is equal. Skipping.");
                    return;
                }
                globalEnemyLists[instanceType] = list;
                if (debugSpam && debugLibrary) LibraryLogger.LogInfo("/updateListInsideDictionary/ updating list for " + instanceType);
            }
        }
        static public void SetLibraryLoggers(ManualLogSource importLogger, bool spammyLogs = false, bool debuglibrary = false)
        {
            LibraryLogger = importLogger;
            debugSpam = spammyLogs;
            debugLibrary = debuglibrary;
        }

        /// <summary>
        /// Returns head with the name, IDs and additional variables of the source or the value of the source.
        /// </summary>
        /// <param name="source">
        /// Source object the head is made for. Supported types: EnemyAI, SandSpiderWebTrap, GrabbableObject, string.
        /// </param>
        /// <param name="shortFormat"></param>
        /// <returns></returns>

        public static string DebugStringHead(object? source, bool shortFormat = true)
        {
            string finalString = "";
            if (source != null)
            {
                string tempString = "";

                if (source is EnemyAI)
                {
                    EnemyAI enemyAI = (EnemyAI)source;
                    tempString = $"{enemyAI.enemyType.enemyName}|ID: {enemyAI.NetworkObjectId}|ThisEnemyIndex: {enemyAI.thisEnemyIndex}";
                    if (shortFormat) tempString = $"{enemyAI?.enemyType.enemyName}|ID: {enemyAI?.NetworkObjectId}";
                }
                else if (source is SandSpiderWebTrap)
                {
                    SandSpiderWebTrap webTrap = (SandSpiderWebTrap)source;
                    tempString = $"Spider web {webTrap.trapID}";
                    if (!shortFormat) tempString = $"Spider web {webTrap.trapID}, Owner {DebugStringHead(webTrap.mainScript)}";
                }
                else if (source is GrabbableObject)
                {
                    GrabbableObject grabbable = (GrabbableObject)source;
                    tempString = $"{grabbable.itemProperties.itemName}, ID: {grabbable.NetworkObjectId}";
                    if (!shortFormat) tempString = $"{grabbable.itemProperties.itemName}|ID: {grabbable.NetworkObjectId}|ItemID: {grabbable.itemProperties.itemId}";
                }
                else if (source is string) tempString = (string)source;
                else tempString = "Unknown source";

                finalString = $"({tempString})";
            }
            return finalString;
        }
        public static List<EnemyAI> GetCompleteList(EnemyAI instance, bool filterThemselves = true, int includeOrReturnTheDead = 0)
        {
            List<EnemyAI> tempList = new List<EnemyAI>(RoundManager.Instance.SpawnedEnemies);

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i] == instance)
                {
                    if (debugLibrary && debugSpam) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found itself in the list. Removing...");
                    tempList.Remove(tempList[i]);
                    continue;
                }
                if (tempList[i].GetType() == instance.GetType() && filterThemselves)
                {
                    if (debugLibrary && debugSpam) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found its type in the list. Removing...");
                    tempList.Remove(tempList[i]);
                    continue;
                }
                if (tempList[i].isEnemyDead)
                {
                    switch (includeOrReturnTheDead)
                    {
                        case 0:
                            {
                                if (debugLibrary && debugSpam) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Found dead enemy in the list. Removing...");
                                tempList.Remove(tempList[i]);
                                continue ;
                            }
                        case 1:
                            {
                                if (debugLibrary && debugSpam) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Found dead enemy in the list. Proceeding...");
                                continue;
                            }
                        case 2:
                            {
                                if (debugLibrary && debugSpam) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Found living enemy in the list. Removing..");
                                tempList.Remove(tempList[i]);
                                continue;;
                            }
                    }
                }
            }
            return tempList;
        }

        public static List<EnemyAI> GetNearbyEnemies(EnemyAI instance, float radius = 0f, Vector3? importEyePosition = null, int includeOrReturnTheDead = 0)
        {
            List<EnemyAI> tempList = new List<EnemyAI>();

            Vector3 eyePosition = instance.eye.position;
            if (importEyePosition != null)
            {
                eyePosition = importEyePosition.Value;
            }
            int mask = LayerMask.GetMask("Enemies");

            int num = Physics.OverlapSphereNonAlloc(eyePosition, radius, RoundManager.Instance.tempColliderResults, mask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < num; i++)
            {
                if (!RoundManager.Instance.tempColliderResults[i].gameObject.TryGetComponent<EnemyAICollisionDetect>(out EnemyAICollisionDetect AIcol))
                {
                    continue;
                }
                EnemyAI enemyAI = AIcol.mainScript;
                if (enemyAI == instance)
                {
                    continue;
                }
                if (!tempList.Contains(enemyAI))
                {
                    if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Enemy not found in imported enemy list! Skipping...");

                    switch (includeOrReturnTheDead)
                    {
                        case 0:
                        {
                            if (!enemyAI.isEnemyDead)
                            {
                                tempList.Add(enemyAI);
                                break;
                            }
                        break;
                        }
                        case 1:
                        {
                            tempList.Add(enemyAI);
                            break;
                        }
                        case 2:
                        {
                            if (enemyAI.isEnemyDead)
                            {
                                tempList.Add(enemyAI);
                                break;
                            }
                            break;
                        }
                    }
                }
            }
            return tempList;
        }
        public static void GetInsideOrOutsideEnemyList(ref List<EnemyAI> importEnemyList, EnemyAI instance)
        {
            foreach (EnemyAI enemy in importEnemyList.ToList())
            {
                if (enemy == instance || (enemy.isOutside != instance.isOutside))
                {
                    importEnemyList.Remove(enemy);
                    if (debugLibrary && debugSpam) LibraryLogger.LogDebug($"{DebugStringHead(instance)}/GetInsideOrOutsideEnemyList/ removed {DebugStringHead(enemy)}");
                }
            }
        }

        public static EnemyAI? FindClosestEnemy(ref List<EnemyAI> importEnemyList, EnemyAI? importClosestEnemy, EnemyAI __instance, bool includeTheDead = false)
        {            
            foreach (EnemyAI enemy in importEnemyList)
            {
                if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(__instance)}/FindClosestEnemy/ item {DebugStringHead(enemy)} inside importEnemyList. IsEnemyDead: {enemy.isEnemyDead}");
            }
            if (debugLibrary && importClosestEnemy != null) LibraryLogger.LogInfo($"{DebugStringHead(__instance)}/FindClosestEnemy/ {DebugStringHead(importClosestEnemy)} inside importClosestEnemy. IsEnemyDead: {importClosestEnemy.isEnemyDead}");
            if (importEnemyList.Count < 1)
            {
                if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(__instance)}importEnemyList is empty!");
                if (importClosestEnemy != null && importClosestEnemy.isEnemyDead)
                {
                    if (!includeTheDead)
                    {
                        if (debugLibrary && debugSpam) LibraryLogger.LogError($"{DebugStringHead(__instance)} {DebugStringHead(importClosestEnemy)} is dead and importEnemyList is empty! Setting importClosestEnemy to null...");
                        return null;
                    }
                    else
                    {
                        if (debugLibrary && debugSpam) LibraryLogger.LogInfo($"{DebugStringHead(__instance)} {DebugStringHead(importClosestEnemy)} is dead and importEnemyList is empty!");
                        return importClosestEnemy;
                    }
                }
            }
            for (int i = 0; i < importEnemyList.Count; i++)
            {
                if (importClosestEnemy == null)
                {
                    if (debugLibrary && debugSpam) LibraryLogger.LogInfo($"{DebugStringHead(__instance)} No enemy assigned. Assigning new closestEnemy...");

                    for (int j = i; j < importEnemyList.Count; j++)
                    {
                        if (importEnemyList[j].isEnemyDead && !includeTheDead)
                        {
                            if (debugLibrary && debugSpam) LibraryLogger.LogWarning($"{DebugStringHead(__instance)} Found dead enemy. Skipping...");
                            continue;
                        }
                        else
                        {
                            if (debugLibrary && debugSpam) LibraryLogger.LogInfo($"{DebugStringHead(__instance)} New closestEnemy found!");
                            importClosestEnemy = importEnemyList[j];
                            break;
                        }
                    }
                    continue;
                }
                if (importClosestEnemy.isEnemyDead)
                {
                    if (!includeTheDead)
                    {
                        if (debugLibrary && debugSpam) LibraryLogger.LogError($"{DebugStringHead(__instance)}, {DebugStringHead(importClosestEnemy)} is dead! Assigning new tempClosestEnemy from importEnemyList...");
                        importClosestEnemy = importEnemyList[i];
                        continue;
                    }
                    else
                    {
                        if (debugLibrary && debugSpam) LibraryLogger.LogInfo($"{DebugStringHead(__instance)} {DebugStringHead(importClosestEnemy)} is dead! The dead enemy will be included. ");
                    }
                }
                if (importClosestEnemy == importEnemyList[i])
                {
                    if (debugLibrary && debugSpam) LibraryLogger.LogWarning($"{DebugStringHead(__instance)} {DebugStringHead(importEnemyList[i])} is already assigned as closestEnemy");
                    continue;
                }
                if (importEnemyList[i] == null)
                {
                    if (debugLibrary) LibraryLogger.LogError($"{DebugStringHead(__instance)} Enemy not found! Skipping...");
                    continue;
                }
                if (Vector3.Distance(__instance.transform.position, importEnemyList[i].transform.position) < Vector3.Distance(__instance.transform.position, importClosestEnemy.transform.position))
                {
                    importClosestEnemy = importEnemyList[i];
                    if (debugLibrary && debugSpam) LibraryLogger.LogDebug(Vector3.Distance(__instance.transform.position, importEnemyList[i].transform.position) < Vector3.Distance(__instance.transform.position, importClosestEnemy.transform.position));
                    if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(__instance)} Assigned {DebugStringHead(importEnemyList[i])} as new closestEnemy. Distance: " + Vector3.Distance(__instance.transform.position, importClosestEnemy.transform.position));

                }
            }
            if (debugLibrary && debugSpam) LibraryLogger.LogWarning($"{DebugStringHead(__instance)} findClosestEnemy returning {DebugStringHead(importClosestEnemy)}");
            return importClosestEnemy;
        }
        public static void FilterEnemyList(ref List<EnemyAI> importEnemyList, List<string>? blacklist,EnemyAI instance, bool filterOutImmortal = true, bool filterTheSameType = true)
        {
            List<EnemyAI> tempList = new List<EnemyAI>(importEnemyList);
            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i] == instance)
                {
                    if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found itself in importEnemyList! Skipping...");
                    importEnemyList.Remove(tempList[i]);
                    continue;
                }
                if (filterTheSameType && tempList[i].GetType() == instance.GetType())
                {
                    if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found its own type in importEnemyList! Skipping...");
                    importEnemyList.Remove(tempList[i]);
                    continue;
                }
                try
                {
                    if (blacklist != null && tempList[i] != null && (blacklist.Contains(tempList[i].enemyType.enemyName) || blacklist.Contains(tempList[i].enemyType.name) || tempList[i].GetComponentInChildren<ScanNodeProperties>() != null && blacklist.Contains(tempList[i].GetComponentInChildren<ScanNodeProperties>().headerText)))
                    {
                        if (debugLibrary && blacklist.Contains(tempList[i].enemyType.enemyName)) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found blacklisted enemy in importEnemyList by EnemyType enemyName! Skipping...");
                        if (debugLibrary && blacklist.Contains(tempList[i].enemyType.name)) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found blacklisted enemy in importEnemyList by EnemyType name! Skipping...");
                        if (debugLibrary && blacklist.Contains(tempList[i].GetComponentInChildren<ScanNodeProperties>().headerText)) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found blacklisted enemy in importEnemyList by scan node headertext! Skipping...");
                        importEnemyList.Remove(tempList[i]);
                        continue;
                    }
                }
                catch (Exception exc)
                {
                    LibraryLogger.LogError($"{DebugStringHead(instance)} Something went wrong.");
                    LibraryLogger.LogError(blacklist);
                    LibraryLogger.LogError(tempList[i]);
                    LibraryLogger.LogError(tempList[i].enemyType.enemyName.ToUpper());
                    LibraryLogger.LogError(tempList[i].GetComponentInChildren<ScanNodeProperties>().headerText.ToUpper());
                    LibraryLogger.LogError(exc.ToString());
                }
                if (filterOutImmortal && !tempList[i].enemyType.canDie)
                {
                    if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Caught and filtered out immortal Enemy of type {tempList[i].GetType()}");
                    importEnemyList.Remove(tempList[i]);
                    continue;
                }
            }
        }


        static public void FilterEnemySizes(ref List<EnemyAI> importEnemyList, EnemySize[] enemySizes, EnemyAI instance,bool inverseToggle = false)
        {
            List<EnemyAI> tempList = new List<EnemyAI>(importEnemyList);
            for (int i = 0; i < tempList.Count; i++)
            {
                if (enemySizes != null && enemySizes.Length > 0 && (inverseToggle == false && enemySizes.Contains(tempList[i].enemyType.EnemySize) || inverseToggle == true && !enemySizes.Contains(tempList[i].enemyType.EnemySize)))
                {
                    if (debugLibrary) LibraryLogger.LogDebug($"{DebugStringHead(instance)} Enemy of size {tempList[i].enemyType.EnemySize} passed the filter. inverseToggle: {inverseToggle}");
                    //filteredList.Add(importEnemyList[i]);
                }
                else if (enemySizes != null && enemySizes.Length > 0)
                {
                    if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Caught and filtered out Enemy of size {tempList[i].enemyType.EnemySize}");
                    importEnemyList.Remove(tempList[i]);
                    continue;
                }
                if (enemySizes == null || enemySizes.Length < 1)
                {
                    if (debugLibrary && enemySizes != null && enemySizes.Length < 1) LibraryLogger.LogInfo($"{DebugStringHead(instance)} enemySizes is empty. Adding enemy of size {tempList[i].enemyType.EnemySize} by default");
                    if (debugLibrary && enemySizes == null) LibraryLogger.LogInfo($"{DebugStringHead(instance)} enemySizes is NULL. Adding enemy of size {tempList[i].enemyType.EnemySize} by default");
                    //filteredList.Add(importEnemyList[i]);
                }
            }
        }

        static public void FilterEnemySizes(ref Dictionary<EnemyAI, int> importEnemySizeDict, int[] enemySizes, EnemyAI instance, bool inverseToggle = false)
        {
            Dictionary<EnemyAI, int> tempList = new Dictionary<EnemyAI, int>(importEnemySizeDict);

            foreach (var keyValuePair in tempList)
            {
                if (enemySizes != null && enemySizes.Length > 0 && (inverseToggle == false && enemySizes.Contains(keyValuePair.Value) || inverseToggle == true && !enemySizes.Contains(keyValuePair.Value)))
                {
                    if (debugLibrary) LibraryLogger.LogDebug($"{DebugStringHead(instance)} Enemy of size {keyValuePair.Value} passed the filter. inverseToggle: {inverseToggle}");
                    //filteredList.Add(importEnemyList[i]);
                }
                else if (enemySizes != null && enemySizes.Length > 0)
                {
                    if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Caught and filtered out Enemy of size {keyValuePair.Value}");
                    importEnemySizeDict.Remove(keyValuePair.Key);
                    continue;
                }
                if (enemySizes == null || enemySizes.Length < 1)
                {
                    if (debugLibrary && enemySizes != null && enemySizes.Length < 1) LibraryLogger.LogInfo($"{DebugStringHead(instance)} enemySizes is empty. Adding enemy of size {keyValuePair.Value} by default");
                    if (debugLibrary && enemySizes == null) LibraryLogger.LogInfo($"{DebugStringHead(instance)} enemySizes is NULL. Adding enemy of size {keyValuePair.Value} by default");
                    //filteredList.Add(importEnemyList[i]);
                }
            }
        }

        static public Dictionary<EnemyAI, float> GetEnemiesInLOS(EnemyAI instance, ref List<EnemyAI> importEnemyList, float width = 45f, float importRange = 0, float proximityAwareness = -1, float importRadius = 0, Vector3? importEyePosition = null)
        {
            Dictionary<EnemyAI, float> tempDictionary = new Dictionary<EnemyAI, float>();
            float range = importRange;
            float radius = importRadius;
            Vector3 eyePosition = instance.eye.position;
            if (importEyePosition != null)
            {
                eyePosition = importEyePosition.Value;
            }
            int mask = LayerMask.GetMask("Enemies");

            if (instance.isOutside && !instance.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(importRange, 0, 30);
            }
            if (radius <= 0)
            {
                radius = range * 2;
            }
            int num = Physics.OverlapSphereNonAlloc(eyePosition, radius, RoundManager.Instance.tempColliderResults, mask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < num; i++)
            {
                if (!RoundManager.Instance.tempColliderResults[i].gameObject.TryGetComponent<EnemyAICollisionDetect>(out EnemyAICollisionDetect AIcol))
                {
                    continue;
                }
                EnemyAI enemyAI = AIcol.mainScript;
                if (enemyAI == instance)
                {
                    continue;
                }
                if (!importEnemyList.Contains(enemyAI))
                {
                    if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Enemy not found in imported enemy list! Skipping...");
                    continue;
                }
                Vector3 position = enemyAI.transform.position;
                if (Vector3.Distance(position, instance.eye.position) < range && !Physics.Linecast(instance.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    if (instance.CheckLineOfSightForPosition(position, width, (int)range, proximityAwareness, instance.eye))
                    {
                        if (!tempDictionary.ContainsKey(enemyAI))
                        {
                            tempDictionary.Add(enemyAI, Vector3.Distance(instance.transform.position, position));
                            if (debugLibrary && debugSpam) LibraryLogger.LogDebug($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Added {enemyAI} to tempDictionary");
                        }
                        if (tempDictionary.ContainsKey(enemyAI) && debugLibrary && debugSpam)
                        {
                            if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} /GetEnemiesInLOS/: {enemyAI} is already in tempDictionary");
                        }
                    }
                }
            }
            if (tempDictionary.Count > 1)
            {
                tempDictionary.OrderBy(value => tempDictionary.Values).Reverse();
                if (debugLibrary)
                {
                    foreach (KeyValuePair<EnemyAI, float> enemy in tempDictionary)
                    {
                        if (debugLibrary && debugSpam) LibraryLogger.LogDebug($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Final list: {enemy.Key}, range: {enemy.Value}");
                    }
                }
            }
            return tempDictionary;
        }

        static public Dictionary<EnemyAI, float> GetEnemiesInLOS(EnemyAI instance, float width = 45f, float importRange = 0, float proximityAwareness = -1, float importRadius = 0, Vector3? importEyePosition = null)
        {
            Dictionary<EnemyAI, float> tempDictionary = new Dictionary<EnemyAI, float>();
            float range = importRange;
            float radius = importRadius;
            Vector3 eyePosition = instance.eye.position;

            if (importEyePosition != null)
            {
                eyePosition = importEyePosition.Value;
            }
            int mask = LayerMask.GetMask("Enemies");

            if (instance.isOutside && !instance.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(importRange, 0, 30);
            }
            if (radius <= 0)
            {
                radius = range * 2;
            }

            int num = Physics.OverlapSphereNonAlloc(eyePosition, radius, RoundManager.Instance.tempColliderResults, mask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < num; i++)
            {
                if (RoundManager.Instance.tempColliderResults[i].gameObject.TryGetComponent<EnemyAICollisionDetect>(out EnemyAICollisionDetect AIcol))
                {
                    EnemyAI enemyAI = AIcol.mainScript;
                    if (enemyAI == instance)
                    {
                        continue;
                    }
                    Vector3 position = enemyAI.transform.position;
                    if (Vector3.Distance(position, instance.eye.position) < range && !Physics.Linecast(instance.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                    {
                        if (instance.CheckLineOfSightForPosition(position, width, (int)range, proximityAwareness, instance.eye))
                        {
                            if (!tempDictionary.ContainsKey(enemyAI))
                            {
                                tempDictionary.Add(enemyAI, Vector3.Distance(instance.transform.position, position));
                                if (debugLibrary && debugSpam) LibraryLogger.LogDebug($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Added {enemyAI} to tempDictionary");
                            }
                            if (tempDictionary.ContainsKey(enemyAI) && debugLibrary && debugSpam)
                            {
                                if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} /GetEnemiesInLOS/: {enemyAI} is already in tempDictionary");
                            }
                        }
                    }
                }
            }
            if (tempDictionary.Count > 1)
            {
                tempDictionary.OrderBy(value => tempDictionary.Values).Reverse();
                if (debugLibrary)
                {
                    foreach (KeyValuePair<EnemyAI, float> enemy in tempDictionary)
                    {
                        if (debugLibrary && debugSpam) LibraryLogger.LogDebug($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Final list: {enemy.Key}, range: {enemy.Value}");
                    }
                }
            }
            return tempDictionary;
        }
    }
}
