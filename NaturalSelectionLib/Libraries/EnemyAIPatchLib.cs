﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace NaturalSelectionLib.Libraries
{
    [HarmonyPatch(typeof(EnemyAI))]
    class EnemyAIPatchLib
    {
        static List<EnemyAI> enemyList = new List<EnemyAI>();
        static float refreshCDtime = 1f;
        static bool debugUnspecified = false;
        static bool debugSpam = false;

        public void SetSpammyLogs(bool spammyLogs = false)
        {
            debugSpam = spammyLogs;
        }
        public void SetUnspecifiedLogs(bool Unspecified = false)
        {
            debugUnspecified = Unspecified;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePostfixPatch(EnemyAI __instance)
        {
            refreshCDtime -= Time.deltaTime;

            if (refreshCDtime <= 0)
            {
                foreach (EnemyAI enemy in RoundManager.Instance.SpawnedEnemies)
                {
                    if (enemyList.Contains(enemy) && enemy.isEnemyDead == false)
                    {
                        if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(__instance) + "Found Duplicate " + enemy.gameObject.name + ", ID: " + enemy.GetInstanceID());
                        continue;
                    }
                    if (enemyList.Contains(enemy) && enemy.isEnemyDead == true)
                    {
                        enemyList.Remove(enemy);
                        if (debugUnspecified) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(__instance) + "Found and removed dead Enemy " + enemy.gameObject.name + ", ID:  " + enemy.GetInstanceID() + "on List.");
                        continue;
                    }
                    if (!enemyList.Contains(enemy) && enemy.isEnemyDead == false && enemy.name != __instance.name)
                    {
                        enemyList.Add(enemy);
                        if (debugUnspecified) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(__instance) + "Added " + enemy.gameObject.name + " detected in List. Instance: " + enemy.GetInstanceID());
                        continue;
                    }
                }

                for (int i = 0; i < enemyList.Count; i++)
                {
                    if (__instance != null && enemyList.Count > 0)
                    {
                        if (enemyList[i] == null)
                        {
                            if (debugUnspecified) NaturalSelectionLib.Logger.LogError(DebugStringHead(__instance) + "Detected null enemy in the list. Removing...");
                            enemyList.RemoveAt(i);
                        }
                        else if (enemyList[i] != null)
                        {
                            if (__instance.CheckLineOfSightForPosition(enemyList[i].transform.position, 360f, 60, 1f, __instance.eye))
                            {
                                if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(__instance) + "LOS check: Have LOS on " + enemyList[i] + ", ID: " + enemyList[i].GetInstanceID());
                            }
                        }
                    }
                }
                refreshCDtime = 1f;
            }
        }

        public static string DebugStringHead(EnemyAI? __instance)
        {
            if (!__instance) return "Unknown instance: ";
            else return __instance?.name + ", ID: " + __instance?.GetInstanceID() + ": ";
        }
        public static List<EnemyAI> GetCompleteList(EnemyAI instance, bool FilterThemselves = true)
        {
            List<EnemyAI> tempList = new List<EnemyAI>();

            for (int i = 0; i < enemyList.Count; i++)
            {
                if (enemyList[i] == instance)
                {
                    if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(instance) + " Found itself in the list. Skipping...");
                    //tempList.RemoveAt(i);
                    continue;
                }
                if (enemyList[i].GetType() == instance.GetType() && FilterThemselves)
                {
                    if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(instance) + " Found its type in the list. Skipping...");
                    //enemyList.RemoveAt(i);
                }
                else
                {
                    tempList.Add(enemyList[i]);
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
                    if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(instance) + " Added " + DebugStringHead(enemy) + "...");
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
                    if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(instance) + " Added " + DebugStringHead(enemy) + "...");
                }
            }
            return insideEnemies;
        }

        public static EnemyAI? findClosestEnemy(List<EnemyAI> importEnemyList, EnemyAI? importClosestEnemy, EnemyAI __instance)
        {
            EnemyAI? tempClosestEnemy = importClosestEnemy;

            if (importEnemyList.Count < 1)
            {
                if (debugUnspecified) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(__instance) + "importEnemyList is empty!");
            }
            else for (int i = 0; i < importEnemyList.Count; i++)
                {
                    if (importEnemyList.Contains(__instance))
                    {
                        if (debugUnspecified) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(__instance) + "Found itself in the findClosestEnemy method! Skipping...");
                        //tempEnemyList.Remove(__instance);
                        continue;
                    }
                    if (tempClosestEnemy == null)
                    {
                        if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogInfo(DebugStringHead(__instance) + "No enemy assigned. Assigning " + importEnemyList[i] + ", ID: " + importEnemyList[i].GetInstanceID() + " as new closestEnemy.");
                        tempClosestEnemy = importEnemyList[i];
                        continue;
                    }
                    if (tempClosestEnemy == importEnemyList[i])
                    {
                        if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(__instance) + importEnemyList[i] + ", ID: " + importEnemyList[i].GetInstanceID() + " is already assigned as closestEnemy");
                        continue;
                    }
                    if (importEnemyList[i] == null)
                    {
                        if (debugUnspecified)
                        {
                            NaturalSelectionLib.Logger.LogError(DebugStringHead(__instance) + "Enemy not found!");
                        }
                        //importEnemyList.RemoveAt(i);
                    }
                    else if (Vector3.Distance(__instance.transform.position, importEnemyList[i].transform.position) < Vector3.Distance(__instance.transform.position, tempClosestEnemy.transform.position))
                    {
                        tempClosestEnemy = importEnemyList[i];
                        if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogDebug(Vector3.Distance(__instance.transform.position, importEnemyList[i].transform.position) < Vector3.Distance(__instance.transform.position, tempClosestEnemy.transform.position));
                        if (debugUnspecified) NaturalSelectionLib.Logger.LogInfo(DebugStringHead(__instance) + "Assigned " + importEnemyList[i] + ", ID: " + importEnemyList[i].GetInstanceID() + " as new closestEnemy. Distance: " + Vector3.Distance(__instance.transform.position, tempClosestEnemy.transform.position));

                    }
                }
            return tempClosestEnemy;
        }
        public static List<EnemyAI> filterEnemyList(List<EnemyAI> importEnemyList, List<Type> targetTypes, EnemyAI instance, bool inverseToggle = false)
        {
            List<EnemyAI> filteredList = new List<EnemyAI>();

            for (int i = 0; i < importEnemyList.Count; i++)
            {
                if (importEnemyList[i] == instance)
                {
                    if (debugUnspecified) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(instance) + "Found itself in importEnemyList! Skipping...");
                    //tempEnemyList.RemoveAt(i);
                    continue;
                }
                if (inverseToggle == false && targetTypes.Contains(importEnemyList[i].GetType()) || inverseToggle == true && !targetTypes.Contains(importEnemyList[i].GetType()))
                {
                    if (debugUnspecified) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(instance) + "Enemy of type " + importEnemyList[i].GetType() + " passed the filter. inverseToggle: " + inverseToggle);

                    filteredList.Add(importEnemyList[i]);
                }
                else if (debugUnspecified && debugSpam)
                {
                    if (debugUnspecified) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(instance) + "Caught and filtered out Enemy of type " + enemyList[i].GetType());
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
                if (!enemy.isEnemyDead && enemy != null)
                {
                    if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogInfo(DebugStringHead(instance) + "/GetEnemiesInLOS/: Added " + enemy + " to tempList");
                    tempList.Add(enemy);
                }
            }
            if (tempList != null && tempList.Count > 0)
            {
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i] == null)
                    {
                        if (debugUnspecified) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(instance) + "/GetEnemiesInLOS/: Enemy not found! Removing from tempList");
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
                                    if (debugUnspecified) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(instance) + "/GetEnemiesInLOS/: Added " + tempList[i] + " to tempDictionary");
                            }
                            if (tempDictionary.ContainsKey(tempList[i]) && debugUnspecified && debugSpam)
                            {
                                if (debugUnspecified) NaturalSelectionLib.Logger.LogWarning(DebugStringHead(instance) + "/GetEnemiesInLOS/:" + tempList[i] + " is already in tempDictionary");
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
                        if (debugUnspecified && debugSpam) NaturalSelectionLib.Logger.LogDebug(DebugStringHead(instance) + "/GetEnemiesInLOS/: Final list: " + tempDictionary[enemy.Key]);
                    }
                }
            }
            return tempDictionary;
        }
    }
    
    public class ReversePatchEnemy : EnemyAI
    {
        public override void Update()
        {
            base.Update();
        }
    }
}
