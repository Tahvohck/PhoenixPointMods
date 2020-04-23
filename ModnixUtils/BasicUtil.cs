using System;
using PhoenixPoint.Tactical.Entities.DamageKeywords;

namespace ModnixUtils
{
    public class BasicUtil
    {
        public static void Log(object input, Func<string, object, object> api = null)
        {
            if (api is null) return;
            api(ModnixAPIActions.log.info, input);
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
