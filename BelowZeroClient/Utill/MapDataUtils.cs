using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using static SaveLoadManager;
using UWE;

namespace BelowZeroClient
{
    public class MapDataUtils
    {
        public static string SaveMapData(byte[] mapData)
        {
            string[] outPath = { AppDomain.CurrentDomain.BaseDirectory, "SNAppData", "SavedGames", "MultiplayerSave" };
            if (PlatformServicesEpic.IsPresent())
            {
                outPath[0] = Application.persistentDataPath;
                outPath[1] = "SubnauticaZero";
            }
            string outDirectoryPath = Path.Combine(outPath);
            if (Directory.Exists(outDirectoryPath))
                Directory.Delete(outDirectoryPath, true);

            Directory.CreateDirectory(outDirectoryPath);
            string[] outPath2 = { outDirectoryPath, "world.zip" };
            string outZipPath = Path.Combine(outPath2);
            File.WriteAllBytes(outZipPath, mapData);
            ZipFile.ExtractToDirectory(outZipPath, outDirectoryPath);
            File.Delete(outZipPath);

            //outDirectoryPath = Path.Combine(outDirectoryPath, "world");

            return outDirectoryPath;
        }

        public static JObject LoadGameInfoFromSavefile(string saveFolderName)
        {
            string gameInfoPath = Path.Combine(saveFolderName, "gameinfo.json");
            return JObject.Parse(File.ReadAllText(gameInfoPath));
        }
    }
}
