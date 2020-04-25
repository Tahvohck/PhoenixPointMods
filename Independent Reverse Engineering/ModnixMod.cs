using Base.Core;
using Base.Defs;
using ModnixUtils;
using PhoenixPoint.Common.Entities.Addons;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Independent_Reverse_Engineering
{
    public class ModConfig
    {

    }


    public class MyMod
    {
        private static ModConfig Config;

        // PPML v0.1 entry point
        public static void Init() => new MyMod().MainMod();

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

            List<WeaponDef> weaponDefs = gameRootDef.GetAllDefs<WeaponDef>().ToList().FindAll(
                def =>
                def.ResourcePath.Contains("Independant") &&
                def.RequiredSlotBinds.ToList().Exists(
                    slotBind =>
                    ((ItemSlotDef)slotBind.RequiredSlot).SlotName == "GunPoint"
                )
            );
            BasicUtil.Log($"Readied {weaponDefs.Count} Independent weapons.", api);
#if DEBUG
            foreach (WeaponDef weapon in weaponDefs) {
                BasicUtil.Log($"{weapon.ViewElementDef.DisplayName1.Localize()} - {weapon}", api);
            }
#endif

            string[] slots = { "Torso", "Legs", "Head" };
            List<ItemDef> armorDefs = gameRootDef.GetAllDefs<ItemDef>().ToList().FindAll(
                def =>
                def.ResourcePath.Contains("Independant") &&
                def.RequiredSlotBinds.ToList().Exists(
                    slotBind =>
                    slots.Contains( ((ItemSlotDef)slotBind.RequiredSlot).SlotName )
                )
            );
            BasicUtil.Log($"Readied {armorDefs.Count} Independent armor items.", api);
#if DEBUG
            foreach (ItemDef armor in armorDefs) {
                BasicUtil.Log($"{armor.ViewElementDef.DisplayName2.Localize()} - {armor}", api);
            }
#endif
        }
    }
}
