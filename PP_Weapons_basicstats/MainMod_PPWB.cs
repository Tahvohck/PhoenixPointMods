using System;
using ModnixUtils;
using Base.Core;
using Base.Defs;
using PhoenixPoint.Tactical.Entities.Weapons;
using PhoenixPoint.Tactical.Entities.DamageKeywords;
using System.Collections.Generic;
using System.Linq;

namespace Tahvohck.PP_Weapons_basicstats
{
    public class ModConfig
    {
        public bool add_shred = false;
        public bool add_pierce = false;
        public bool add_bleed = false;
    }


    public class MyMod
    {
        private static ModConfig Config;
        private DefRepository definitions_repo;

        // PPML v0.1 entry point
        public static void Init() => new MyMod().SplashMod();

        /// <summary>
        /// Called very early, just after main assemblies are loaded, before logos. Saves have not been scanned and most game data are unavailable.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#SplashMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public void SplashMod(Func<string, object, object> api = null)
        {

            DefRepository   definitions_repo = GameUtl.GameComponent<DefRepository>();
            ModConfig       Config = (ModConfig)api(ModnixAPIActions.config, new ModConfig());
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
        }
    }

    /// <summary>
    /// Extensions for a variety of PP default classes.
    /// </summary>
    public static class PPWB_Extensions
    {
        /// <summary>
        /// Special case sort list
        /// </summary>
        private static string[] damageKeywordDefSortSpecial = {
            "Damage_DamageKeywordDataDef",
            "Piercing_DamageKeywordDataDef",
            "Shredding_DamageKeywordDataDef",
            "Bleeding_DamageKeywordDataDef"
        };
        /// <summary>
        /// Extension method to sort damage keyword pairs based on their damage keyword def.
        /// </summary>
        /// <param name="dkpA"></param>
        /// <param name="dkpB"></param>
        /// <returns></returns>
        public static int CompareTo(this DamageKeywordPair dkpA, DamageKeywordPair dkpB)
        {
            // Setup
            string nameA = dkpA.DamageKeywordDef.name;
            string nameB = dkpB.DamageKeywordDef.name;
            int posA = Array.FindIndex(damageKeywordDefSortSpecial, x => x == nameA);
            int posB = Array.FindIndex(damageKeywordDefSortSpecial, x => x == nameB);

            if (posA != -1 && posB != -1) {
                // If both ar in the list, do a comparison on their index
                return posA - posB;
            } else if (posA != -1) {
                // If A is in the list but not B, A should be first
                return -1;
            } else if (posB != -1) {
                // If B is in the list bit not A, B should be first
                return 1;
            } else {
                // Otherwise fallback on the general comparison
                return nameA.CompareTo(nameB);
            }
        }

        /// <summary>
        /// Clone a damageKeywordPair by Value and damageKeywordDef. This should be enough to prevent any shallow copy issues.
        /// </summary>
        /// <param name="dkp">The damageKeywordPair to clone</param>
        /// <returns>A semi-fresh copy of the dkp</returns>
        public static DamageKeywordPair Clone(this DamageKeywordPair dkp)
        {
            return new DamageKeywordPair() {
                Value = dkp.Value,
                DamageKeywordDef = dkp.DamageKeywordDef
            };
        }
    }
}
