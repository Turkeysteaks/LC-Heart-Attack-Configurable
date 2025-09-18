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
        private static bool hasAdrenalineOccurred = false;

        private static bool heartAttackWhenAlone = false;

        private static bool trembling = false;
        private static bool adrenaline = false;

        private static float minFearToTrigger = 0.5f;
        private static float maxFearToReset = 0.25f;
        
        private static float heartAttackChance = 10f;
        
        private static float trembleChance = 13f;
        private static bool trembleInstant = true;
        private static float trembleMoveSpeed = 0.35f;
        private static float trembleClimbSpeed = 2f;
        private static float trembleTime = 2.5f;

        private static float adrenalineChance = 10f;
        private static bool adrenalineInstant = false;
        private static float adrenalineMoveSpeed = 1.5f;
        private static float adrenalineClimbSpeed = 4f;
        private static float adrenalineTime = 2.5f;
        private static float adrenalineFOV = 72f;
        private static float adrenalineDrunkness = 5f;
        
        private static bool trembleAfterAdrenaline = false;
        
        private static bool hasSetVals = false; 

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void HeartAttack(PlayerControllerB __instance)
        {
            if (!(__instance == GameNetworkManager.Instance.localPlayerController))
            {
                return;
                
            }
            if (!hasSetVals)
            {
                heartAttackChance = Plugin.BoundConfig.heartAttackChance.Value;
                trembleChance = Plugin.BoundConfig.trembleChance.Value;
                adrenalineChance = Plugin.BoundConfig.adrenalineChance.Value;
                minFearToTrigger = Plugin.BoundConfig.minFearToTrigger.Value;
                maxFearToReset = Plugin.BoundConfig.maxFearToReset.Value;

                heartAttackWhenAlone = Plugin.BoundConfig.heartAttackWhenAlone.Value;
                
                trembleInstant = Plugin.BoundConfig.trembleInstant.Value;
                trembleMoveSpeed = Plugin.BoundConfig.playerTrembleMoveSpeed.Value;
                trembleClimbSpeed = Plugin.BoundConfig.playerTrembleClimbSpeed.Value;
                trembleTime = Plugin.BoundConfig.playerTrembleTime.Value;
                
                adrenalineInstant = Plugin.BoundConfig.adrenalineInstant.Value;
                adrenalineMoveSpeed = Plugin.BoundConfig.adrenalineMoveSpeed.Value;
                adrenalineClimbSpeed = Plugin.BoundConfig.adrenalineClimbSpeed.Value;
                adrenalineTime = Plugin.BoundConfig.adrenalineTime.Value;
                adrenalineFOV = Plugin.BoundConfig.adrenalineFOV.Value;
                adrenalineDrunkness = Plugin.BoundConfig.adrenalineDrunkness.Value;
                trembleAfterAdrenaline = Plugin.BoundConfig.trembleAfterAdrenaline.Value;
                
                hasSetVals = true;
            }
            int playerCount = StartOfRound
                .Instance.allPlayerScripts
                .Count(x => x.isPlayerControlled);

            if (StartOfRound.Instance.fearLevel >= minFearToTrigger)
            {
                if (!hasHeartAttackOccurred && (playerCount <= 1 || !__instance.isPlayerAlone || heartAttackWhenAlone))
                {
                    TriggerHeartAttack(__instance);
                }
                else if (!hasFearAttackOccurred)
                {
                    TriggerTremblingFear(__instance);
                }
                else if (!hasAdrenalineOccurred)
                {
                    TriggerAdrenaline(__instance);
                }
            }
            else if (StartOfRound.Instance.fearLevel < maxFearToReset)
            {
                hasHeartAttackOccurred = false;
                hasFearAttackOccurred = false;
                hasAdrenalineOccurred = false;
            }
            
        }

        private static void TriggerHeartAttack(PlayerControllerB player)
        {
                float triggerValue = UnityEngine.Random.Range(0f, 100f);
                float chance = heartAttackChance;
                Plugin.Logger.LogDebug($"Heart Attack Trigger Value: {triggerValue}");

                if (triggerValue <= chance)
                {
                    player.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown, 0, Vector3.zero);
                    if (CoronerCompatibility.enabled)
                    {
                        CoronerCompatibility.CoronerRegister();
                        CoronerCompatibility.CoronerSetCauseOfDeathHeartAttack(player);
                    }
                    Plugin.Logger.LogDebug("HEART ATTACK");
                }

                hasHeartAttackOccurred = true;
        }

        private static void TriggerTremblingFear(PlayerControllerB player, bool force = false)
        {
                float triggerValue = UnityEngine.Random.Range(0f, 100f);
                float chance = trembleChance;
                Plugin.Logger.LogDebug($"Tremble Trigger Value: {triggerValue}");
                if ((!trembling && !adrenaline && triggerValue <= chance) || force)
                {
                    Plugin.Logger.LogDebug("FEAR!");
                    player.StartCoroutine(ApplyTheFear(player));
                }

                hasFearAttackOccurred = true;
        }

        private static void TriggerAdrenaline(PlayerControllerB player)
        {
            float triggerValue = UnityEngine.Random.Range(0f, 100f);
            float chance = adrenalineChance;
            
            Plugin.Logger.LogDebug($"Adrenaline Trigger Value: {triggerValue}");
            if (!adrenaline && !trembling && triggerValue <= chance)
            {
                Plugin.Logger.LogDebug("ADRENALINE!");
                player.StartCoroutine(ApplyTheAdrenaline(player));
            }

            hasAdrenalineOccurred = true;
        }

        private static IEnumerator ApplyTheFear(PlayerControllerB player)
        {
            trembling = true;
            float originalMovementSpeed = player.movementSpeed;
            float originalClimbSpeed = player.climbSpeed;
            float originalLookSensitivity = player.lookSensitivity;
               
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            if(trembleInstant)
            {
                player.movementSpeed = trembleMoveSpeed;
                player.climbSpeed = trembleClimbSpeed;
                //player.lookSensitivity = 0.05f; This does not work.
                yield return new WaitForSeconds(trembleTime);
            }
            else
            {
                float trembleTimeInc = trembleTime / 20;
                for (int i = 0; i < 15; i++)
                {
                    player.movementSpeed = Mathf.Lerp(originalMovementSpeed, trembleMoveSpeed, (float)i/15);
                    player.climbSpeed = Mathf.Lerp(originalClimbSpeed, trembleClimbSpeed, (float)i/15);
                    yield return new WaitForSeconds(trembleTimeInc);
                }

                for (int i = 0; i < 5; i++)
                {
                    player.movementSpeed = Mathf.Lerp(trembleMoveSpeed, originalMovementSpeed, (float)i/5);
                    player.climbSpeed = Mathf.Lerp(trembleClimbSpeed, originalClimbSpeed, (float)i/5);
                    yield return new WaitForSeconds(trembleTimeInc);
                }
            }

            player.movementSpeed = originalMovementSpeed;
            player.climbSpeed = originalClimbSpeed;
            //player.lookSensitivity = originalLookSensitivity;
            trembling = false;
        }

        private static IEnumerator ApplyTheAdrenaline(PlayerControllerB player)
        {
            adrenaline = true;
            float originalMovementSpeed = player.movementSpeed;
            float originalClimbSpeed = player.climbSpeed;
            float originalFOV = player.targetFOV;
            float originalDrunkness = player.drunkness;

            if (adrenalineInstant)
            {
                
                // player.targetFOV = adrenalineFOV;        
                player.movementSpeed = adrenalineMoveSpeed;
                player.climbSpeed = adrenalineClimbSpeed;
                player.drunkness = adrenalineDrunkness;
                yield return new WaitForSeconds(adrenalineTime);
            } else
            {

                float adrenalineTimeInc = adrenalineTime / 20;
                for (int i = 0; i < 15; i++)
                {
                    player.movementSpeed = Mathf.Lerp(originalMovementSpeed, adrenalineMoveSpeed, (float)i/15);
                    player.climbSpeed = Mathf.Lerp(originalClimbSpeed, adrenalineClimbSpeed, (float)i/15);
                    player.drunkness = Mathf.Lerp(originalDrunkness, adrenalineDrunkness, (float)i/15);
                    yield return new WaitForSeconds(adrenalineTimeInc);
                }

                for (int i = 0; i < 5; i++)
                {
                    player.movementSpeed = Mathf.Lerp(adrenalineMoveSpeed, originalMovementSpeed, (float)i/5);
                    player.climbSpeed = Mathf.Lerp(adrenalineClimbSpeed, originalClimbSpeed, (float)i/5);
                    player.drunkness = Mathf.Lerp(adrenalineDrunkness, originalDrunkness, (float)i/5);
                    yield return new WaitForSeconds(adrenalineTimeInc);
                }
            }

            
            player.movementSpeed = originalMovementSpeed;
            player.climbSpeed = originalClimbSpeed;
            player.targetFOV = originalFOV;
            player.drunkness = originalDrunkness;
            if (trembleAfterAdrenaline)
            {
                TriggerTremblingFear(player,true);
            }

            adrenaline = false;
        }
    }
}
