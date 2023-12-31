﻿using System;
using System.Collections.Generic;
using System.IO;

namespace BelowZeroServer
{
    public class Logger
    {
        public static List<string> m_logs = new List<string>();

        public static void Log(string msg)
        {
            Console.WriteLine(msg);
            msg = AppendDateTime(msg);
            m_logs.Add(msg);
        }

        public static void SilentLog(string msg)
        {
            msg = AppendDateTime(msg);
            m_logs.Add(msg);
        }

        public static void WriteToFile()
        {
            File.WriteAllLines("Log.txt", m_logs);
        }

        private static string AppendDateTime(string msg)
        {
            string timeFormatted = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $"[{timeFormatted}] {msg}";
        }
    }
}
