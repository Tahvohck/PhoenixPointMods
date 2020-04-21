using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaponModifiers
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
            api(ModnixAPIActions.log.info, "New SplashMod initialized");
        }

        /// <summary>
        /// Called after basic assets are loaded, before the hottest year cinematic. Virtually the same time as PPML.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#MainMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public void MainMod(Func<string, object, object> api = null)
        {
            api(ModnixAPIActions.log.info, "New MainMod initialized");
        }
    }
}
