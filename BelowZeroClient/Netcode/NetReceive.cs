using BelowZeroClient.Utill;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Story;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;

namespace BelowZeroClient
{
    public class NetReceive
    {
        public static void Connected(Packet _packet)
        {
            // Read the packet
            int newClientId = _packet.ReadInt();
            string serverGuid = _packet.ReadString();

            ErrorMessage.AddMessage($"Assigned client ID: {newClientId}");

            NetworkClient.Instance.m_clientId = newClientId;

            // Tell the server we got it and to check our credentials
            NetSend.ConnectedReceived(serverGuid);
        }

        public static void HandleNewMachineToken(Packet _packet)
        {
            string serverGuid = _packet.ReadString();
            string machineToken = _packet.ReadString();

            ApplicationSettings.SaveCredentailToken(serverGuid, machineToken);
        }

        public static void HandleUserNameInUse(Packet _packet)
        {
            ErrorMessage.AddWarning("Username Is Taken, please try another...");
        }

        public static void PlayerDisconnected(Packet _packet)
        {
            int client = _packet.ReadInt();

            NetworkClient.Instance.m_remotePlayers[client].RemovePlayer();
        }

        public static void HandleSpawnPlayer(Packet _packet)
        {
            // Read the ID of the player that spawned
            int newClientId = _packet.ReadInt();
            string newClientName = _packet.ReadString();
            Vector3 newClientPos = _packet.ReadVector3();
            Quaternion newClientRot = _packet.ReadQuaternoin();
            bool newClientIsInside = _packet.ReadBool();

            if (newClientId != NetworkClient.Instance.m_clientId)
            {
                NetworkClient.Instance.AddRemotePlayer(newClientId, newClientName, newClientPos);
            }
            else
            {
                // This is us, so we need to find our player and teleport
                ReplicatePlayer.m_instance.Teleport(newClientPos, newClientRot, newClientIsInside);
            }
        }

        public static void HandleSycPlayerList(Packet _packet)
        {
            // Get the list length
            int numberOfClients = _packet.ReadInt();
            for (int i = 0; i < numberOfClients; i++)
            {
                int clientId = _packet.ReadInt();
                string clientName = _packet.ReadString();
                Vector3 position = _packet.ReadVector3();
                if (clientId != NetworkClient.Instance.m_clientId)
                {
                    NetworkClient.Instance.AddRemotePlayer(clientId, clientName, position);
                }
            }
        }

        public static void HandlePlayerTransformUpdate(Packet _packet)
        {
            int clientId = _packet.ReadInt();
            Vector3 pos = _packet.ReadVector3();
            Quaternion rot = _packet.ReadQuaternoin();

            if (NetworkClient.Instance.m_remotePlayers.ContainsKey(clientId))
            {
                NetworkClient.Instance.m_remotePlayers[clientId].UpdateTransform(pos, rot);
            }
        }

        public static void HandleMapData(Packet _packet)
        {
            // Start the UDP stream here...
            NetworkClient.Instance.StartUDPConnection();

            // Grab the length
            int length = _packet.ReadInt();

            // Read the map data
            byte[] mapData = _packet.ReadBytes(length);
            string mapLocation = MapDataUtils.SaveMapData(mapData);

            // Read the game info
            JObject mapInfo = MapDataUtils.LoadGameInfoFromSavefile(mapLocation);

            string session = mapInfo["session"].ToString();
            string changeSet = mapInfo["changeSet"].ToString();
            string gameModeId = mapInfo["gameModePresetId"].ToString();
            string storyVersion = mapInfo["storyVersion"].ToString();

            // We need to figure out what the game mode is as
            // the number that is saved in gamemode.json does not match the enum
            GameModePresetId gameModePreset = GameModePresetId.Survival;
            switch (gameModeId)
            {
                case "0":
                    {
                            gameModePreset = GameModePresetId.Survival;
                    }
                    break;
                case "1":
                    {
                        gameModePreset = GameModePresetId.Freedom;
                    }
                    break;
                case "2":
                    {
                        gameModePreset = GameModePresetId.Hardcore;
                    }
                    break;
                case "3":
                    {
                        gameModePreset = GameModePresetId.Creative;
                    }
                    break;
                case "100":
                    {
                        // TODO: Custom game modes are not yet supported so we set the game to survival for now...
                        gameModePreset = GameModePresetId.Survival;
                    }
                    break;
            }

            // TODO: Read the options from the file so we can support custom game modes
            GameOptions options = new GameOptions();

            // Ask the game to load this file
            CoroutineHost.StartCoroutine(uGUI_MainMenu.main.LoadGameAsync(mapLocation, session, int.Parse(changeSet), gameModePreset, options, int.Parse(storyVersion)));

            ErrorMessage.AddMessage($"Didn't crash :)");
        }

        public static void handlePlayerDroppedItem(Packet _packet)
        {
            string teckName = _packet.ReadString();
            Vector3 pos = _packet.ReadVector3();
            string token = _packet.ReadString();

            TechType techType = (TechType)Enum.Parse(typeof(TechType), teckName);

            CoroutineHost.StartCoroutine(CreateTechTypeAsyc.CreateNetworkedTechTypeAsyc(techType, pos, token, null));
        }

        public static void handlePlayerPickedUpItem(Packet _packet)
        {
            string token = _packet.ReadString();

            // TODO: Refactor this so that we store all networked pickupables in a list
            NetToken[] tokens = GameObject.FindObjectsOfType<NetToken>();
            foreach (NetToken tok in tokens)
            {
                if (tok.guid == token)
                {
                    UnityEngine.Object.Destroy(tok.gameObject);
                    return;
                }
            }
        }

        public static void HandlePlayerUnlockedTechKnowledge(Packet _packet)
        {
            TechType techType = (TechType)_packet.ReadInt();
            bool unlockEncyclopedia = _packet.ReadBool();
            bool verbose = _packet.ReadBool();

            TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType);
            if (techUnlockState != TechUnlockState.Available)
            {
                KnownTech.Add(techType, unlockEncyclopedia, verbose);
            }
        }

        public static void HandlePlayerUnlockedPDAEncyclopedia(Packet _packet)
        {
            string key = _packet.ReadString();
            PDAUnlockQueue.m_instance.UnlockDelayed(key);
        }

        public static void HandlePlayerUpdatedFragmentProgress(Packet _packet)
        {
            TechType techType = (TechType)_packet.ReadInt();

            PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);
            if (PDAScanner.GetPartialEntryByKey(techType, out PDAScanner.Entry entry))
            {
                entry.unlocked++;
                PDAScanner.onProgress.Invoke(entry);
            }
            else
            {
                // Really horrible but we need to get the PDA's data by serilizing it and then
                // pass it back to it for it to read
                PDAScanner.Data data = PDAScanner.Serialize();
                entry = new PDAScanner.Entry();
                entry.techType = techType;
                entry.unlocked = 1;
                data.partial.Add(entry);
                PDAScanner.onAdd.Invoke(entry);
            }

            int totalFragments = entryData.totalFragments;
            if (totalFragments > 1 && entryData.blueprint != 0)
            {
                float arg = Mathf.RoundToInt((float)entry.unlocked / (float)totalFragments * 100f);
                ErrorMessage.AddError(Language.main.GetFormat("ScannerInstanceScanned", Language.main.Get(techType.AsString()), arg, entry.unlocked, totalFragments));
            }
        }
    }
}
