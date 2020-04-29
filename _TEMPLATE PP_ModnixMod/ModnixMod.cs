﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$
{
    // Reminder: You will need to add a reference to Assembly-CSharp.dll in order to reference Phoenix Point classes.

    /// <summary>
    /// Put all configuration options here, Modnix will use this to create json configurations
    /// You can change the class to use for settings in Properties/mod_info.js
    /// Make sure to update mod_info if you change the namespace as well!
    /// </summary>
    public class ModConfig
    {

    }


    public class MyMod
    {
#pragma warning disable IDE0044 // Add readonly modifier (This prevents the IDE from whining about the lack of assignment)
    private static ModConfig Config;
#pragma warning restore IDE0044 // Add readonly modifier

    /// <summary>PPML v0.1 entry point</summary>
    public static void Init() => MainMod();

        /// <summary>
        /// Called very early, just after main assemblies are loaded, before logos. Saves have not been scanned and most game data are unavailable.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#SplashMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public static void SplashMod(Func<string, object, object> api = null)
        {
            // ADVICE: Don't use SplashMod unless you need it!
            // Loading things early in the game will slow down the game's initial load.
            api("log info", "New SplashMod initialized");
        }

        /// <summary>
        /// Called after basic assets are loaded, before the hottest year cinematic. Virtually the same time as PPML.
        /// Full info at https://github.com/Sheep-y/Modnix/wiki/DLL-Specs#MainMod
        /// </summary>
        /// <param name="api">First param (string) is the query/action. Second param (object) and result (object) varies by action.</param>
        public static void MainMod(Func<string, object, object> api = null)
        {
            // Un-comment this to be able to use the configuration class in your code.
            //Config = api("config", null) as ModConfig ?? new ModConfig();
            api("log info", "New MainMod initialized");
        }
    }
}
