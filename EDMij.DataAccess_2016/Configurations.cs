#region Namespaces
using System;
using System.Configuration;

#endregion

namespace Energie.DataAccess
{
    /// <summary>
    /// Class containing configuration constants
    /// </summary>
    public class Configurations
    {
        #region Constants

        public const string DEBUG = "_DEBUG";
        public const string TEST = "_TEST";
        public const string RELEASE = "_RELEASE";

        public const string STYLESHEET = "StyleSheet";

        #endregion

        #region Methods

        public static string GetApplicationSetting(string key)
        {
            string value = null;
#if DEBUG
            value = ConfigurationManager.AppSettings[key + Configurations.DEBUG];
#elif TEST
			value = ConfigurationManager.AppSettings[key + Configurations.TEST];
#else
            value = ConfigurationManager.AppSettings[key + Configurations.RELEASE];
#endif
            if (value == null)
            {
                value = ConfigurationManager.AppSettings[key];
            }

            return value;
        }

        

        #endregion
    }
}
