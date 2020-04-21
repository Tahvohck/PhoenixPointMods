using System;

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
}
