using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class ClientConnection
    {
        public const int DATA_BUFFER_SIZE = 4096;

        public int m_clientId;
        public string m_clientName = "";
        public TCP m_tcp;
        public UDP m_udp;

        public Vector3 m_lastPos;
        public Quaternion m_lastRot;
        public bool m_isInside;

        public ClientConnection(int _clientId, string _clientName)
        {
            m_clientId = _clientId;
            m_clientName = _clientName;
            m_tcp = new TCP(m_clientId, DATA_BUFFER_SIZE);
            m_udp = new UDP(m_clientId);
        }

        public void Disconnect()
        {
            if (m_tcp.m_tcpClient != null)
            {
                SaveData();

                try
                {
                    Logger.Log($"{m_tcp.m_tcpClient.Client.RemoteEndPoint} has disconnected");
                    NetSend.PlayerDisconnected(m_clientId);
                }
                catch (Exception ex)
                {
                    Logger.Log($"[ClientConnection:Disconnect] Error while disconnecting client: {ex}");
                }
                finally
                {
                    m_tcp.DisconnectTcp();
                }
            }

            m_udp.DisconnectUdp();
        }

        public void SetLastPos(Vector3 _pos, Quaternion _rot, bool _isInside)
        {
            m_lastPos = _pos;
            m_lastRot = _rot;
            m_isInside = _isInside;
        }

        public void SaveData()
        {
            PlayerSaveData saveData = new PlayerSaveData();
            saveData.Pos = m_lastPos;
            saveData.Rot = m_lastRot;
            saveData.IsInside = m_isInside;
            DataStore.SavePlayerData(saveData, m_clientName);
        }
    }

    public class PlayerSaveData
    {
        public Vector3 Pos = new Vector3();
        public Quaternion Rot = new Quaternion();
        public bool IsInside = false;
    }
}
