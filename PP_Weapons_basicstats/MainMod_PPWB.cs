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
    public class ModConfig : ModConfigBase
    {
        public bool add_shred = true;
        public bool add_pierce = true;
        public bool add_bleed = true;
    }


    public class MyMod
    {
        private static ModConfig Config;

        // PPML v0.1 entry point
        public static void Init() => new MyMod().SplashMod();

        /// <summary>
        /// Called very early, just after main assemblies are loaded, before logos. Saves have not been scanned and most game data are unavailable.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#SplashMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public void SplashMod(Func<string, object, object> api = null)
        {
            // Technically, SplashMod isn't the right place to init this mod. However, it ensures a VERY
            // early load, and it doesn't cause any issues. This means PPDefModifier can modify it.
            BasicUtil.EnsureAPI(ref api);
            BasicUtil.GetConfig(ref Config, api);

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
}
