using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public abstract class AbstractAppConfigBase
    {
        protected Dictionary<string, string> AppConfig = new Dictionary<string, string>();

        public virtual string GetConfig(string key)
        {
            string val;
            if (AppConfig.TryGetValue(key, out val))
                return val;
            else
                return null;
        }

        public virtual bool SetConfig(string key, string value)
        {
            AppConfig[key] = value;

            return true;
        }
    }
}
