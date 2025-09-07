using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using BepInEx.Configuration;

namespace HeartAttack.Patches
{
    public class HeartAttackConfig
    {
        public readonly ConfigEntry<float> heartAttackChance;
        public readonly ConfigEntry<float> trembleChance;

        public readonly ConfigEntry<float> playerTrembleMoveSpeed;
        public readonly ConfigEntry<float> playerTrembleClimbSpeed;
        
        
        public static HeartAttackConfig Instance;

        public HeartAttackConfig(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;

            heartAttackChance = cfg.Bind(
                "General.Chances",
                "HeartAttackChance",
                5f,
                new ConfigDescription("Percentage chance to have a heart attack on fear", new AcceptableValueRange<float>(0f, 100f))
            );

            trembleChance = cfg.Bind(
                "General.Chances",
                "TrembleChance",
                13f,
                new ConfigDescription("Percentage chacne to tremble on fear", new AcceptableValueRange<float>(0f, 100f))
            );

            playerTrembleMoveSpeed = cfg.Bind(
                "General.Tremble",
                "TrembleMoveSpeed",
                0.35f,
                "Speed the player walks while trembling"
            );
            playerTrembleClimbSpeed = cfg.Bind(
                "General.Tremble",
                "TrembleClimbSpeed",
                2f,
                "Speed the player climbs while trembling"
            );

            /*  https://lethal.wiki/dev/intermediate/custom-configs  */
            // Get rid of old settings from the config file that are not used anymore //
            ClearOrphanedEntries(cfg);
            // We need to manually save since we disabled `SaveOnConfigSet` earlier //
            cfg.Save();
            // And finally, we re-enable `SaveOnConfigSet` so changes to our config //
            // entries are written to the config file automatically from now on //
            cfg.SaveOnConfigSet = true;
        }

        static void ClearOrphanedEntries(ConfigFile cfg)
        {
            /*  https://lethal.wiki/dev/intermediate/custom-configs  */
            // Find the private property `OrphanedEntries` from the type `ConfigFile` //
            PropertyInfo orphanedEntriesProp = AccessTools.Property(
                typeof(ConfigFile),
                "OrphanedEntries"
            );
            // And get the value of that property from our ConfigFile instance //
            var orphanedEntries =
                (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg);
            // And finally, clear the `OrphanedEntries` dictionary //
            orphanedEntries.Clear();
        }
    }
}