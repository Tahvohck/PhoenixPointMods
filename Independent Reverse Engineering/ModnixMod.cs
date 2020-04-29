//#define COMPAREMANY
//#define NOISY
using Base.Core;
using Base.Defs;
using ModnixUtils;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Geoscape.Entities.Research;
using PhoenixPoint.Geoscape.Entities.Research.Cost;
using PhoenixPoint.Geoscape.Entities.Research.Requirement;
using PhoenixPoint.Geoscape.Entities.Research.Reward;
using PhoenixPoint.Geoscape.Levels;
using PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Independent_Reverse_Engineering
{
    public class ModConfig
    {

    }


    public class THV_Indie_Revenge
    {
#pragma warning disable 0169    // Disable ModConfig not used
        private static ModConfig Config;
#pragma warning restore 0169
        private static string[] allowedSlots = { "Torso", "Legs", "Head", "GunPoint" };

        // PPML v0.1 entry point
        public static void Init() => MainMod();


        /// <summary>
        /// Called after basic assets are loaded, before the hottest year cinematic. Virtually the same time as PPML.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#MainMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public static void MainMod(Func<string, object, object> api = null)
        {
            api("log info", "New MainMod initialized");
            DefRepository gameRootDef = GameUtl.GameComponent<DefRepository>();

            List<TacticalItemDef> tacticalItems = gameRootDef.GetAllDefs<TacticalItemDef>().ToList().FindAll(
                new Predicate<TacticalItemDef>(FilterDefList)
            );
            BasicUtil.Log($"Readied {tacticalItems.Count} Independent tactical items.", api);

            string guid = "";
            foreach (ItemDef item in tacticalItems) {
                bool isWeapon = item.GetType() == typeof(WeaponDef);
#if DEBUG
                #region Debug output list items in tacticalItems
                Base.UI.LocalizedTextBind localizeMe = item.GetDisplayName();
                BasicUtil.Log($"{localizeMe.Localize()} - {item}", api);
                #endregion
#endif
                ResearchTagDef optional = (ResearchTagDef)gameRootDef.GetDef("08191866-ff38-9e74-abd7-cb484188911a");
                GeoFactionDef PhoenixPointFaction = (GeoFactionDef)gameRootDef.GetDef("8be7e872-0ad2-a2a4-7bee-c980ed304a8a");
                GeoFactionDef DisciplesOfAnuFaction = (GeoFactionDef)gameRootDef.GetDef("edc6783a-be00-1a84-2b97-2fe1e0fc5448");
                GeoFactionDef NewJerichoFaction = (GeoFactionDef)gameRootDef.GetDef("d31c78b9-ff0e-8b94-ab96-9672da73da54");
                GeoFactionDef SynedrionFaction = (GeoFactionDef)gameRootDef.GetDef("0e6dc218-e157-5954-c9ab-1a0606e0d914");

                // 1 + length of compatible ammo list
                int researchUnlockLength = 1 + ((isWeapon) ? ((WeaponDef)item).CompatibleAmmunition.Length : 0);
                ItemDef[] researchUnocks = new ItemDef[researchUnlockLength];
                researchUnocks[0] = item;
                for (int i = 1; i < researchUnlockLength; i++) {
                    researchUnocks[i] = ((WeaponDef)item).CompatibleAmmunition[i - 1];
                }

                string[] guidparts = item.Guid.Split('-');
                string guidBase = string.Join("-", guidparts.Take(3));
                string guidTail = "deadbeefbabe";
                int typeInt;

                #region Generate reverse engineering def
                typeInt = (int)ResearchGUIDSegments.ReceiveItemResearchRequirement;
                ReceiveItemResearchRequirementDef rirrDef = ScriptableObject.CreateInstance<ReceiveItemResearchRequirementDef>();
                rirrDef.name = item.name + "_ReceiveItemResearchRequirementDef";
                rirrDef.Guid = $"{guidBase}-{typeInt:x4}-{guidTail}";
                rirrDef.ItemDef = item;

                typeInt = (int)ResearchGUIDSegments.ItemResearchCost;
                ItemResearchCostDef ircDef = ScriptableObject.CreateInstance<ItemResearchCostDef>();
                ircDef.name = item.name + "_ItemResearchCostDef";
                ircDef.Guid = $"{guidBase}-{typeInt:x4}-{guidTail}";
                ircDef.ItemDef = item;
                ircDef.Amount = 1;
                ircDef.LocalizationText = new Base.UI.LocalizedTextBind() {
                    LocalizationKey = $"REVERSE_ENGINEER_COSTDEF_{item.name}"
                };

                typeInt = (int)ResearchGUIDSegments.ManufactureResearchReward;
                ManufactureResearchRewardDef mrdDef = ScriptableObject.CreateInstance<ManufactureResearchRewardDef>();
                mrdDef.name = item.name + "_ManufactureResearchRewardDef";
                mrdDef.Guid = $"{guidBase}-{typeInt:x4}-{guidTail}";
                mrdDef.Items = researchUnocks;

                typeInt = (int)ResearchGUIDSegments.Research;
                string rName = item.name + "_ResearchDef";
                ResearchDef reverseEngineerDef = ScriptableObject.CreateInstance<ResearchDef>();
                reverseEngineerDef.name = rName;
                reverseEngineerDef.Guid = $"{guidBase}-{typeInt:X4}-{guidTail}";
                reverseEngineerDef.Id = rName;
                reverseEngineerDef.Faction = PhoenixPointFaction;
                reverseEngineerDef.Costs = new ResearchCostDef[] { ircDef };
                reverseEngineerDef.ResearchCost = 100;
                reverseEngineerDef.Tags = new ResearchTagDef[] { optional };
                reverseEngineerDef.ValidForFactions = new List<GeoFactionDef> { PhoenixPointFaction };
                reverseEngineerDef.Unlocks = new ResearchRewardDef[] { mrdDef };
                reverseEngineerDef.InitialStates = new ResearchDef.InitialResearchState[] {
                    new ResearchDef.InitialResearchState {
                        Faction = PhoenixPointFaction,
                        State = ResearchState.Hidden
                    },
                    new ResearchDef.InitialResearchState {
                        Faction = DisciplesOfAnuFaction,
                        State = ResearchState.Hidden
                    },
                    new ResearchDef.InitialResearchState {
                        Faction = NewJerichoFaction,
                        State = ResearchState.Hidden
                    },
                    new ResearchDef.InitialResearchState {
                        Faction = SynedrionFaction,
                        State = ResearchState.Hidden
                    }
                };
                reverseEngineerDef.RevealRequirements.Container = new ReseachRequirementDefOpContainer[] {
                    new ReseachRequirementDefOpContainer() {
                        Operation = ResearchContainerOperation.ALL,
                        Requirements = new ResearchRequirementDef[] {
                            rirrDef
                        }
                    }
                };
#if DEBUG
                BasicUtil.Log($"{researchUnocks.Length} items prepared for the rDef.", api);
#if NOISY
                BasicUtil.Log(reverseEngineerDef.Repr(), api);
#endif
#endif
#endregion
                gameRootDef.CreateRuntimeDef(rirrDef, rirrDef.GetType(), rirrDef.Guid);
                gameRootDef.CreateRuntimeDef(ircDef,  ircDef.GetType(),  ircDef.Guid);
                gameRootDef.CreateRuntimeDef(mrdDef,  mrdDef.GetType(),  mrdDef.Guid);
                gameRootDef.CreateRuntimeDef(reverseEngineerDef, guid: reverseEngineerDef.Guid);
                guid = reverseEngineerDef.Guid;
            }
#if DEBUG
            foreach (GeoFactionDef fact in gameRootDef.GetAllDefs<GeoFactionDef>().ToList()) {
                BasicUtil.Log(fact.Render(), api);
            }
            BasicUtil.Log($"Looking for GUID: {guid}", api);
            DefRepository temp = GameUtl.GameComponent<DefRepository>();
            ResearchDef rDef = (ResearchDef)temp.GetDef(guid);
            BasicUtil.Log(rDef.name, api);
            BasicUtil.Log(rDef.Guid, api);
            BasicUtil.Log(rDef.Costs[0].Guid, api);
            BasicUtil.Log(rDef.Unlocks[0].Guid, api);
            BasicUtil.Log(rDef.RevealRequirements.Container[0].Requirements[0].Guid, api);

            BasicUtil.Log(
                rDef.Repr(),
                api);
            BasicUtil.Log(
                // Reverse Engineering example
                ((ResearchDef)temp.GetDef("15d2170b-469b-0341-22d4-a8b4d90eefb8")).Repr(),
                api);
#if COMPAREMANY
            BasicUtil.Log(
                // Capture research
                ((ResearchDef)temp.GetDef("c7b3f8ab-8c90-0343-823b-c966d2c4edb8")).Repr(),
                api);
            BasicUtil.Log(
                // Research that rewards multiple resources
                temp.GetAllDefs<ResearchDef>().ToList().Find( x => x.Resources.Count > 1).Repr(),
                api);
#endif
#endif
        }


        /// <summary>
        /// Returns true if the item matches our filter parameters.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool FilterDefList(ItemDef item)
        {
            // Result is true if item is under the 'Independant' resource path and
            // one of the allowed slots exists in its slotbinds.
            bool result =
                item.ResourcePath.Contains("Independant") &&
                item.RequiredSlotBinds.ToList().Exists(
                    slotBind => allowedSlots.Contains(
                        ((ItemSlotDef)slotBind.RequiredSlot).SlotName
                ));
            return result;
        }


        enum ResearchGUIDSegments
        {
            Research,
            ReceiveItemResearchRequirement,
            ItemResearchCost,
            ManufactureResearchReward,
        }
    }
}
