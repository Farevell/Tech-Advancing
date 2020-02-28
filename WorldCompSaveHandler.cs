﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace TechAdvancing
{
    internal class WorldCompSaveHandler : WorldComponent
    {
        private Dictionary<string, int> Configvalues = new Dictionary<string, int>();

        internal List<string> GetConfigValueNames => Configvalues.Keys.ToList();

        /// <summary>
        /// Stores all the pawns that joined along with their old Faction
        /// </summary>
        public Dictionary<Pawn, Faction> ColonyPeople = new Dictionary<Pawn, Faction>(); //pawn , ORIGINAL faction

        public WorldCompSaveHandler(World world) : base(world)
        {

        }

        public bool IsValueSaved(string key) { return Configvalues.ContainsKey(key); }
        public void RemoveConfigValue(string key) { Configvalues.Remove(key); }

        public void TA_ExposeData(string key, ref int value, TA_Expose_Mode mode = TA_Expose_Mode.Load)
        {
            if (mode == TA_Expose_Mode.Save)
            {
                LogOutput.WriteLogMessage(Errorlevel.Debug, "Adding " + key + " : " + value + "to save dictionary");
                if (Configvalues.ContainsKey(key))
                {
                    Configvalues.Remove(key);
                }
                Configvalues.Add(key, value);
            }
            else if (mode == TA_Expose_Mode.Load)
            {
                if (Configvalues.TryGetValue(key, out int tempval))
                {
                    value = tempval;
                }
                else if (Configvalues.TryGetValue(Enum.GetNames(typeof(TA_Expose_Name)).Contains(key) ? ((int)Enum.Parse(typeof(TA_Expose_Name), key)).ToString() : key, out tempval)) // TODO remove backwards compatability fallback
                {
                    value = tempval;
                    LogOutput.WriteLogMessage(Errorlevel.Information, "Value " + key + " was loaded via fallback. (A new save system is in place. But this message shouldnt appear anymore after saving)");
                }
                else
                {
                    LogOutput.WriteLogMessage(Errorlevel.Information, "Value " + key + " could not be loaded. This usually happens when updating to the new config-system. Try saving and reloading the map.");
                }

                LogOutput.WriteLogMessage(Errorlevel.Debug, "Successfully loaded " + key + " : " + value + "from save dictionary.");
            }
        }

        public override void ExposeData()
        {
            TechAdvancing_Config_Tab.worldCompSaveHandler = this;
            base.ExposeData();

            Scribe_Collections.Look(ref Configvalues, "TA_Expose_Numbers", LookMode.Value, LookMode.Value);
            int isPplDictSaved = 1;
            //LogOutput.WriteLogMessage(Errorlevel.Information, "val:" + isPplDictSaved.ToString());
            Scribe_Values.Look(ref isPplDictSaved, "TA_Expose_People_isSaved", -1, true);
            //LogOutput.WriteLogMessage(Errorlevel.Information, "val:" + isPplDictSaved.ToString());
            if (ColonyPeople != null)
            {
                ColonyPeople.RemoveAll(x => x.Key == null);
            }
            if (isPplDictSaved == 1)
            {
                Scribe_Collections.Look(ref ColonyPeople, "TA_Expose_People", LookMode.Reference, LookMode.Reference);
                //LogOutput.WriteLogMessage(Errorlevel.Information, "Read TA_ExposePeople");
            }
            TechAdvancing_Config_Tab.ExposeData(TA_Expose_Mode.Load);
            if (ColonyPeople == null)
            {
                ColonyPeople = new Dictionary<Pawn, Faction>();
            }
            LogOutput.WriteLogMessage(Errorlevel.Information, "Loading finished.");
        }
    }

    public enum TA_Expose_Mode
    {
        Save,
        Load
    }

    public enum TA_Expose_Name // TODO Remove soon
    {
        Conditionvalue_A,
        Conditionvalue_B,
        Conditionvalue_B_s,
        baseTechlvlCfg,
        configCheckboxNeedTechColonists,
        configCheckboxDisableCostMultiplicatorCap
    }
}