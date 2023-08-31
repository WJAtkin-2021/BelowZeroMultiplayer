using BelowZeroMultiplayerCommon;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BelowZeroServer
{
    public class DataStore
    {
        private const string DATABASE_NAME = "datastore.sqlite";

        public static DataStore m_instance;
        private static SQLiteConnection m_connection;
        private static string m_serverGuid = "";

        public static void CreateDataStore()
        {
            if (m_instance == null)
            {
                m_instance = new DataStore();
                m_instance.OpenConnection();
            }
            else
            {
                Logger.Log("Tried to create a new data store when one already exists");
            }
        }

        public static string GetServerGuid()
        {
            if (m_serverGuid == "")
            {
                SQLiteCommand cmd = m_connection.CreateCommand();
                cmd.CommandText = "select ServerGuid from ServerSettings";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var serverGuid = reader.GetString(0);
                        return serverGuid;
                    }
                }

                throw new Exception("Could not find server guid in database");
            }
            else
            {
                return m_serverGuid;
            }
        }

        public static bool IsUserNameInUse(string _userName)
        {
            try
            {
                SQLiteCommand cmd = m_connection.CreateCommand();
                cmd.CommandText = $"select * from PlayerToken WHERE PlayerName = \"{_userName}\"";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }

            return false;
        }

        public static bool CheckCredentials(string _userName, string _userGuid)
        {
            if (_userGuid == "")
                return false;

            try
            {
                SQLiteCommand cmd = m_connection.CreateCommand();
                cmd.CommandText = $"select * from PlayerToken WHERE PlayerName = \"{_userName}\"";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var playerGuid = reader.GetString(1);
                        if (playerGuid == _userGuid)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return false;
            }
            catch(Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }

            return false;
        }

        public static void SaveCredentails(string _userName, string _userGuid)
        {
            ExecuteNonQuery($"INSERT INTO PlayerToken (PlayerName, PlayerGuid) VALUES (\"{_userName}\", \"{_userGuid}\")");
        }

        public static PlayerSaveData GetPlayerData(string _userName)
        {
            try
            {
                SQLiteCommand cmd = m_connection.CreateCommand();
                cmd.CommandText = $"select * from PlayerPos WHERE PlayerName = \"{_userName}\"";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var posX = reader.GetFloat(1);
                        var posY = reader.GetFloat(2);
                        var posZ = reader.GetFloat(3);
                        var rotX = reader.GetFloat(4);
                        var rotY = reader.GetFloat(5);
                        var rotZ = reader.GetFloat(6);
                        var rotW = reader.GetFloat(7);
                        var isInside = reader.GetBoolean(8);
                        
                        PlayerSaveData saveData = new PlayerSaveData();
                        saveData.Pos = new Vector3((int)posX, posY, posZ);
                        saveData.Rot = new Quaternion(rotX, rotY, rotZ, rotW);
                        saveData.IsInside = isInside;
                        return saveData;
                    }
                }

                return GenerateDefaultData();
            }
            catch (Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }

            return GenerateDefaultData();
        }

        public static void SavePlayerData(PlayerSaveData _playerSaveData, string _userName)
        {
            Vector3 pos = _playerSaveData.Pos;
            Quaternion rot = _playerSaveData.Rot;
            ExecuteNonQuery($"DELETE FROM PlayerPos WHERE PlayerName = \"{_userName}\"");
            ExecuteNonQuery($"INSERT INTO PlayerPos (PlayerName, xPos, yPos, zPos, xRot, yRot, zRot, wRot, isInside) VALUES (\"{_userName}\", {pos.x}, {pos.y}, {pos.z}, {rot.x}, {rot.y}, {rot.z}, {rot.w}, {_playerSaveData.IsInside})");
        }

        public static void SaveUnlockData(UnlockData _unlockData)
        {
            for (int i = 0; i < _unlockData.techUnlocks.Count; i++)
            {
                ExecuteNonQuery($"DELETE FROM TechTypeUnlocks WHERE techType = {_unlockData.techUnlocks[i]}");
                ExecuteNonQuery($"INSERT INTO TechTypeUnlocks (techType) VALUES ({_unlockData.techUnlocks[i]})");
            }

            foreach (KeyValuePair<string, int> entry in _unlockData.pdaEncyclopedia)
            {
                ExecuteNonQuery($"DELETE FROM PdaEntryUnlocks WHERE key = \"{entry.Key}\"");
                ExecuteNonQuery($"INSERT INTO PdaEntryUnlocks (key, techType) VALUES (\"{entry.Key}\", {entry.Value})");
            }

            foreach(KeyValuePair<string, FragmentKnowledge> entry in _unlockData.fragments)
            {
                ExecuteNonQuery($"DELETE FROM Fragments WHERE key = \"{entry.Key}\"");
                ExecuteNonQuery($"INSERT INTO Fragments (key, techType, current) VALUES (\"{entry.Key}\", {entry.Value.techType}, {entry.Value.parts})");
            }
        }

        public static UnlockData LoadUnlockData()
        {
            UnlockData unlockData = new UnlockData();

            try
            {
                SQLiteCommand selectTechCmd = m_connection.CreateCommand();
                selectTechCmd.CommandText = $"select * from TechTypeUnlocks";
                using (var reader = selectTechCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        unlockData.techUnlocks.Add(Int32.Parse(reader["techType"].ToString()));
                    }
                }

                SQLiteCommand selectPdaCmd = m_connection.CreateCommand();
                selectPdaCmd.CommandText = $"select * from PdaEntryUnlocks";
                using (var reader = selectPdaCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string key = reader.GetString(0);
                        int techType = reader.GetInt32(1);
                        unlockData.pdaEncyclopedia[key] = techType;
                    }
                }

                SQLiteCommand selectFragmentsCmd = m_connection.CreateCommand();
                selectFragmentsCmd.CommandText = $"select * from Fragments";
                using (var reader = selectFragmentsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string key = reader.GetString(0);
                        int techType = reader.GetInt32(1);
                        int current = reader.GetInt32(2);
                        FragmentKnowledge fragmentKnowledge = new FragmentKnowledge();
                        fragmentKnowledge.techType = techType;
                        fragmentKnowledge.parts = current;
                        unlockData.fragments[key] = fragmentKnowledge;
                    }
                }

                return unlockData;
            }
            catch (Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }

            return null;
        }

        public static void SaveInventoryData(InventoryData _inventoryData, string _userName)
        {
            List<string> quickSlots = new List<string>();
            for (int i = 0; i < _inventoryData.serializedQuickSlots.Length; i++)
            {
                quickSlots.Add(_inventoryData.serializedQuickSlots[i]);
            }
            string quickSlotsAsCSV = string.Join(",", quickSlots);

            List<string> equipMentSlots = new List<string>();
            foreach (KeyValuePair<string, string> entry in _inventoryData.serializedEquipmentSlots)
            {
                equipMentSlots.Add(entry.Key);
                equipMentSlots.Add(entry.Value);
            }
            string equipMentSlotsAsCSV = string.Join(",", equipMentSlots);

            ExecuteNonQuery($"DELETE FROM PlayerInventory WHERE PlayerName = \"{_userName}\"");

            SQLiteCommand cmd = m_connection.CreateCommand();
            cmd.CommandText =
            @"
                INSERT INTO PlayerInventory (PlayerName, storage, quickSlots, equipment, equipmentSlots, pendingItems)
                VALUES ($PlayerName, $storage, $quickSlots, $equipment, $equipmentSlots, $pendingItems)
            ";
            cmd.Parameters.AddWithValue("$PlayerName", _userName);
            cmd.Parameters.AddWithValue("$storage", Convert.ToBase64String(_inventoryData.serializedStorage));
            cmd.Parameters.AddWithValue("$quickSlots", quickSlotsAsCSV);
            cmd.Parameters.AddWithValue("$equipment", Convert.ToBase64String(_inventoryData.serializedEquipment));
            cmd.Parameters.AddWithValue("$equipmentSlots", equipMentSlotsAsCSV);
            cmd.Parameters.AddWithValue("$pendingItems", Convert.ToBase64String(_inventoryData.serializedPendingItems));
            ExecuteCommand(cmd);
        }

        public static InventoryData LoadInventoryData(string _userName)
        {
            try
            {
                SQLiteCommand cmd = m_connection.CreateCommand();
                cmd.CommandText = @"
                    select *
                    from PlayerInventory
                    WHERE PlayerName = $PlayerName
                ";
                cmd.Parameters.AddWithValue("$PlayerName", _userName);

                //cmd.CommandText = $"select * from PlayerInventory WHERE PlayerName = \"{_userName}\"";
                using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.KeyInfo))
                {
                    while (reader.Read())
                    {
                        // Read in all the data
                        string storage = reader.GetString(1);
                        string quickSlots = reader.GetString(2);
                        string equipment = reader.GetString(3);
                        string equipmentSlots = reader.GetString(4);
                        string pendingItems = reader.GetString(5);

                        // Put the data back into the original format
                        byte[] storageAsBytes = Convert.FromBase64String(storage);
                        string[] quickSlotsAsArray = quickSlots.Split(',');
                        byte[] equipmentAsBytes = Convert.FromBase64String(equipment);
                        byte[] pendingItemsAsBytes = Convert.FromBase64String(pendingItems);
                        string[] equipmentSlotsAsKeyPair = equipmentSlots.Split(',');
                        Dictionary<string, string> equipmentSlotsAsDictionary = new Dictionary<string, string>();
                        for (int i = 0; i < equipmentSlotsAsKeyPair.Length; i++)
                        {
                            string key = equipmentSlotsAsKeyPair[i];
                            i++;
                            string value = equipmentSlotsAsKeyPair[i];
                            equipmentSlotsAsDictionary.Add(key, value);
                        }

                        // Store in the inventory data
                        InventoryData data = new InventoryData();
                        data.serializedStorage = storageAsBytes;
                        data.serializedQuickSlots = quickSlotsAsArray;
                        data.serializedEquipment = equipmentAsBytes;
                        data.serializedEquipmentSlots = equipmentSlotsAsDictionary;
                        data.serializedPendingItems = pendingItemsAsBytes;
                        return data;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }

            return null;
        }

        private void OpenConnection()
        {
            m_connection = new SQLiteConnection($"Data Source={DATABASE_NAME}; Version = 3; New = True; Compress = True;");
            try
            {
                if (!File.Exists(DATABASE_NAME))
                {
                    Logger.Log($"Generating New Data Store");
                    SQLiteConnection.CreateFile(DATABASE_NAME);
                    m_connection.Open();
                    CreateInitalDataTables();
                }
                else
                {
                    m_connection.Open();
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }
        }

        private static void CreateInitalDataTables()
        {
            m_serverGuid = Guid.NewGuid().ToString();
            ExecuteNonQuery("CREATE TABLE ServerSettings (ServerGuid TEXT)");
            ExecuteNonQuery($"INSERT INTO ServerSettings (ServerGuid) VALUES (\"{m_serverGuid}\")");
            ExecuteNonQuery("CREATE TABLE PlayerToken (PlayerName Text, PlayerGuid TEXT)");
            ExecuteNonQuery("CREATE TABLE PlayerPos (PlayerName TEXT, xPos REAL, yPos REAL, zPos REAL, xRot REAL, yRot REAL, zRot REAL, wRot REAL, isInside INTEGER)");
            ExecuteNonQuery("CREATE TABLE PlayerInventory (PlayerName TEXT PRIMARY KEY, storage TEXT, quickSlots TEXT, equipment TEXT, equipmentSlots TEXT, pendingItems TEXT)");
            ExecuteNonQuery("CREATE TABLE TechTypeUnlocks (techType INTEGER)");
            ExecuteNonQuery("CREATE TABLE PdaEntryUnlocks (key TEXT, techType INTEGER)");
            ExecuteNonQuery("CREATE TABLE Fragments (key TEXT, techType INTEGER, current INTERGER)");
        }

        private static void ExecuteCommand(SQLiteCommand _cmd)
        {
            try
            {
                _cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }
        }

        private static void ExecuteNonQuery(string _nonQuery)
        {
            try
            {
                SQLiteCommand cmd = m_connection.CreateCommand();
                cmd.CommandText = _nonQuery;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Log($"SQL Error: {ex}");
            }
        }

        private static PlayerSaveData GenerateDefaultData()
        {
            PlayerSaveData saveData = new PlayerSaveData();
            saveData.Pos = new Vector3(-309.5f, 17.75f, 255.0f);
            return saveData;
        }
    }
}
