using System;
using PhoenixPoint.Tactical.Entities.DamageKeywords;

namespace ModnixUtils
{
    public class BasicUtil
    {
        public static void Log(object input, Func<string, object, object> api)
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
    }
}
