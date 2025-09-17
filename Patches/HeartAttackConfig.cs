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
        public readonly ConfigEntry<float> adrenalineChance;
        public readonly ConfigEntry<bool> trembleAfterAdrenaline;

        public readonly ConfigEntry<bool> trembleInstant;
        public readonly ConfigEntry<float> playerTrembleMoveSpeed;
        public readonly ConfigEntry<float> playerTrembleClimbSpeed;
        public readonly ConfigEntry<float> playerTrembleTime;

        public readonly ConfigEntry<bool> adrenalineInstant;
        public readonly ConfigEntry<float> adrenalineMoveSpeed;
        public readonly ConfigEntry<float> adrenalineClimbSpeed;
        public readonly ConfigEntry<float> adrenalineTime;
        public readonly ConfigEntry<float> adrenalineFOV;
        public readonly ConfigEntry<float> adrenalineDrunkness;
        public HeartAttackConfig(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;

            heartAttackChance = cfg.Bind(
                "General",
                "HeartAttackChance",
                5f,
                new ConfigDescription("Percentage chance to have a heart attack on fear. Set to 0 for no chance.", new AcceptableValueRange<float>(0f, 100f))
            );

            trembleChance = cfg.Bind(
                "General",
                "TrembleChance",
                13f,
                new ConfigDescription("Percentage chance to tremble on fear. Set to 0 for no chance.", new AcceptableValueRange<float>(0f, 100f))
            );
            
            adrenalineChance = cfg.Bind(
                "General",
                "AdrenalineChance",
                15f,
                new ConfigDescription("Percentage chance to gain an adrenaline rush on fear. Set to 0 for no chance.", new AcceptableValueRange<float>(0f, 100f))
            );

            trembleAfterAdrenaline = cfg.Bind(
                "General",
                "TrembleAfterAdrenaline",
                false,
                "Whether to always force a tremble (slowdown) after an adrenaline rush."
            );

            trembleInstant = cfg.Bind(
                "Tremble",
                "TrembleIsInstant",
                true,
                "Whether tremble's effects happen instantly or are gradually started and ended."
            );
            playerTrembleMoveSpeed = cfg.Bind(
                "Tremble",
                "TrembleMoveSpeed",
                0.5f,
                "Speed the player walks while trembling."
            );
            playerTrembleClimbSpeed = cfg.Bind(
                "Tremble",
                "TrembleClimbSpeed",
                2f,
                "Speed the player climbs while trembling"
            );
            playerTrembleTime = cfg.Bind(
                "Tremble",
                "TrembleTime",
                2.5f,
                "Time (in seconds) the player trembles for."
            );
            
            adrenalineInstant = cfg.Bind(
                "Adrenaline",
                "AdrenalineIsInstant",
                false,
                "Whether adrenaline's effects happen instantly or are gradually started and ended."
            );
            adrenalineMoveSpeed = cfg.Bind(
                "Adrenaline",
                "AdrenalineMoveSpeed",
                7f,
                "Speed the player walks with adrenaline."
            );
            
            adrenalineClimbSpeed = cfg.Bind(
                "Adrenaline",
                "AdrenalineClimbSpeed",
                4f,
                "Speed the player climbs with adrenaline."
            );
            
            adrenalineTime = cfg.Bind(
                "Adrenaline",
                "AdrenalineTime",
                3f,
                "Time (in seconds) the player is affected by the adrenaline rush."
            );
            
            adrenalineFOV = cfg.Bind(
                "Adrenaline",
                "AdrenalineFOV",
                72f,
                "(Currently not used) Field of View to use while affected by the adrenaline rush."
            );
            
            adrenalineDrunkness = cfg.Bind(
                "Adrenaline",
                "AdrenalineExtraDrunkness",
                0.6f,
                "How much 'drunkness' to add to the player when hit with adrenaline."
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