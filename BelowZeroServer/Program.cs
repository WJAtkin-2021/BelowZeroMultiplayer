using System;

namespace BelowZeroServer
{
    internal class Program
    {
        private static Server m_server;

        static void Main(string[] args)
        {
            try
            {
                DataStore.CreateDataStore();
                UnlockManager.LoadUnlocks();
                Logger.Log($"Server GUID is: {DataStore.GetServerGuid()}");
                m_server = new Server();
                m_server.StartServer(5000);

                while (!m_server.m_isShuttingDown)
                {
                    string cmd = Console.ReadLine();
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        CommandRunner.RunCommand(cmd);
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
    }
}
