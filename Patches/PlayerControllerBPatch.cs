using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;
using HeartAttack;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeartAttack.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static bool hasHeartAttackOccurred = false;
        private static bool hasFearAttackOccurred = false;
        private static float heartAttackChance = 10f;
        private static float trembleChance = 13f;
        private static float playerTrembleMoveSpeed = 0.35f;
        private static float playerTrembleClimbSpeed = 2f;
        private static bool hasSetVals = false; 

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void heartAttack(PlayerControllerB __instance)
        {
            if (__instance == GameNetworkManager.Instance.localPlayerController)
            {
                if (!hasSetVals)
                {
                        heartAttackChance = Plugin.BoundConfig.heartAttackChance.Value;
                        trembleChance = Plugin.BoundConfig.trembleChance.Value;
                        playerTrembleMoveSpeed = Plugin.BoundConfig.playerTrembleMoveSpeed.Value;
                        playerTrembleClimbSpeed = Plugin.BoundConfig.playerTrembleClimbSpeed.Value;
                        hasSetVals = true;
                }
                int playerCount = StartOfRound
                    .Instance.allPlayerScripts.Where(x => x.isPlayerControlled)
                    .Count();

                if (playerCount > 1)
                {                
                    if (StartOfRound.Instance.fearLevel > 0.5f)
                    {               
                        if (!hasHeartAttackOccurred && !__instance.isPlayerAlone)
                        {
                            triggerHeartAttack(__instance);
                        }
                        else if (!hasFearAttackOccurred)
                        {
                            tremblingFear(__instance);
                        }

                    }
                    else if (StartOfRound.Instance.fearLevel < 0.5f)
                    {
                        hasHeartAttackOccurred = false;
                        hasFearAttackOccurred = false;
                    }
                }
                else
                {
                    // if there is some better way to do this please let me know (I mean this for the entire code).
                    if (StartOfRound.Instance.fearLevel > 0.5f)
                    {
                        if (!hasHeartAttackOccurred)
                        {
                            triggerHeartAttack(__instance);
                        }
                        else if (!hasFearAttackOccurred)
                        {
                            tremblingFear(__instance);
                        }
                    }
                    else if (StartOfRound.Instance.fearLevel < 0.5f)
                    {
                        hasHeartAttackOccurred = false;
                        hasFearAttackOccurred = false;
                    }
                }
            }
            else
            {
                Plugin.Logger.LogWarning("18");
                return;
            }
        }

        private static void triggerHeartAttack(PlayerControllerB player)
        {
                float heartAttackValue = UnityEngine.Random.Range(0f, 100f);
                float chance = heartAttackChance;
                Plugin.Logger.LogDebug($"Heart Attack Value: {heartAttackValue}");

                if (heartAttackValue <= chance)
                {
                    player.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown, 0, Vector3.zero);
                    Plugin.Logger.LogDebug("HEART ATTACK");
                }

                hasHeartAttackOccurred = true;
        }

        private static void tremblingFear(PlayerControllerB player)
        {
                float fearValue = UnityEngine.Random.Range(0f, 100f);
                float chance = trembleChance;
                Plugin.Logger.LogDebug($"Fear Value: {fearValue}");
                if (fearValue <= chance)
                {
                    player.StartCoroutine(applyTheFear(player));

                    Plugin.Logger.LogDebug("FEAR!");
                }

                hasFearAttackOccurred = true;
        }

        private static IEnumerator applyTheFear(PlayerControllerB player)
        {
                float originalMovementSpeed = player.movementSpeed;
                float originalClimbSpeed = player.climbSpeed;
                float originalLookSensitivity = player.lookSensitivity;

                player.movementSpeed = playerTrembleMoveSpeed;
                player.climbSpeed = playerTrembleClimbSpeed;
                //player.lookSensitivity = 0.05f; This does not work.
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);

                yield return new WaitForSeconds(2.5f);
                    player.movementSpeed = originalMovementSpeed;
                    player.climbSpeed = originalClimbSpeed;
                    //player.lookSensitivity = originalLookSensitivity;
                
        }
    }
}
