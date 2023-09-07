using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BelowZeroServer;

namespace BelowZeroServerGui
{
    public partial class BelowZeroServerGui : Form
    {
        private Server m_server;
        private int m_longestItem = 0;

        public BelowZeroServerGui()
        {
            CheckForIllegalCrossThreadCalls = false;

            InitializeComponent();
            AcceptButton = SendCmdButton;
            ConsoleOutput.Resize += HandleResizeEvent;

            RunServerProgram();
        }

        public void HandleLog(string _log)
        {
            ConsoleOutput.Items.Add(_log);
            ConsoleOutput.TopIndex = ConsoleOutput.Items.Count - 1;

            int itemSize = (int)(_log.Length * ConsoleOutput.Font.Size * 1.1f);
            if (itemSize > m_longestItem)
            {
                m_longestItem = itemSize;
                int newExtentSize = Math.Max(0, m_longestItem - (int)(ConsoleOutput.Size.Width / 2.0f));
                ConsoleOutput.HorizontalExtent = newExtentSize;
            }  
        }

        public void HandleClear()
        {
            ConsoleOutput.Items.Clear();
            m_longestItem = 0;
        }

        public void HandleResizeEvent(object sender, System.EventArgs e)
        {
            int newExtentSize = Math.Max(0, m_longestItem - (int)(ConsoleOutput.Size.Width / 2.0f));
            ConsoleOutput.HorizontalExtent = newExtentSize;
        }

        private void SendCmdButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CommandTextBox.Text))
                return;

            // Send command
            CommandRunner.RunCommand(CommandTextBox.Text);
            CommandTextBox.Text = "";
        }

        private async void RunServerProgram()
        {
            try
            {
                Logger.OnLogAdded += HandleLog;
                Logger.OnClearLogs += HandleClear;

                DataStore.CreateDataStore();
                UnlockManager.LoadUnlocks();
                Logger.Log($"Server GUID is: {DataStore.GetServerGuid()}");
                m_server = new Server();
                m_server.StartServer(5000);

                while (!m_server.m_isShuttingDown)
                {
                    await Task.Delay(100);
                }

                UnlockManager.SaveUnlocks();
                Logger.Log("Server shutdown complete, window will close automatically");
                Logger.WriteToFile();

                Logger.OnLogAdded -= HandleLog;
                Logger.OnClearLogs -= HandleClear;

                await Task.Delay(2000);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Log($"[RunServerProgram] Fatal Error: {ex}");
                m_server.StopServer();
                Logger.Log($"Server application terminated due to unhanded exception");
                Logger.Log($"Attempts to save player data have been made, window can be safely closed");
                Logger.WriteToFile();
            }
        }
    }
}
