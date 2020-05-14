using System;
using System.Collections.Generic;
using System.Linq;
using Base.Defs;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Geoscape.Entities.Research;
using PhoenixPoint.Geoscape.Entities.Research.Cost;
using PhoenixPoint.Geoscape.Entities.Research.Requirement;
using PhoenixPoint.Geoscape.Entities.Research.Reward;
using PhoenixPoint.Geoscape.Levels;
using PhoenixPoint.Tactical.Entities.DamageKeywords;

namespace ModnixUtils
{
    using ModnixCallback = Func<string, object, object>;

    public abstract class ModConfigBase
    {
        public static readonly DateTime mostRecentVersion = new DateTime(year: 2020, month: 04, day: 21);
        public DateTime configversion = mostRecentVersion;

        public void Upgrade()
        {
            if (configversion < mostRecentVersion) {
                configversion = mostRecentVersion;
            }
        }
    }

    public class BasicUtil
    {
        public static void Log(object input, Func<string, object, object> api)
        {
            if (api is null) return;
            api(ModnixAPIActions.Log.info, input);
        }

        /// <summary>
        /// Creates a dummy stub for <seealso cref="ModnixCallback"/> type if api is null.
        /// </summary>
        public static void EnsureAPI(ref ModnixCallback api)
        {
            if (api is null) {
                api = (str, obj) => null;
            }
        }

        /// <summary>
        /// Get the config from Modnix or a new, default config. Optionally upgrade (on by default).
        /// </summary>
        /// <typeparam name="GenericConfig">Derived config type</typeparam>
        /// <param name="doUpgrade">Set to false to skip the upgrade and save the config while loading.</param>
        /// <returns>True if the config was able to be loaded from Modnix.</returns>
        public static bool GetConfig<GenericConfig>(ref GenericConfig config, ModnixCallback api, bool doUpgrade = true)
        where GenericConfig : ModConfigBase, new()
        {
            EnsureAPI(ref api);     // api is not passed by ref to GetConfig, so this only ensures it's locally non-null
            config = api(ModnixAPIActions.config, new GenericConfig()) as GenericConfig;
            if (config is null) {
                config = new GenericConfig();
                return false;
            }
            if (doUpgrade) {
                config.Upgrade();
                api(ModnixAPIActions.Config.save, config);
            }
            return true;
        }
    }


    /// <summary>
    /// Configuration class
    /// </summary>
    public class ModConfig : ModConfigBase
    {
        public bool newlinesInOutput = false;
        public string indentString = "    ";
    }


    /// <summary>
    /// Hook to get Modnix to load this.
    /// </summary>
    public class ModnixMod
    {
        public void SplashMod(Func<string, object, object> api)
        {
            BasicUtil.Log("ModnixUtil Loaded", api);
        }
    }


    /// <summary>
    /// Extensions for a variety of PP default classes.
    /// </summary>
    public static class BaseGameExtensions
    {
        /// <summary>
        /// Special case sort list
        /// </summary>
        private static readonly string[] damageKeywordDefSortSpecial = {
            "Damage_DamageKeywordDataDef",
            "Piercing_DamageKeywordDataDef",
            "Shredding_DamageKeywordDataDef",
            "Bleeding_DamageKeywordDataDef"
        };

        /// <summary>
        /// [Extension method] Compares this instance with a specified <seealso cref="DamageKeywordPair"/>
        /// object and indicates whether this instance precedes, follows, or appears in the same position
        /// in the sort order as the specified <seealso cref="DamageKeywordPair"/>.
        /// </summary>
        /// <param name="dkpB">The <seealso cref="DamageKeywordPair"/> to compare to this instance</param>
        /// <returns>-1 if A comes first, 1 if B comes first, 0 if they sort in the same place.</returns>
        public static int CompareTo(this DamageKeywordPair dkpA, DamageKeywordPair dkpB)
        {
            return dkpA.DamageKeywordDef.CompareTo(dkpB.DamageKeywordDef);
        }

        /// <summary>
        /// [Extension method] Compares this instance with a specified <seealso cref="DamageKeywordDef"/>
        /// object and indicates whether this instance precedes, follows, or appears in the same position
        /// in the sort order as the specified <seealso cref="DamageKeywordDef"/>.
        /// </summary>
        /// <param name="dkdB">The <seealso cref="DamageKeywordDef"/> to compare to this instance</param>
        /// <returns>-1 if A comes first, 1 if B comes first, 0 if they sort in the same place.</returns>
        public static int CompareTo(this DamageKeywordDef dkdA, DamageKeywordDef dkdB)
        {
            // Setup
            string nameA = dkdA.name;
            string nameB = dkdB.name;
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

        /// <summary>
        /// Return a string representation of a <see cref="ResearchDef"/>.
        /// </summary>
        /// <param name="prefix">Optional prefix to add to each line of the string.</param>
        /// <returns></returns>
        public static string Repr(this ResearchDef rDef, string prefix = "")
        {
            string prefix2 = "    ";
            bool level2newline = true;
            int initial_indent = 0;
            string strNotForDLC =       HandleArrayItems(rDef.NotEnabledForDLC, Render, prefix2, level2newline, initial_indent);
            string strInitialStates =   HandleArrayItems(rDef.InitialStates, Render, prefix2, level2newline, initial_indent);
            string strDiploShift =      HandleArrayItems(rDef.DiplomacyShift, Render, prefix2, level2newline, initial_indent);
            string strUnlocks =         HandleArrayItems(rDef.Unlocks, Render, prefix2, level2newline, initial_indent);
            string strInvalidatedBy =   HandleArrayItems(rDef.InvalidatedBy, Render, prefix2, level2newline, initial_indent);
            string strCosts =           HandleArrayItems(rDef.Costs, Render, prefix2, level2newline, initial_indent);
            string strTags =            HandleArrayItems(rDef.Tags, Render, prefix2, level2newline, initial_indent);
            string strValidFor =        HandleArrayItems(rDef.ValidForFactions, Render, prefix2, level2newline, initial_indent);
            string strUnlockReq =       HandleArrayItems(rDef.UnlockRequirements.Container, Render, prefix2, level2newline, initial_indent);
            string strRevealReq =       HandleArrayItems(rDef.RevealRequirements.Container, Render, prefix2, level2newline, initial_indent);

            #region reprStr construction (multiline)
            string reprStr = $"===== RESEARCHDEF REPR BEGINS =====" +
                $"\n{prefix}rdef:       {rDef.name} {{{rDef.Guid}}}" +
                $"\n{prefix}ID:         {rDef.Id}" +
                $"\n{prefix}LearnCost:  {rDef.ResearchCost}" +
                $"\n{prefix}Faction:    {rDef.Faction}" +
                $"\n{prefix}DLC Off:    {strNotForDLC}" +
                $"\n{prefix}DLC:        {rDef.DLC}" +
                $"\n{prefix}Cutscene:   {rDef.TriggerCutscene}" +
                $"\n{prefix}InitState:  {strInitialStates}" +
                $"\n{prefix}Diplomacy:  {strDiploShift}" +
                $"\n{prefix}RSC Reward: {rDef.Resources}" +
                $"\n{prefix}Unlocks:    {strUnlocks}" +
                $"\n{prefix}Invalid by: {strInvalidatedBy}" +
                $"\n{prefix}Costs:      {strCosts}" +
                $"\n{prefix}Priority:   {rDef.Priority}" +
                $"\n{prefix}ViewElDef:  {rDef.ViewElementDef}" +
                $"\n{prefix}Tags:       {strTags}" +
                $"\n{prefix}ValidFor:   {strValidFor}" +
                $"\n{prefix}UnlockReq:  {rDef.UnlockRequirements.Operation}: {strUnlockReq}" +
                $"\n{prefix}RevealReq:  {rDef.RevealRequirements.Operation}: {strRevealReq}" +
                $"\n===== RESEARCHDEF REPR ENDS =====";
            #endregion

            return reprStr;
        }

        public static string Render(this Base.Platforms.EntitlementDef dlcDef, string prefix = "", bool newline = false, int depth = 0)
        {
            if (dlcDef is null) return "null";
            return dlcDef.Name.Localize();
        }

        public static string Render(this ResearchDef.InitialResearchState state, string prefix = "", bool newline = false, int depth = 0)
        {
            return $"{state.Faction.GetPPName(),-10} - {state.State}";
        }

        public static string Render(this DiplomacyRelation relation, string prefix = "", bool newline = false, int depth = 0)
        {
            if (relation is null) return "null";
            return
                $"{relation.WithFaction.GetPPName(),-10} - " +
                $"{relation.FactionDiplomacy} - " +
                $"{relation.LeaderDiplomacy}";
        }

        public static string Render(this ResearchRewardDef reward, string prefix = "", bool newline = false, int depth = 0)
        {
            if (reward is null) return "";
            return reward.name;
        }

        public static string Render(this ResearchDef research, string prefix = "", bool newline = false, int depth = 0)
        {
            if (research is null) return "null";
            return $"{research.name} {{{research.Guid}}}";
        }

        public static string Render(this ResearchCostDef costDef, string prefix = "", bool newline = false, int depth = 0)
        {
            if (costDef is null || costDef.LocalizationText is null) return "null";
            return $"{costDef.LocalizationText.Localize()}: {costDef.Amount}";
        }

        public static string Render(this ResearchTagDef tag, string prefix = "", bool newline = false, int depth = 0)
        {
            if (tag is null) return "null";
            return tag.name;
        }

        public static string Render(this GeoFactionDef faction, string prefix = "", bool newline = false, int depth = 0)
        {
            if (faction is null) return "null";
            return faction.GetPPName();
        }

        public static string Render(this ReseachRequirementDefOpContainer arg, string prefix = "", bool newline = false, int depth = 0)
        {
            return $"{arg.Operation}: {HandleArrayItems(arg.Requirements, Render, prefix, newline, depth)}";
        }

        public static string Render(this ResearchRequirementDef arg1, string prefix = "", bool newline = false, int depth = 0)
        {
            return arg1.name;
        }

        public static string HandleArrayItems<T>(IList<T> arr, Func<T, string, bool, int, string> Render, string prefix = "", bool newline = true, int depth = 0)
        {
            char[] tailend = new[] { ',', ' '};
            string inner = "";
            depth++;
            string localPrefix = string.Concat(Enumerable.Repeat(prefix, depth));
            string outerPrefix = string.Concat(Enumerable.Repeat(prefix, Math.Max(depth - 1, 0)));

            if (!(arr is null)) {
                localPrefix = (arr.Count > 1) ? localPrefix : "";
                foreach (T item in arr) {
                    if (newline && arr.Count > 1) inner += "\n";
                    inner += $"{localPrefix}{Render(item, localPrefix, newline, depth)}, ";
                }
                inner = inner.TrimEnd(tailend);
                if (arr.Count > 0) inner = $" {inner} ";
                if (arr.Count > 1 && newline) inner += $"\n{outerPrefix}";
            }

            return $"[{inner}]";
        }
    }
}
