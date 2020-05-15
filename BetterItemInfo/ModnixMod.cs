using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Base.UI;
using Harmony;
using ModnixUtils;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Common.UI;
using PhoenixPoint.Common.View.ViewControllers.Inventory;
using PhoenixPoint.Tactical.Entities.DamageKeywords;
using PhoenixPoint.Tactical.Entities.Weapons;
using UnityEngine;

namespace BetterItemInfo
{
    // Reminder: You will need to add a reference to Assembly-CSharp.dll in order to reference Phoenix Point classes.
    using ModnixCallback = Func<string, object, object>;  // This is just an easier way to call the Func<string, object, object>
    using PriorityDict = Dictionary<string, int>;


    /// <summary>
    /// Put all configuration options here, Modnix will use this to create json configurations
    /// You can change the class to use for settings in Properties/mod_info.js
    /// Make sure to update mod_info if you change the namespace as well!
    /// </summary>
    public class ModConfig
    {

    }


    public class MyModnixMod
    {
#pragma warning disable IDE0044 // Add readonly modifier (This prevents the IDE from whining about the lack of assignment)
        internal static ModConfig Config;
#pragma warning restore IDE0044 // Add readonly modifier
        internal static HarmonyInstance harmonyInst;
        #region Modnix Support
        internal static ModnixCallback storedAPI;   // Use this if you need the API in helper methods or other classes.

        /// <summary>
        /// Fallback function for PPML mods or the (unlikely?) instance that Modnix doesn't supply an API function. Set "api" to this if api is null
        /// and it will make the mod PPML-safe (see <see cref="SplashMod"/> or <seealso cref="MainMod"/> stubs for example use.
        /// </summary>
        /// <returns>Always returns null.</returns>
        internal static object APIFallback(string str, object obj) { return null; }
        #endregion

        /// <summary>PPML v0.1 entry point</summary>
        public static void Init() => MainMod();

        /// <summary>
        /// Called after basic assets are loaded, before the hottest year cinematic. Virtually the same time as PPML.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#MainMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public static void MainMod(ModnixCallback api = null)
        {
            BasicUtil.EnsureAPI(ref api);
            FileLog.logPath = "./Mods/BII_Harmony.log";
            if (storedAPI is null) storedAPI = api;
            harmonyInst = HarmonyInstance.Create(typeof(MyModnixMod).FullName);
            harmonyInst.PatchAll();
            //BasicUtil.GetConfig(ref Config, api);

        }
    }

    
    [HarmonyPatch(typeof(UIItemTooltip))]
    [HarmonyPatch("SetStats")]
    public class HPatch_UIIT_SetStat
    {
        // Find once, use many
        // I really don't like it, but I have to Reflect to get this fucker
        static MethodInfo protectedMethod = typeof(UIItemTooltip).GetMethod("SetStat", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix(ItemDef item, bool secondObject, UIItemTooltip __instance)
        {
            bool isWeapon = !((item as WeaponDef) is null);
            int weight = item.CombinedWeight();

            if (isWeapon) {
                // TODO Implement Weapon code
                WeaponDef weapon = item as WeaponDef;
                bool is_melee = weapon.Tags.Contains(__instance.MeleeWeaponTag);
                bool is_explode = !float.IsInfinity(weapon.AreaRadius);
                FileLog.Log($"Displaying Item {item.GetDisplayName().Localize()}");

                if (!(weapon.DamagePayload is null)) {
                    FileLog.Log("It's a Weapon");
                    int AmmoPerAction = weapon.DamagePayload.AutoFireShotCount * weapon.DamagePayload.ProjectilesPerShot;

                    // Display Attack Type
                    try {
                        if (is_melee) {
                            FileLog.Log("Melee");
                            SetStat(__instance.MeleeBurstStatName, string.Empty, 1);
                        } else if (AmmoPerAction > 1) {
                            FileLog.Log("Burst fire");
                            SetStat(__instance.RoundBurstStatName, AmmoPerAction);
                        } else {
                            FileLog.Log("Single fire");
                            SetStat(__instance.SingleBurstStatName, string.Empty, 1);
                        }
                    } catch (Exception e) {
                        FileLog.Log($"---------------------------------------\n" +
                            e.Message + "\n" +
                            e.StackTrace +
                            $"\n---------------------------------------");
                    }

                    try 
                    {
                        FileLog.Log($"Damage types to parse: {weapon.DamagePayload.DamageKeywords.Count}");
                        foreach (DamageKeywordPair dkp in weapon.DamagePayload.DamageKeywords) {
                            FileLog.Log($"Add damage: {dkp.DamageKeywordDef.Visuals.DisplayName1.Localize()} - {dkp.Value}");
                            if (dkp.Value == 0) continue;   // Skip damage values of zero
                            SetStat(dkp.DamageKeywordDef.Visuals.DisplayName1, dkp.Value, dkp.Value);
                        }
                    } catch (Exception e) {
                        FileLog.Log($"---------------------------------------\n" +
                            e.Message + "\n" +
                            e.StackTrace +
                            $"\n---------------------------------------");
                    }
                }

                // TODO: Determin if this sprite call even matters
                //ViewElementDef visuals = weapon.DamagePayload.GetTopPriorityDamageType().Visuals;
                //if (!(visuals is null)) {
                //    Sprite sprite = visuals.SmallIcon;
                //}
            } else {
                FileLog.Log("Defaulting for standard item.");
                // TODO Implement Other Code
                return true; // run the default code until implmented
            }
            SetStat(__instance.WeightStatName, weight, -weight);
            return false;


            // helper to make calling __instance.SetStat easier
            void SetStat(LocalizedTextBind localizedText, object statValue, object statCompareValue = null, Sprite statIcon = null)
            {
                // apply those STDs
                protectedMethod.Invoke(__instance, new object[] { localizedText, secondObject, statValue, statCompareValue, statIcon });
            }

        }
    }


    [Flags]
    public enum ItemTypes
    {
        Weapon,
        Armor,
        Ammo,
        Consumable,
        VehicleItem
    }
}
