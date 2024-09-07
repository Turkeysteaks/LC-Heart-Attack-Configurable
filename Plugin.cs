using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using HeartAttack.Patches;

namespace HeartAttack
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Nono.HeartAttackMod";
        private const string modName = "Heart Attack";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        internal new static ManualLogSource Logger;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Logger = base.Logger;

            Logger.LogInfo("OH MY LORD!");

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
        }
    }
}
