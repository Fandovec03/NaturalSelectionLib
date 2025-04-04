using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

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

        static public void UpdateListInsideDictionrary(Type instanceType, List<EnemyAI> list)
        {
            List<Type> enemyTypes = new List<Type>();
            if (!globalEnemyLists.ContainsKey(instanceType))
            {
                globalEnemyLists.Add(instanceType, new List<EnemyAI>(list));
                if (debugSpam && debugLibrary) LibraryLogger.LogInfo("/updateListInsideDictionary/ created new list for " + instanceType);
            }
            else
            {
                globalEnemyLists[instanceType] = new List<EnemyAI>(list);
                if (debugSpam && debugLibrary) LibraryLogger.LogInfo("/updateListInsideDictionary/ updating list for " + instanceType);
            }
            if (!enemyTypes.Contains(instanceType)) enemyTypes.Add(instanceType);
        }
        static public void LibrarySetup(ManualLogSource importLogger, bool spammyLogs = false, bool debuglibrary = false)
        {
            LibraryLogger = importLogger;
            debugSpam = spammyLogs;
            debugLibrary = debuglibrary;
        }

        public static string DebugStringHead(EnemyAI? __instance)
        {
            if (!__instance) return "Unknown instance: ";
            else return $"({__instance?.name}|ID: {__instance?.NetworkObjectId}|LocalID: {__instance?.GetInstanceID()}|ThisEnemyIndex: {__instance?.thisEnemyIndex}) ";
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

        
        public static List<EnemyAI> GetInsideOrOutsideEnemyList(List<EnemyAI> importEnemyList, EnemyAI instance)
        {
            List<EnemyAI> tempEnemyList = new List<EnemyAI>();

            foreach (EnemyAI enemy in importEnemyList)
            {
                if (enemy != instance && (enemy.isOutside && instance.isOutside || !enemy.isOutside && !instance.isOutside))
                {
                    tempEnemyList.Add(enemy);
                    if (debugLibrary && debugSpam) LibraryLogger.LogDebug($"{DebugStringHead(instance)}/GetInsideOrOutsideEnemyList/ addded {DebugStringHead(enemy)}");
                }
            }
            return tempEnemyList;
        }

        public static EnemyAI? FindClosestEnemy(List<EnemyAI> importEnemyList, EnemyAI? importClosestEnemy, EnemyAI __instance, bool includeTheDead = false)
        {            
            foreach (EnemyAI enemy in importEnemyList)
            {
                if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(__instance)}/FindClosestEnemy/ item {DebugStringHead(enemy)} inside importEnemyList. IsEnemyDead: {enemy.isEnemyDead}");
            }
            if (debugLibrary && importClosestEnemy != null) LibraryLogger.LogInfo($"{DebugStringHead(__instance)}/FindClosestEnemy/ {DebugStringHead(importClosestEnemy)} inside importClosestEnemy. IsEnemyDead: {importClosestEnemy.isEnemyDead}");
            //if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(__instance)}/FindClosestEnemy/ {DebugStringHead(__instance)} inside instance.");
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
                    //importEnemyList.RemoveAt(i);
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
        public static List<EnemyAI> FilterEnemyList(List<EnemyAI> importEnemyList, List<Type>? targetTypes, List<string>? blacklist,EnemyAI instance, bool inverseToggle = false, bool filterOutImmortal = true)
        {
            List<EnemyAI> filteredList = new List<EnemyAI>();

            for (int i = 0; i < importEnemyList.Count; i++)
            {
                if (importEnemyList[i] == instance)
                {
                    if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found itself in importEnemyList! Skipping...");
                    continue;
                }
                try
                {
                    if (blacklist != null && importEnemyList[i] != null && (blacklist.Contains(importEnemyList[i].enemyType.enemyName.ToUpper()) || blacklist.Contains(importEnemyList[i].enemyType.name.ToUpper()) || importEnemyList[i].GetComponentInChildren<ScanNodeProperties>() != null && blacklist.Contains(importEnemyList[i].GetComponentInChildren<ScanNodeProperties>().headerText.ToUpper())))
                    {
                        if (debugLibrary && blacklist.Contains(importEnemyList[i].enemyType.enemyName.ToUpper())) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found blacklisted enemy in importEnemyList by EnemyType enemyName! Skipping...");
                        if (debugLibrary && blacklist.Contains(importEnemyList[i].enemyType.name.ToUpper())) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found blacklisted enemy in importEnemyList by EnemyType name! Skipping...");
                        if (debugLibrary && blacklist.Contains(importEnemyList[i].GetComponentInChildren<ScanNodeProperties>().headerText.ToUpper())) LibraryLogger.LogWarning($"{DebugStringHead(instance)} Found blacklisted enemy in importEnemyList by scan node headertext! Skipping...");
                        continue;
                    }
                }
                catch (Exception exc)
                {
                    LibraryLogger.LogError($"{DebugStringHead(instance)} Something went wrong.");
                    LibraryLogger.LogError(blacklist);
                    LibraryLogger.LogError(importEnemyList[i]);
                    LibraryLogger.LogError(importEnemyList[i].enemyType.enemyName.ToUpper());
                    LibraryLogger.LogError(importEnemyList[i].GetComponentInChildren<ScanNodeProperties>().headerText.ToUpper());
                    LibraryLogger.LogError(exc.ToString());
                }
                if (filterOutImmortal && !importEnemyList[i].enemyType.canDie)
                {
                    if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Caught and filtered out immortal Enemy of type {importEnemyList[i].GetType()}");
                    continue;
                }

                if (targetTypes != null && targetTypes.Count > 0 && (inverseToggle == false && targetTypes.Contains(importEnemyList[i].GetType()) || inverseToggle == true && !targetTypes.Contains(importEnemyList[i].GetType())))
                {
                    if (debugLibrary) LibraryLogger.LogDebug($"{DebugStringHead(instance)} Enemy of type {importEnemyList[i].GetType()} passed the filter. inverseToggle: {inverseToggle}");
                    filteredList.Add(importEnemyList[i]);
                }
                else if (targetTypes != null && targetTypes.Count > 0)
                {
                    if (debugLibrary) LibraryLogger.LogInfo($"{DebugStringHead(instance)} Caught and filtered out Enemy of type {importEnemyList[i].GetType()}");
                    continue;
                }
                if (targetTypes == null || targetTypes.Count < 1)
                {
                    if (debugLibrary && targetTypes != null && targetTypes.Count < 1) LibraryLogger.LogInfo($"{DebugStringHead(instance)} TargetTypes is empty. Adding enemy of type {importEnemyList[i].GetType()} by default");
                    if (debugLibrary && targetTypes == null) LibraryLogger.LogInfo($"{DebugStringHead(instance)} TargetTypes is NULL. Adding enemy of type {importEnemyList[i].GetType()} by default");
                    filteredList.Add(importEnemyList[i]);
                }
            }
            return filteredList;
        }


        static public Dictionary<EnemyAI, float> GetEnemiesInLOS(EnemyAI instance, List<EnemyAI> importEnemyList, float width = 45f, float importRange = 0, float proximityAwareness = -1)
        {
            List<EnemyAI> tempList = new List<EnemyAI>(importEnemyList);
            Dictionary<EnemyAI, float> tempDictionary = new Dictionary<EnemyAI, float>();
            float range = importRange;

            if (instance.isOutside && !instance.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(importRange, 0, 30);
            }
            if (tempList != null && tempList.Count > 0)
            {
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i] == null)
                    {
                        if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Enemy not found! Removing from tempList");
                        tempList.Remove(tempList[i]);
                        continue;
                    }
                    Vector3 position = tempList[i].transform.position;
                    if (Vector3.Distance(position, instance.eye.position) < range && !Physics.Linecast(instance.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                    {
                        if (instance.CheckLineOfSightForPosition(position, width, (int)range, proximityAwareness, instance.eye))
                        {
                            if (!tempDictionary.ContainsKey(tempList[i]))
                            {
                                tempDictionary.Add(tempList[i], Vector3.Distance(instance.transform.position, position));
                                if (debugLibrary && debugSpam) LibraryLogger.LogDebug($"{DebugStringHead(instance)} /GetEnemiesInLOS/: Added {tempList[i]} to tempDictionary");
                            }
                            if (tempDictionary.ContainsKey(tempList[i]) && debugLibrary && debugSpam)
                            {
                                if (debugLibrary) LibraryLogger.LogWarning($"{DebugStringHead(instance)} /GetEnemiesInLOS/: {tempList[i]} is already in tempDictionary");
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
