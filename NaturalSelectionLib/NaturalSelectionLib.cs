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
        public static bool debugUnspecified = false;
        public static bool debugSpam = false;
        public static ManualLogSource LibraryLogger = new ManualLogSource("NaturalSelectionLib");
        public static Dictionary<Type, List<EnemyAI>> globalEnemyLists = new Dictionary<Type, List<EnemyAI>>();

        static public void UpdateListInsideDictionrary(Type instanceType, List<EnemyAI> list)
        {
            List<Type> enemyTypes = new List<Type>();
            if (!globalEnemyLists.ContainsKey(instanceType))
            {
                globalEnemyLists.Add(instanceType, new List<EnemyAI>());
                if (debugSpam && debugUnspecified)LibraryLogger.LogInfo("/updateListInsideDictionary/ created new list for " + instanceType);
            }
            else
            {
                globalEnemyLists[instanceType] = list;
                if (debugSpam && debugUnspecified)LibraryLogger.LogInfo("/updateListInsideDictionary/ updating list for " + instanceType);
            }
            if (!enemyTypes.Contains(instanceType)) enemyTypes.Add(instanceType);
        }
        static public void LibrarySetup(ManualLogSource importLogger, bool spammyLogs = false, bool Unspecified = false)
        {
            LibraryLogger = importLogger;
            debugSpam = spammyLogs;
            debugUnspecified = Unspecified;
        }

        public static string DebugStringHead(EnemyAI? __instance)
        {
            if (!__instance) return "Unknown instance: ";
            else return __instance?.name + ", ID: " + __instance?.GetInstanceID() + ": ";
        }
        public static List<EnemyAI> GetCompleteList(EnemyAI instance, bool filterThemselves = true, int includeOrReturnTheDead = 0)
        {
            List<EnemyAI> tempList = RoundManager.Instance.SpawnedEnemies;

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i] == instance)
                {
                    if (debugUnspecified && debugSpam) LibraryLogger.LogWarning(DebugStringHead(instance) + " Found itself in the list. Removing...");
                    tempList.RemoveAt(i);
                    continue;
                }
                if (tempList[i].GetType() == instance.GetType() && filterThemselves)
                {
                    if (debugUnspecified && debugSpam) LibraryLogger.LogWarning(DebugStringHead(instance) + " Found its type in the list. Removing...");
                    tempList.RemoveAt(i);
                    continue;
                }
                if (tempList[i].isEnemyDead)
                {
                    switch (includeOrReturnTheDead)
                    {
                        case 0:
                            {
                                if (debugUnspecified && debugSpam) LibraryLogger.LogInfo(DebugStringHead(instance) + " Found dead enemy in the list. Removing...");
                                tempList.RemoveAt(i);
                                continue ;
                            }
                        case 1:
                            {
                                if (debugUnspecified && debugSpam) LibraryLogger.LogInfo(DebugStringHead(instance) + " Found dead enemy in the list. Proceeding...");
                                continue;
                            }
                        case 2:
                            {
                                if (debugUnspecified && debugSpam) LibraryLogger.LogInfo(DebugStringHead(instance) + " Found living enemy in the list. Removing..");
                                tempList.RemoveAt(i);
                                continue;;
                            }
                    }
                }
            }
            return tempList;
        }

        public static List<EnemyAI> GetOutsideEnemyList(List<EnemyAI> importEnemyList, EnemyAI instance)
        {
            List<EnemyAI> outsideEnemies = new List<EnemyAI>();

            foreach (EnemyAI enemy in importEnemyList)
            {
                if (enemy.isOutside && enemy != instance)
                {
                    outsideEnemies.Add(enemy);
                    if (debugUnspecified && debugSpam) LibraryLogger.LogDebug(DebugStringHead(instance) + " Added " + DebugStringHead(enemy) + "...");
                }
            }

            return outsideEnemies;
        }

        public static List<EnemyAI> GetInsideEnemyList(List<EnemyAI> importEnemyList, EnemyAI instance)
        {
            List<EnemyAI> insideEnemies = new List<EnemyAI>();

            foreach (EnemyAI enemy in importEnemyList)
            {
                if (!enemy.isOutside && enemy != instance)
                {
                    insideEnemies.Add(enemy);
                    if (debugUnspecified && debugSpam) LibraryLogger.LogDebug(DebugStringHead(instance) + " Added " + DebugStringHead(enemy) + "...");
                }
            }
            return insideEnemies;
        }

        public static EnemyAI? FindClosestEnemy(List<EnemyAI> importEnemyList, EnemyAI? importClosestEnemy, EnemyAI __instance, bool includeTheDead = false)
        {
            EnemyAI? tempClosestEnemy = importClosestEnemy;
           
            foreach (EnemyAI enemy in importEnemyList)
            {
                if (debugUnspecified) LibraryLogger.LogInfo(DebugStringHead(__instance) + "/FindClosestEnemy/ item " + DebugStringHead(enemy) + " inside importEnemyList. IsEnemyDead: " + enemy.isEnemyDead);
            }
            if (debugUnspecified && importClosestEnemy != null) LibraryLogger.LogInfo(DebugStringHead(__instance) + "/FindClosestEnemy/ " + DebugStringHead(importClosestEnemy) + " inside importClosestEnemy. IsEnemyDead: " + importClosestEnemy.isEnemyDead);
            if (debugUnspecified) LibraryLogger.LogInfo(DebugStringHead(__instance) + "/FindClosestEnemy/ " + DebugStringHead(__instance) + " inside instance.");
            if (importEnemyList.Count < 1)
            {
                if (debugUnspecified) LibraryLogger.LogWarning(DebugStringHead(__instance) + "importEnemyList is empty!");
                if (importClosestEnemy != null && importClosestEnemy.isEnemyDead)
                {
                    if (!includeTheDead)
                    {
                        if (debugUnspecified && debugSpam) LibraryLogger.LogError(DebugStringHead(__instance) + DebugStringHead(importClosestEnemy) + " is dead and importEnemyList is empty! Setting importClosestEnemy to null...");
                    }
                    else
                    {
                        if (debugUnspecified && debugSpam) LibraryLogger.LogInfo(DebugStringHead(__instance) + DebugStringHead(importClosestEnemy) + " is dead and importEnemyList is empty! The dead enemy will be included. ");
                    }
                }
                return null;
            }
            for (int i = 0; i < importEnemyList.Count; i++)
            {
                if (tempClosestEnemy == null)
                {
                    if (debugUnspecified && debugSpam) LibraryLogger.LogInfo(DebugStringHead(__instance) + "No enemy assigned. Assigning new closestEnemy....");

                    for (int j = i; j < importEnemyList.Count; j++)
                    {
                        if (importEnemyList[j].isEnemyDead && !includeTheDead)
                        {
                            if (debugUnspecified && debugSpam) LibraryLogger.LogWarning(DebugStringHead(__instance) + "Found dead enemy. Skipping....");
                            continue;
                        }
                        else
                        {
                            if (debugUnspecified && debugSpam) LibraryLogger.LogInfo(DebugStringHead(__instance) + "New closestEnemy found!");
                            tempClosestEnemy = importEnemyList[j];
                            break;
                        }
                    }
                    continue;
                }
                if (tempClosestEnemy.isEnemyDead)
                {
                    if (!includeTheDead)
                    {
                        if (debugUnspecified && debugSpam) LibraryLogger.LogError(DebugStringHead(__instance) + ", " + DebugStringHead(__instance) + " is dead! Assigning new tempClosestEnemy from importEnemyList...");
                        tempClosestEnemy = importEnemyList[i];
                        continue;
                    }
                    else
                    {
                        if (debugUnspecified && debugSpam) LibraryLogger.LogInfo(DebugStringHead(__instance) + DebugStringHead(importClosestEnemy) + " is dead! The dead enemy will be included. ");
                    }
                }
                if (tempClosestEnemy == importEnemyList[i])
                {
                    if (debugUnspecified && debugSpam) LibraryLogger.LogWarning(DebugStringHead(__instance) + importEnemyList[i] + ", ID: " + importEnemyList[i].GetInstanceID() + " is already assigned as closestEnemy");
                    continue;
                }
                if (importEnemyList[i] == null)
                {
                    if (debugUnspecified) LibraryLogger.LogError(DebugStringHead(__instance) + "Enemy not found! Skipping...");
                    continue;
                    //importEnemyList.RemoveAt(i);
                }
                if (Vector3.Distance(__instance.transform.position, importEnemyList[i].transform.position) < Vector3.Distance(__instance.transform.position, tempClosestEnemy.transform.position))
                {
                    tempClosestEnemy = importEnemyList[i];
                    if (debugUnspecified && debugSpam) LibraryLogger.LogDebug(Vector3.Distance(__instance.transform.position, importEnemyList[i].transform.position) < Vector3.Distance(__instance.transform.position, tempClosestEnemy.transform.position));
                    if (debugUnspecified) LibraryLogger.LogInfo(DebugStringHead(__instance) + "Assigned " + importEnemyList[i] + ", ID: " + importEnemyList[i].GetInstanceID() + " as new closestEnemy. Distance: " + Vector3.Distance(__instance.transform.position, tempClosestEnemy.transform.position));

                }
            }
            if (debugUnspecified && debugSpam) LibraryLogger.LogWarning(DebugStringHead(__instance) + "findClosestEnemy returning " + DebugStringHead(tempClosestEnemy));
            return tempClosestEnemy;
        }
        public static List<EnemyAI> FilterEnemyList(List<EnemyAI> importEnemyList, List<Type>? targetTypes, List<string>? blacklist,EnemyAI instance, bool inverseToggle = false, bool filterOutImmortal = true)
        {
            List<EnemyAI> filteredList = new List<EnemyAI>();

            for (int i = 0; i < importEnemyList.Count; i++)
            {
                if (importEnemyList[i] == instance)
                {
                    if (debugUnspecified) LibraryLogger.LogWarning(DebugStringHead(instance) + "Found itself in importEnemyList! Skipping...");
                    //tempEnemyList.RemoveAt(i);
                    continue;
                }
                if (blacklist != null && blacklist.Count > 0 && (blacklist.Contains(importEnemyList[i].enemyType.enemyName.ToUpper()) || blacklist.Contains(importEnemyList[i].GetComponentInChildren<ScanNodeProperties>().headerText.ToUpper())))
                {
                    if (debugUnspecified && blacklist.Contains(importEnemyList[i].enemyType.enemyName.ToUpper())) LibraryLogger.LogWarning(DebugStringHead(instance) + "Found blacklisted enemy in importEnemyList by EnemyType name! Skipping...");
                    if (debugUnspecified && blacklist.Contains(importEnemyList[i].GetComponentInChildren<ScanNodeProperties>().headerText.ToUpper())) LibraryLogger.LogWarning(DebugStringHead(instance) + "Found blacklisted enemy in importEnemyList by scan node headertext! Skipping...");
                    continue;
                }
                if (targetTypes != null && targetTypes.Count > 0 && (inverseToggle == false && targetTypes.Contains(importEnemyList[i].GetType()) || inverseToggle == true && !targetTypes.Contains(importEnemyList[i].GetType())))
                {
                    if (debugUnspecified) LibraryLogger.LogDebug(DebugStringHead(instance) + "Enemy of type " + importEnemyList[i].GetType() + " passed the filter. inverseToggle: " + inverseToggle);

                    filteredList.Add(importEnemyList[i]);
                }
                else if (targetTypes != null && targetTypes.Count > 0 && debugUnspecified && debugSpam)
                {
                    if (debugUnspecified) LibraryLogger.LogInfo(DebugStringHead(instance) + "Caught and filtered out Enemy of type " + importEnemyList[i].GetType());
                }
                if (filterOutImmortal && !importEnemyList[i].enemyType.canDie)
                {
                    if (debugUnspecified) LibraryLogger.LogInfo(DebugStringHead(instance) + "Caught and filtered out immortal Enemy of type " + importEnemyList[i].GetType());
                    continue;
                }
                if (targetTypes == null || targetTypes.Count < 1)
                {
                    filteredList.Add(importEnemyList[i]);
                    if (debugUnspecified && targetTypes != null && targetTypes.Count < 1) LibraryLogger.LogInfo(DebugStringHead(instance) + "TargetTypes is empty.  Added enemy of type " + importEnemyList[i].GetType() + " to filteredList by default");
                    if (debugUnspecified && targetTypes == null) LibraryLogger.LogInfo(DebugStringHead(instance) + "TargetTypes is NULL.  Added enemy of type " + importEnemyList[i].GetType() + " to filteredList by default");
                }
            }
            return filteredList;
        }


        static public Dictionary<EnemyAI, float> GetEnemiesInLOS(EnemyAI instance, List<EnemyAI> importEnemyList, float width = 45f, int importRange = 0, float proximityAwareness = -1)
        {
            List<EnemyAI> tempList = new List<EnemyAI>();
            Dictionary<EnemyAI, float> tempDictionary = new Dictionary<EnemyAI, float>();
            float range = (float)importRange;

            if (instance.isOutside && !instance.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(importRange, 0, 30);
            }
            foreach (EnemyAI enemy in importEnemyList)
            {
                if (enemy != null)
                {
                    if (debugUnspecified && debugSpam)
                    {
                        if (enemy.isEnemyDead) LibraryLogger.LogInfo(DebugStringHead(instance) + "/GetEnemiesInLOS/: " + enemy + " is dead");
                        LibraryLogger.LogInfo(DebugStringHead(instance) + "/GetEnemiesInLOS/: Added " + enemy + " to tempList");
                    }
                    tempList.Add(enemy);
                }
            }
            if (tempList != null && tempList.Count > 0)
            {
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i] == null)
                    {
                        if (debugUnspecified) LibraryLogger.LogWarning(DebugStringHead(instance) + "/GetEnemiesInLOS/: Enemy not found! Removing from tempList");
                        tempList.RemoveAt(i);
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
                                if (debugUnspecified && debugSpam)
                                    if (debugUnspecified) LibraryLogger.LogDebug(DebugStringHead(instance) + "/GetEnemiesInLOS/: Added " + tempList[i] + " to tempDictionary");
                            }
                            if (tempDictionary.ContainsKey(tempList[i]) && debugUnspecified && debugSpam)
                            {
                                if (debugUnspecified) LibraryLogger.LogWarning(DebugStringHead(instance) + "/GetEnemiesInLOS/:" + tempList[i] + " is already in tempDictionary");
                            }
                        }
                    }
                }
            }
            if (tempDictionary.Count > 1)
            {
                tempDictionary.OrderBy(value => tempDictionary.Values);
                if (debugUnspecified)
                {
                    foreach (KeyValuePair<EnemyAI, float> enemy in tempDictionary)
                    {
                        if (debugUnspecified && debugSpam) LibraryLogger.LogDebug(DebugStringHead(instance) + "/GetEnemiesInLOS/: Final list: " + tempDictionary[enemy.Key]);
                    }
                }
            }
            return tempDictionary;
        }
    }
}
