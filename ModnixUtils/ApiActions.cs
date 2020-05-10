using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModnixUtils
{
    /// <summary>
    /// Actions for use in API function
    /// </summary>
    public struct ModnixAPIActions
    {
        /// <summary>
        /// <para>Spec: Name of API to add</para>
        /// <para>Param: API Function</para>
        /// <para>Return: True on success</para>
        /// </summary>
        public const string api_add     = "api_add";
        public const string api_info    = "api_info";
        public const string api_list    = "api_list";
        public const string api_remove  = "api_remove";
        public const string assembly    = "assembly";
        public const string assemblies  = "assemblies";
        /// <summary>
        /// Work with the configuration.
        /// <para>Spec (default, ""): Returns deserialised object of same type</para>
        /// <para>Spec ("save", "write"): Save the provided object to the mod's config file.</para>
        /// </summary>
        [Obsolete("Use Config.load instead.")]
        public const string config      = "config";
        public const string dir         = "dir";
        public const string logger      = "logger";
        public const string mod_info    = "mod_info";
        public const string mod_list    = "mod_list";
        public const string path        = "path";
        public const string stacktrace  = "stacktrace";
        public const string version     = "version";

        /// <summary>
        /// Set of Log actions
        /// </summary>
        public struct Log
        {
            public const string normal      = "log";
            public const string critical    = "log crit";
            public const string error       = "log error";
            public const string warning     = "log warn";
            public const string info        = "log info";
            public const string verbose     = "log verbose";
            /// <summary>
            /// Special action that immediately flushes all queued messages to the log
            /// </summary>
            public const string flush       = "log flush";
        }

        /// <summary>
        /// set of config actions
        /// </summary>
        public struct Config
        {
            public const string load = "config";
            public const string save = "config save";
        }
    }
}
