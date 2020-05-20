using Base.Core;
using Base.Defs;
using Base.UI;
using Harmony;
using ModnixUtils;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Common.View.ViewControllers.Inventory;
using PhoenixPoint.Tactical.Entities.DamageKeywords;
using PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Tactical.Entities.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tahvohck.PP_Weapons_basicstats
{
    using ModnixCallback = Func<string, object, object>;


    public class ModConfig : ModConfigBase
    {
        public bool add_shred = true;
        public bool add_pierce = true;
        public bool add_bleed = true;
        public bool show_zeros = false;
    }


    public class MyMod
    {
        internal static ModConfig Config;
        internal static ModnixCallback storedAPI;
        internal static HarmonyInstance harmInst;

        // PPML v0.1 entry point
        public static void Init() => new MyMod().SplashMod();

        /// <summary>
        /// Called very early, just after main assemblies are loaded, before logos. Saves have not been scanned and most game data are unavailable.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#SplashMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public void SplashMod(ModnixCallback api = null)
        {
            // Technically, SplashMod isn't the right place to init this mod. However, it ensures a VERY
            // early load, and it doesn't cause any issues. This means PPDefModifier can modify it.
            harmInst = HarmonyInstance.Create(typeof(MyMod).FullName);
            harmInst.PatchAll();
            FileLog.logPath = "./Mods/PPWB_Harmony.log";
            FileLog.Log("PPWB...");
            BasicUtil.EnsureAPI(ref api);
            BasicUtil.GetConfig(ref Config, api);
            storedAPI = api;

            DefRepository definitions_repo = GameUtl.GameComponent<DefRepository>();
            List<WeaponDef> WeaponList = definitions_repo.GetAllDefs<WeaponDef>().ToList();
            List<DamageKeywordDef> damageKeywords = definitions_repo.GetAllDefs<DamageKeywordDef>().ToList();

            BasicUtil.Log($"Found {WeaponList.Count} weapons loaded into the game.", api);
            BasicUtil.Log($"Found {damageKeywords.Count} damage types loaded into the game.", api);

            #region DamageTypeSetup
            List<DamageKeywordPair> damageKeywordPairsToAdd = new List<DamageKeywordPair>();
            if (Config.add_bleed) {
                damageKeywordPairsToAdd.Add(new DamageKeywordPair() {
                    Value = 0,
                    DamageKeywordDef = damageKeywords.Find(x => x.name.Contains("Bleed"))
                });
            }
            if (Config.add_pierce) {
                damageKeywordPairsToAdd.Add(new DamageKeywordPair() {
                    Value = 0,
                    DamageKeywordDef = damageKeywords.Find(x => x.name.Contains("Pierc")) // Because it's truncated "Piercing"
                });
            }
            if (Config.add_shred) {
                damageKeywordPairsToAdd.Add(new DamageKeywordPair() {
                    Value = 0,
                    DamageKeywordDef = damageKeywords.Find(x => x.name.Contains("Shred"))
                });
            };
            #endregion

            BasicUtil.Log("Adding damage types to all weapons.", api);
            foreach (WeaponDef weapon in WeaponList) {
                foreach (DamageKeywordPair dkp in damageKeywordPairsToAdd) {
                    if (!weapon.DamagePayload.DamageKeywords.Exists(x => x.DamageKeywordDef == dkp.DamageKeywordDef)) {
                        weapon.DamagePayload.DamageKeywords.Add(dkp.Clone());
#if DEBUG
                        BasicUtil.Log($"Had to add damage type {dkp.DamageKeywordDef.Visuals.DisplayName1.Localize()} to {weapon.GetDisplayName().Localize()}", api);
#endif
                    }
                }
                weapon.DamagePayload.DamageKeywords.Sort( (x, y) => x.CompareTo(y));
            }

            BasicUtil.Log("Done adding damage types.", api);
        }
    }

#if DEBUG // Don't bother injecting into higher-level stack if not in debug mode
    #region Higher-level stack injection
    [HarmonyPatch(typeof(UIItemTooltip))]
    [HarmonyPatch("SetStats")]
    public class HarmonyInjection_UIItemTooltip_SetStats
    {
        public static void Prefix(ItemDef item, MethodInfo __originalMethod)
        {
            string str = $"Successfully injected into {__originalMethod.ReflectedType}.{__originalMethod.Name}, object is {item.GetDisplayName().Localize()}";
            BasicUtil.Log(str, MyMod.storedAPI);
            FileLog.Log(str);
        }
    }


    [HarmonyPatch(typeof(UIItemTooltip))]
    [HarmonyPatch("SetWeaponStats")]
    public class HarmonyInjection_UIItemTooltip_SetStatsWeapon
    {
        public static void Prefix(ItemDef item, MethodInfo __originalMethod)
        {
            string str = $"Successfully injected into {__originalMethod.ReflectedType}.{__originalMethod.Name}, object is {item.GetDisplayName().Localize()}";
            BasicUtil.Log(str, MyMod.storedAPI);
            FileLog.Log(str);
        }
    }


    [HarmonyPatch(typeof(UIItemTooltip))]
    [HarmonyPatch("SetTacItemStats")]
    public class HarmonyInjection_UIItemTooltip_SetStatsTacItem
    {
        public static void Prefix(TacticalItemDef tacItemDef, MethodInfo __originalMethod)
        {
            string str = $"Successfully injected into {__originalMethod.ReflectedType}.{__originalMethod.Name}, object is {tacItemDef.GetDisplayName().Localize()}";
            BasicUtil.Log(str, MyMod.storedAPI);
            FileLog.Log(str);
        }
    }
    #endregion
#endif

    [HarmonyPatch(typeof(UIItemTooltip))]
    [HarmonyPatch("SetStat")]
    public class HarmonyInjection_UIItemTooltip_SetStat
    {
        /// <summary>
        /// Skip SetStat if stat is 0
        /// </summary>
        public static bool Prefix(
            UIItemTooltip __instance, MethodInfo __originalMethod,
            LocalizedTextBind statLocalization, bool secondObject, object statValue,
            object statCompareValue = null, UnityEngine.Sprite statIcon = null )
        {
            float svFloat,scvFloat;
            string svType, scvType = "";
            string svString = statValue as string ?? "null" ;
            string scvString = statCompareValue as string ?? "null";
            string zeroPercent = Base.UI.UIUtil.PercentageStat(0f, __instance.PercentageString.Localize(null));

            try {
                svFloat = (float)statValue;
            } catch (InvalidCastException) {
                svFloat = 12.34f;
            }
            try {
                scvFloat = (float)statValue;
            } catch (InvalidCastException) {
                scvFloat = 12.34f;
            }
            try {
                svType = statValue.GetType().ToString();
            } catch (NullReferenceException) {
                svType = "NULL";
            }
            try {
                scvType = statCompareValue.GetType().ToString();
            } catch (NullReferenceException) {
                scvType = "NULL";
            }

            bool svSkip = svString == "0" || svString == zeroPercent || svFloat == 0f;
            bool scvSkip = scvString == "0" || scvString == zeroPercent || scvFloat == 0f;
#if DEBUG
            BasicUtil.Log($"Injected into {__instance.GetType()}.{__originalMethod.Name}()", MyMod.storedAPI);
            BasicUtil.Log($"  {statLocalization.Localize()} [2nd: {secondObject}]", MyMod.storedAPI);
            BasicUtil.Log($"  SV: {statValue ?? 0,4} [{svType}]", MyMod.storedAPI);
            BasicUtil.Log($"  CV: {statCompareValue ?? 0,4} [{scvType}]", MyMod.storedAPI);
            BasicUtil.Log($"  SVSkip: {svSkip}", MyMod.storedAPI);
            BasicUtil.Log($"  CVSkip: {scvSkip}", MyMod.storedAPI);
#endif

            return !(scvSkip && svSkip) || MyMod.Config.show_zeros;
        }
    }
}
