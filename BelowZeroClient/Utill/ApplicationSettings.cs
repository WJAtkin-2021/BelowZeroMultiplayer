using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BelowZeroClient
{
    public class ApplicationSettings
    {
        private static string SUB_KEY = "SOFTWARE\\WillAtkin\\BelowZeroClient";

        public static void SaveSocket(string _socket)
        {
            RegistryKey userPrefs = Registry.CurrentUser.OpenSubKey(SUB_KEY, true);
            if (userPrefs == null)
            {
                userPrefs = Registry.CurrentUser.CreateSubKey(SUB_KEY);
            }

            userPrefs.SetValue("Socket", _socket);
            userPrefs.Close();
        }

        public static string LoadSocket()
        {
            RegistryKey userPrefs = Registry.CurrentUser.OpenSubKey(SUB_KEY, true);

            if (userPrefs != null)
            {
                return (string)userPrefs.GetValue("Socket");
            }
            else
            {
                return "127.0.0.1:5000";
            }
        }
    }
}
