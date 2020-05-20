using Base.UI;
using Harmony;
using ModnixUtils;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Common.View.ViewControllers.Inventory;
using PhoenixPoint.Tactical.Entities.DamageKeywords;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Reflection;
using UnityEngine;

namespace BetterItemInfo
{
    // Reminder: You will need to add a reference to Assembly-CSharp.dll in order to reference Phoenix Point
    // classes.
    // This is just an easier way to call the Func<string, object, object> that Modnix passes for API calls.
    using ModnixCallback = Func<string, object, object>;


    /// <summary>
    /// Put all configuration options here, Modnix will use this to create json configurations
    /// You can change the class to use for settings in Properties/mod_info.js
    /// Make sure to update mod_info if you change the namespace as well!
    /// </summary>
    public class ModConfig
    {
        public bool hideZeroes = true;
    }


    public class MyModnixMod
    {
        internal static ModConfig Config;
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
        /// Called after basic assets are loaded, before the hottest year cinematic. Virtually the same time
        /// as PPML. Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#MainMod </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result
        /// (object) varies by action.</param>
        public static void MainMod(ModnixCallback api = null)
        {
            BasicUtil.EnsureAPI(ref api);
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
        // Find once, use many. I really don't like it, but I have to Reflect to get this fucker
        static MethodInfo protectedMethod =
            typeof(UIItemTooltip).GetMethod("SetStat", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix(ItemDef item, bool secondObject, UIItemTooltip __instance)
        {
            bool isWeapon = !((item as WeaponDef) is null);
            int weight = item.CombinedWeight();

            if (isWeapon) {
                // TODO Implement Weapon code
                WeaponDef weapon = item as WeaponDef;
                bool is_melee = weapon.Tags.Contains(__instance.MeleeWeaponTag);
                bool is_explode = !float.IsInfinity(weapon.AreaRadius);

                if (!(weapon.DamagePayload is null)) {
                    int AmmoPerAction =
                        weapon.DamagePayload.AutoFireShotCount * weapon.DamagePayload.ProjectilesPerShot;

                    // Display Attack Type
                    if (is_melee) {
                        SetStat(__instance.MeleeBurstStatName, string.Empty, 1);
                    } else if (AmmoPerAction > 1) {
                        SetStat(__instance.RoundBurstStatName, AmmoPerAction);
                    } else {
                        SetStat(__instance.SingleBurstStatName, string.Empty, 1);
                    }
                    // TODO: Display scatter shot amount separate from burst.

                    // Iterate over damages
                    foreach (DamageKeywordPair dkp in weapon.DamagePayload.DamageKeywords) {
                        // Skip damage values of zero if configured to do so
                        if (MyModnixMod.Config.hideZeroes && dkp.Value == 0) continue;
                        SetStat(dkp.DamageKeywordDef.Visuals.DisplayName1, dkp.Value, dkp.Value);
                    }
                } else {
                    BasicUtil.Log($"{weapon.GetDisplayName().Localize()}: DamagePayload is null",
                        MyModnixMod.storedAPI);
                }

                // TODO: Range
                // TODO: Blast Radius
                // TODO: AP Cost

                // TODO: Determine if this sprite call even matters
                //ViewElementDef visuals = weapon.DamagePayload.GetTopPriorityDamageType().Visuals;
                //if (!(visuals is null)) {
                //    Sprite sprite = visuals.SmallIcon;
                //}
            } else {
#if DEBUG
                BasicUtil.Log("Defaulting for standard item.", MyModnixMod.storedAPI);
#endif
                // TODO Implement other items code, until then run the default code until implmented
                return true;
                // TODO: Armor stats (BodyPartAspect)
            }

            // TODO: Ammo charges (do anything but weapons use this?)
            // TODO: Hands to use
            SetStat(__instance.WeightStatName, weight, -weight);    // Weight
            return false;

            // helper to make calling __instance.SetStat easier
            void SetStat(LocalizedTextBind localizedText, object statValue, object statCompareValue = null,
                Sprite statIcon = null)
            {
                // apply those STDs
                protectedMethod.Invoke(__instance,
                    new object[] { localizedText, secondObject, statValue, statCompareValue, statIcon });
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
