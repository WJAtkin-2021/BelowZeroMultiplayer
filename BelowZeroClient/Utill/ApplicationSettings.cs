using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using UnityEngine;

namespace BelowZeroClient
{
    public class ApplicationSettings
    {
        private static string SUB_KEY = "SOFTWARE\\WillAtkin\\BelowZeroClient";

        public static void SaveSocket(string _socket)
        {
            RegistryKey userPrefs = GetKey(SUB_KEY);

            userPrefs.SetValue("Socket", _socket);
            userPrefs.Close();
        }

        public static string LoadSocket()
        {
            RegistryKey userPrefs = GetKey(SUB_KEY);
            return (string)userPrefs.GetValue("Socket", "127.0.0.1:5000");
        }

        public static void SavePlayerName(string _playerName)
        {
            RegistryKey userPrefs = GetKey(SUB_KEY);

            userPrefs.SetValue("PlayerName", _playerName);
            userPrefs.Close();
        }

        public static string LoadPlayerName()
        {
            RegistryKey userPrefs = GetKey(SUB_KEY);
            return (string)userPrefs.GetValue("PlayerName", "");
        }

        private static RegistryKey GetKey(string key)
        {
            RegistryKey userPrefs = Registry.CurrentUser.OpenSubKey(key, true);
            if (userPrefs == null)
            {
                userPrefs = Registry.CurrentUser.CreateSubKey(key);
            }
            return userPrefs;
        }
    }
}
