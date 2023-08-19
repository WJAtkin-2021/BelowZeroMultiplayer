using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Xml;
using System.Data.Common;
using System.Data.SqlClient;

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

        public static PlayerSaveData GetPlayerData()
        {
            return new PlayerSaveData();
        }

        private static void SavePlayerData(PlayerSaveData _playerSaveData)
        {

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
            ExecuteNonQuery("CREATE TABLE PlayerPos (PlayerGuid TEXT, xPos REAL, yPos REAL, zPos REAL, xRot REAL, yRot REAL, zRot REAL, wRot REAL)");
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
    }
}
