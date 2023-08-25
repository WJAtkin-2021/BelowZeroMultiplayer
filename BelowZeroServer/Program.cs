using System;
using System.Threading;

namespace BelowZeroServer
{
    internal class Program
    {
        private static Server m_server;
        private static bool m_serverClosing = false;

        static void Main(string[] args)
        {
            try
            {
                DataStore.CreateDataStore();
                UnlockManager.LoadUnlocks();
                Logger.Log($"Server GUID is: {DataStore.GetServerGuid()}");
                m_server = new Server();
                m_server.StartServer(5000);

                while (!m_serverClosing)
                {
                    string cmd = Console.ReadLine();
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        HandleConsoleCommand(cmd);
                    }
                }

                UnlockManager.SaveUnlocks();
                Thread.Sleep(1000);
                Logger.WriteToFile();
                Console.WriteLine("Server shutdown, press any key to close console...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }

        private static int currentFrags = 0;
        static void HandleConsoleCommand(string cmd)
        {
            cmd = cmd.ToLower();

            Logger.SilentLog($"Command entered: {cmd}");

            if (cmd == "stop")
            {
                Logger.Log("Server shutting down");
                m_server.StopServer();
                m_serverClosing = true;
            }
            else if (cmd == "clear")
            {
                Console.Clear();
            }
            else if (cmd == "droptest")
            {
                Logger.Log("Its raining NutrientBlock");

                for (float x = -315.0f; x < -300.0f; x += 1.0f)
                {
                    for (float z = 245.0f; z < 265.0f; z += 1.0f)
                    {
                        NetSend.PlayerDroppedItem(0, "NutrientBlock", new Vector3(x, 25.0f, z), Guid.NewGuid().ToString());
                    }
                }
            }
            else if (cmd == "fragtest")
            {
                Logger.Log("Adding Seaglide fragment");
                currentFrags++;

                UnlockManager.UpdateFragment(1117, currentFrags);
                NetSend.PlayerUpdatedFragmentProgress(0, 1117, currentFrags);
            }
            else if (cmd == "pdatest")
            {
                Logger.Log("TwistyBridgesMushroom test");
                UnlockManager.AddPdaEntry("TwistyBridgesMushroom");
                NetSend.PlayerUnlockedPDAEncyclopedia(0, "TwistyBridgesMushroom");
            }
            else if (cmd == "blueprinttest")
            {
                Logger.Log("Seaglide test ride");
                UnlockManager.AddTechUnlock(751);
                NetSend.PlayerUnlockedTechKnowledge(0, 751, true, true);
            }
            else if (cmd == "testplayer")
            {
                Logger.Log("Adding Test Player");
                NetSend.PlayerSpawned(420, "Bing Bong", new Vector3(-275.5f, -11.473f, -23.05f), new Quaternion(0.0f, 0.9997668f, 0.0f, -0.02159826f), true);
            }
            else if (cmd == "testplayerrip")
            {
                Logger.Log("Removing Test Player");
                NetSend.PlayerDisconnected(420);
            }
            else if (cmd.Contains("send "))
            {
                string messageToClients = cmd.Substring(5);
                if (messageToClients.Length > 0)
                {
                    NetSend.MessageBroadcast(messageToClients);
                }
                Logger.SilentLog($"Sending message: {messageToClients}");
            }
            else if (cmd.Contains("savetest"))
            {
                UnlockManager.SaveUnlocks();
            }
            else
            {
                Logger.Log($"Invalid Command: {cmd}");
            }
        }
    }
}
