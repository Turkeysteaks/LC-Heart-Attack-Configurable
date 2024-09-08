using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace HeartAttack.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static bool hasHeartAttackOccurred = false;
        [HarmonyPatch("Update")]
        [HarmonyPostfix]

        static void heartAttack(PlayerControllerB __instance)
        {
            if ((__instance == GameNetworkManager.Instance.localPlayerController))
            {
                if (StartOfRound.Instance.fearLevel > 0.5f && !hasHeartAttackOccurred)
                {

                    float heartAttackValue = UnityEngine.Random.Range(0f, 100f);
                    float chance = 20f;
                    Plugin.Logger.LogDebug($"Heart Attack Value: {heartAttackValue}");
                    if (heartAttackValue <= chance)
                    {
                        __instance.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown, 0, Vector3.zero);
                        Plugin.Logger.LogDebug("HEART ATTACK");
                    }

                    hasHeartAttackOccurred = true;

                }
                else if (StartOfRound.Instance.fearLevel < 0.5f)
                {
                    hasHeartAttackOccurred = false;
                }
            }
            else
            {
                return;
            }
        }
    }
}
