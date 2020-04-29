using System;
using System.Collections.Generic;
using System.Linq;
using Base.Defs;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Geoscape.Entities.Research;
using PhoenixPoint.Geoscape.Entities.Research.Cost;
using PhoenixPoint.Geoscape.Entities.Research.Reward;
using PhoenixPoint.Geoscape.Levels;
using PhoenixPoint.Tactical.Entities.DamageKeywords;

namespace ModnixUtils
{
    public class BasicUtil
    {
        public static void Log(object input, Func<string, object, object> api)
        {
            if (api is null) return;
            api(ModnixAPIActions.Log.info, input);
        }
    }


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
            string prefix2 = "";
            bool level2newline = false;
            string strNotForDLC = HandleArrayItems(rDef.NotEnabledForDLC, RenderEntitlement, prefix2, level2newline);
            string strInitialStates =   HandleArrayItems(rDef.InitialStates, RenderInitalState, prefix2, level2newline);
            string strDiploShift =      HandleArrayItems(rDef.DiplomacyShift, RenderDiplomaticShift, prefix2, level2newline);
            string strUnlocks =         HandleArrayItems(rDef.Unlocks, RenderUnlock, prefix2, level2newline);
            string strInvalidatedBy =   HandleArrayItems(rDef.InvalidatedBy, RenderResearch, prefix2, level2newline);
            string strCosts =           HandleArrayItems(rDef.Costs, RenderCost, prefix2, level2newline);
            string strTags =            HandleArrayItems(rDef.Tags, RenderTag, prefix2, level2newline);
            string strValidFor =        HandleArrayItems(rDef.ValidForFactions, RenderFaction, prefix2, level2newline);

            string reprStr = $"===== RESEARCHDEF REPR BEGINS =====" +
                $"\n{prefix}rdef:       {rDef.name}" +
                $"\n{prefix}Faction:    {rDef.Faction}" +
                $"\n{prefix}DLC Off:    {strNotForDLC}" +
                $"\n{prefix}DLC:        {rDef.DLC}" +
                $"\n{prefix}Cutscene:   {rDef.TriggerCutscene}" +
                $"\n{prefix}Init:       {strInitialStates}" +
                $"\n{prefix}Diplomacy:  {strDiploShift}" +
                $"\n{prefix}RSC Reward: {rDef.Resources}" +
                $"\n{prefix}Unlocks:    {strUnlocks}" +
                $"\n{prefix}Invalid by: {strInvalidatedBy}" +
                $"\n{prefix}Costs:      {strCosts}" +
                $"\n{prefix}Priority:   {rDef.Priority}" +
                $"\n{prefix}ViewElDef:  {rDef.ViewElementDef}" +
                $"\n{prefix}Tags:       {strTags}" +
                $"\n{prefix}ValidFor:   {strValidFor}" +
                $"\n{prefix}UnlockReq:  {rDef.UnlockRequirements}" +
                $"\n{prefix}RevealReq:  {rDef.RevealRequirements}" +
                $"\n{prefix}ID:         {rDef.Id}" +
                $"\n{prefix}ResCost:    {rDef.ResearchCost}" +
                $"";

            return reprStr;
        }

        public static string RenderEntitlement(Base.Platforms.EntitlementDef dlcDef)
        {
            if (dlcDef is null) return "null";
            return dlcDef.Name.Localize();
        }

        private static string RenderInitalState(ResearchDef.InitialResearchState state)
        {
            return $"{state.Faction.GetPPName(),-10} - {state.State}";
        }

        private static string RenderDiplomaticShift(DiplomacyRelation relation)
        {
            if (relation is null) return "null";
            return
                $"{relation.WithFaction.GetPPName(),-10} - " +
                $"{relation.FactionDiplomacy} - " +
                $"{relation.LeaderDiplomacy}";
        }

        private static string RenderUnlock(ResearchRewardDef reward)
        {
            if (reward is null) return "";
            return reward.name;
        }

        private static string RenderResearch(ResearchDef research)
        {
            if (research is null) return "null";
            return $"{research.name} {{{research.Guid}}}";
        }

        private static string RenderCost(ResearchCostDef costDef)
        {
            if (costDef is null || costDef.LocalizationText is null) return "null";
            return $"{costDef.LocalizationText.Localize()}: {costDef.Amount}";
        }

        private static string RenderTag(ResearchTagDef tag)
        {
            if (tag is null) return "null";
            return tag.name;
        }

        private static string RenderFaction(GeoFactionDef faction)
        {
            if (faction is null) return "null";
            return faction.GetPPName();
        }

        public static string HandleArrayItems<T>(IList<T> arr, Func<T, string> Render, string prefix = "", bool newline = true)
        {
            char[] tailend = new[] { ',', ' '};
            string inner = "";
            if (!(arr is null)) {
                foreach (T item in arr) {
                    if (newline) inner += "\n";
                    inner += $"{prefix}{Render(item)}, ";
                }
                inner = inner.TrimEnd(tailend);
                if (arr.Count > 0) inner = $" {inner} ";
                if (newline) inner += "\n";
            }

            return $"[{inner}]";
        }
    }
}
