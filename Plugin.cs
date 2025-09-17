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
        private const string modGUID = "Turkeysteaks.HeartAttackMod";
        private const string modName = "Real Heart Attack";
        private const string modVersion = "1.1.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        internal static new ManualLogSource Logger;
        internal static HeartAttackConfig BoundConfig { get; private set; } = null;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            BoundConfig = new HeartAttackConfig(base.Config);

            Logger = base.Logger;

            Logger.LogInfo("OH MY LORD!");

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
        }
    }
}
