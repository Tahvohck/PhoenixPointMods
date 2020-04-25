﻿using Base.Core;
using Base.Defs;
using ModnixUtils;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static void Init() => new THV_Indie_Revenge().MainMod();

        /// <summary>
        /// Called very early, just after main assemblies are loaded, before logos. Saves have not been scanned and most game data are unavailable.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#SplashMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public void SplashMod(Func<string, object, object> api = null)
        {
        }


        /// <summary>
        /// Called after basic assets are loaded, before the hottest year cinematic. Virtually the same time as PPML.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#MainMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public void MainMod(Func<string, object, object> api = null)
        {
            api("log info", "New MainMod initialized");
            DefRepository gameRootDef = GameUtl.GameComponent<DefRepository>();
            
            List<TacticalItemDef> tacticalItems = gameRootDef.GetAllDefs<TacticalItemDef>().ToList().FindAll(
                new Predicate<TacticalItemDef>(FilterDefList)
            );
            BasicUtil.Log($"Readied {tacticalItems.Count} Independent tactical items.", api);

            foreach (ItemDef item in tacticalItems) {
                bool isWeapon = item.GetType() == typeof(WeaponDef);
#if DEBUG
                #region Debug output list items in tacticalItems
                Base.UI.LocalizedTextBind localizeMe = item.GetDisplayName();
                BasicUtil.Log($"{localizeMe.Localize()} - {item}", api);
                #endregion
#endif
            }
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
    }
}
