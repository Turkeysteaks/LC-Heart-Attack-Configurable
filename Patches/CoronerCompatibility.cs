using System.Runtime.CompilerServices;
using GameNetcodeStuff;
using Coroner;

namespace HeartAttack.Patches
{
    public class CoronerCompatibility
    {
        private static bool? _enabled;
        static string HEART_ATTACK_LANGUAGE_KEY = "DeathHeartAttack";

        private static AdvancedCauseOfDeath HEART_ATTACK;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.elitemastereric.coroner");
                }

                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void CoronerRegister()
        {
            if (Coroner.API.IsRegistered(HEART_ATTACK_LANGUAGE_KEY))
                return;
            HEART_ATTACK = Coroner.API.Register(HEART_ATTACK_LANGUAGE_KEY);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void CoronerSetCauseOfDeathHeartAttack(PlayerControllerB player)
        {
            Coroner.API.SetCauseOfDeath(player, HEART_ATTACK);
        }

    }
}