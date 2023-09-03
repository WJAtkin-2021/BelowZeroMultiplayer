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
            try
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
                    UnlockManager.AddPdaEntry("TwistyBridgesMushroom", 3099);
                    NetSend.PlayerUnlockedPDAEncyclopedia(0, "TwistyBridgesMushroom", 3099);
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
                else if (cmd.StartsWith("giveitem"))
                {
                    // TODO: Make this function also except the item names in place of the
                    // tech type enum

                    string[] subparts = cmd.Split(null);
                    if (subparts.Length != 4)
                    {
                        Logger.Log($"Formatting error when calling 'giveitem' Usage:");
                        Logger.Log($"giveitem -playerName -itemTechType -itemQty");
                        return;
                    }

                    string playerName = subparts[1];
                    string fullPlayerName = Server.ResolvePartialPlayerName(playerName);
                    int clientId;
                    if (!string.IsNullOrEmpty(fullPlayerName))
                    {
                        clientId = Server.ResolveClientId(fullPlayerName);
                        if (clientId == -1)
                        {
                            // This should never happen but we will guard against it anyways...
                            Logger.Log($"Error! Unable to resolve player name: {fullPlayerName} to client ID!");
                            return;
                        }
                    }
                    else
                    {
                        Logger.Log($"Error! Unable to resolve player name: {playerName}");
                        return;
                    }


                    int itemTechType;
                    if (!int.TryParse(subparts[2], out itemTechType))
                    {
                        Logger.Log($"Formatting error! Unable to parse item tech type into integer");
                        return;
                    }

                    int itemQty;
                    if (!int.TryParse(subparts[3], out itemQty))
                    {
                        Logger.Log($"Formatting error! Unable to parse item qty into integer");
                        return;
                    }

                    if (itemQty == 1)
                        Logger.Log($"Giving {fullPlayerName} item with ID: {itemTechType}");
                    else
                        Logger.Log($"Giving {fullPlayerName} {itemQty} items with ID: {itemTechType}");

                    NetSend.AddInventoryItem(clientId, itemTechType, itemQty);
                }
                else if (cmd.StartsWith("unlock"))
                {
                    string[] subparts = cmd.Split(null);
                    if (subparts.Length != 2)
                    {
                        Logger.Log($"Formatting error when calling 'unlock' Usage:");
                        Logger.Log($"unlock -TechType");
                        return;
                    }

                    int itemTechType;
                    if (!int.TryParse(subparts[1], out itemTechType))
                    {
                        Logger.Log($"Formatting error! Unable to parse item tech type into integer");
                        return;
                    }

                    NetSend.ForceTechUnlock(itemTechType);
                }
                else if (cmd == "runtestunlocks")
                {
                    // Blueprints
                    NetSend.ForceTechUnlock(509);
                    NetSend.ForceTechUnlock(1001);
                    NetSend.ForceTechUnlock(1044);

                    foreach (var client in Server.m_instance.m_clients)
                    {
                        if (client.Value.IsConnected())
                        {
                            // Items
                            NetSend.AddInventoryItem(client.Value.m_clientId, 751, 1);
                            NetSend.AddInventoryItem(client.Value.m_clientId, 752, 1);
                            NetSend.AddInventoryItem(client.Value.m_clientId, 504, 2);
                            NetSend.AddInventoryItem(client.Value.m_clientId, 509, 1);
                            NetSend.AddInventoryItem(client.Value.m_clientId, 805, 1);
                            NetSend.AddInventoryItem(client.Value.m_clientId, 804, 1);
                        }
                    }
                }
                else
                {
                    Logger.Log($"Invalid Command: {cmd}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while running command: {ex}");
            }
        }
    }
}
