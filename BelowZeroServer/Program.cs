using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                Thread.Sleep(1000);
                Console.WriteLine("Server shutdown, press any key to close console...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }

        static void HandleConsoleCommand(string cmd)
        {
            cmd = cmd.ToLower();

            if (cmd == "stop")
            {
                Console.WriteLine("Server shutting down");
                m_server.StopServer();
                m_serverClosing = true;
            }
            else if (cmd == "droptest")
            {
                Console.WriteLine("Its raining NutrientBlock");

                for (float x = -315.0f; x < -300.0f; x += 1.0f)
                {
                    for (float z = 245.0f; z < 265.0f; z += 1.0f)
                    {
                        NetSend.PlayerDroppedItem(0, "NutrientBlock", new Vector3(x, 25.0f, z), Guid.NewGuid().ToString());
                    }
                }
            }
        }
    }
}
