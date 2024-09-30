using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using System.Collections;
using UnityEngine.InputSystem;

namespace HeartAttack.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static bool hasHeartAttackOccurred = false;
        private static bool hasFearAttackOccurred = false;
        [HarmonyPatch("Update")]
        [HarmonyPostfix]

        static void heartAttack(PlayerControllerB __instance)
        {
            
            if ((__instance == GameNetworkManager.Instance.localPlayerController))
            {
                int playerCount = StartOfRound.Instance.allPlayerScripts.Where(x => x.isPlayerControlled).Count();
                //Plugin.Logger.LogDebug($"FearLevel: {StartOfRound.Instance.fearLevel}");
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
                return;
            }
        }

        private static void triggerHeartAttack(PlayerControllerB player)
        {
            float heartAttackValue = UnityEngine.Random.Range(0f, 100f);
            float chance = 10f;
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
            float chance = 13f; 
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

            player.movementSpeed = 0.35f;
            player.climbSpeed = 2f;
            //player.lookSensitivity = 0.05f; This does not work.
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);

            yield return new WaitForSeconds(2.5f);

            player.movementSpeed = originalMovementSpeed;
            player.climbSpeed = originalClimbSpeed;
            //player.lookSensitivity = originalLookSensitivity;
        }
    }
}
