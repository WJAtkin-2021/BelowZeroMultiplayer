using System;
using BelowZeroMultiplayerCommon;

namespace BelowZeroServer
{
    public static class CommandRunner
    {
        private static int Debug_currentFragments = 0;

        public static void RunCommand(string _cmd)
        {
            try
            {
                _cmd = _cmd.ToLower();

                Logger.SilentLog($"Command entered: {_cmd}");

                if (_cmd == "stop")
                {
                    StopServer();
                }
                else if (_cmd == "clear")
                {
                    ClearConsole();
                }
                else if (_cmd == "droptest")
                {
                    RunDropTest();
                }
                else if (_cmd == "fragtest")
                {
                    RunFragmentTest();
                }
                else if (_cmd == "pdatest")
                {
                    RunPdaTest();
                }
                else if (_cmd == "blueprinttest")
                {
                    RunBlueprintTest();
                }
                else if (_cmd == "testplayer")
                {
                    CreateTestPlayer();
                }
                else if (_cmd == "testplayerrip")
                {
                    DestroyTestPlayer();
                }
                else if (_cmd.Contains("send "))
                {
                    BroadcastMessage(_cmd);
                }
                else if (_cmd.Contains("savetest"))
                {
                    RunSaveTest();
                }
                else if (_cmd.StartsWith("giveitem"))
                {
                    GiveItem(_cmd);
                }
                else if (_cmd.StartsWith("unlock"))
                {
                    UnlockTech(_cmd);
                }
                else if (_cmd == "testunlocks")
                {
                    GiveTestUnlocks();
                }
                else if (_cmd == "tokentest")
                {
                    RunTokenTest();
                }
                else if (_cmd.StartsWith("removetoken"))
                {
                    RunRemoveTokenTest(_cmd);
                }
                else
                {
                    Logger.Log($"Invalid Command: {_cmd}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[CommandRunner] Error while running command: {ex}");
            }
        }

        #region NormalCommands

        private static void StopServer()
        {
            Logger.Log("Server shutting down");
            Server.m_instance.StopServer();
        }

        private static void ClearConsole()
        { 
            Console.Clear();
        }

        private static void GiveItem(string _cmd)
        {
            // TODO: Make this function also except the item names in place of the
            // tech type enum

            string[] subparts = _cmd.Split(null);
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

        private static void UnlockTech(string _cmd)
        {
            string[] subparts = _cmd.Split(null);
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

        private static void BroadcastMessage(string _cmd)
        {
            string messageToClients = _cmd.Substring(5);
            if (messageToClients.Length > 0)
            {
                NetSend.MessageBroadcast(messageToClients);
            }
            Logger.SilentLog($"Sending message: {messageToClients}");
        }

        #endregion

        #region DebugCommands

        private static void RunDropTest()
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

        private static void RunFragmentTest()
        {
            Logger.Log("Adding Seaglide fragment");
            Debug_currentFragments++;

            UnlockManager.UpdateFragment(1117, Debug_currentFragments);
            NetSend.PlayerUpdatedFragmentProgress(0, 1117, Debug_currentFragments);
        }

        private static void RunPdaTest()
        {
            Logger.Log("TwistyBridgesMushroom test");
            UnlockManager.AddPdaEntry("TwistyBridgesMushroom", 3099);
            NetSend.PlayerUnlockedPDAEncyclopedia(0, "TwistyBridgesMushroom", 3099);
        }

        private static void RunBlueprintTest()
        {
            Logger.Log("Seaglide test ride");
            UnlockManager.AddTechUnlock(751);
            NetSend.PlayerUnlockedTechKnowledge(0, 751, true, true);
        }

        private static void CreateTestPlayer()
        {
            Logger.Log("Adding Test Player");
            NetSend.PlayerSpawned(99, "Test Dude", new Vector3(-275.5f, -11.473f, -23.05f), new Quaternion(0.0f, 0.9997668f, 0.0f, -0.02159826f), true);
        }

        private static void DestroyTestPlayer()
        {
            Logger.Log("Removing Test Player");
            NetSend.PlayerDisconnected(99);
        }

        private static void RunSaveTest()
        {
            UnlockManager.SaveUnlocks();
        }

        private static void GiveTestUnlocks()
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

        private static void RunTokenTest()
        {
            TokenData data = new TokenData();
            data.associatedTechType = 16;
            data.position = new Vector3(-274.8f, -11.0f, -27.1f);
            data.rotation = new Quaternion();
            data.scale = new Vector3(1.0f, 1.0f, 1.0f);
            data.clientWithToken = 0; // TEST: 0 is server
            data.tokenGuid = Guid.NewGuid().ToString();
            data.networkedEntity = NetworkedEntityType.Pickupable;
            data.tokenExchangePolicy = TokenExchangePolicy.DoNotYield;
            data.tickRate = 0.0f;
            NetSend.PlayerAddedNewToken(data);
        }

        private static void RunRemoveTokenTest(string _cmd)
        {
            string[] subparts = _cmd.Split(null);
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
            string token = subparts[2];
            NetSend.DestroyToken(clientId, token);
        }

        #endregion
    }
}
