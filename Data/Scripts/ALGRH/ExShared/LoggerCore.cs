/*  ExShared/LoggerCore.cs
 *  Version: v2.3 (2022.07.08)
 * 
 * Core Framework for Logger.
 * This is the Core Logger used to write log file to a file in World Storage.
 * File name is partly configurable.
 * All log files have the same prefix "debug".
 * 
 *  Contributor
 *      Arime-chan
 */

using Sandbox.ModAPI;
using System;
using System.IO;
using VRage.Utils;

namespace ExShared
{
    public class Logger
    {
        public bool Suppressed
        {
            get { return m_Suppressed; }
            set
            {
                if (value == m_Suppressed)
                    return;

                if (value)
                {
                    m_Suppressed = false;
                    WriteLine(">> Log Suppressed <<");
                    m_Suppressed = true;
                }
                else
                {
                    m_Suppressed = false;
                    WriteLine(">> Log Unsuppressed <<");
                }
            }
        }
        public int LogLevel { get; set; } = 5;
        public ushort IndentLevel { get; protected set; } = 0;

        private string m_Prefix = "";
        private bool m_Suppressed = false;
        private TextWriter m_TextWriter = null;

        public Logger(string _name, string _prefix = "ExS")
        {
            LogLevel = 5;
            m_Prefix = "[" + _prefix + "]";

            string filename = "debug_" + _name;
            try
            {
                m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename + ".log", typeof(Logger));
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(m_Prefix + " > Exception < Problem encountered while initializing logger '" + filename + "': " + _e.Message);
            }

            WriteLine(">> " + m_Prefix + " Log Begin <<");
        }

        public void Close()
        {
            if (m_TextWriter != null)
            {
                m_Suppressed = false;
                WriteLine(">> " + m_Prefix + " Log End <<");
                m_TextWriter.Close();
            }
        }

        public void Write(string _message, int _level = 0)
        {
            if (m_Suppressed || _level > LogLevel)
                return;

            try
            {
                m_TextWriter.Write("[" + DateTime.Now.ToString("yy.MM.dd HH:mm:ss.fff") + "][" + _level + "]: " + _message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(m_Prefix + " > Exception < Problem encountered while logging: " + _e.Message);
                MyLog.Default.WriteLine(m_Prefix + "   Msg: " + _message);
            }
        }

        public void WriteLine(string _message, int _level = 0)
        {
            if (m_Suppressed || _level > LogLevel)
                return;

            _message = _message.PadLeft(_message.Length + (int)(IndentLevel * 2U), ' ');

            try
            {
                m_TextWriter.WriteLine("[" + DateTime.Now.ToString("yy.MM.dd HH:mm:ss.fff") + "][" + _level + "]: " + _message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(m_Prefix + " > Exception < Problem encountered while logging: " + _e.Message);
                MyLog.Default.WriteLine(m_Prefix + "   Msg: " + _message);
            }
        }

        public void WriteInline(string _message, int _level = 0, bool _breakNow = false)
        {
            if (m_Suppressed || _level > LogLevel)
                return;

            try
            {
                if (_breakNow)
                    m_TextWriter.WriteLine(_message);
                else
                    m_TextWriter.Write(_message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(m_Prefix + " > Exception < Problem encountered while logging: " + _e.Message);
                MyLog.Default.WriteLine(m_Prefix + "   Msg: " + _message);
            }
        }

        public void WriteCRLF(int _level = 0)
        {
            if (m_Suppressed || _level > LogLevel)
                return;

            try
            {
                m_TextWriter.WriteLine();
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(m_Prefix + " > Exception < Problem encountered while logging: " + _e.Message);
                MyLog.Default.WriteLine(m_Prefix + "   Msg: \\n");
            }
        }

        public void IndentIn()
        {
            if (IndentLevel < ushort.MaxValue)
                ++IndentLevel;
        }

        public void IndentOut()
        {
            if (IndentLevel > 0)
                --IndentLevel;
        }

        private string GetDateTimeAsString()
        {
            DateTime datetime = DateTime.Now;
            //DateTime datetime = DateTime.UtcNow + m_LocalUtcOffset.TimeSpan;
            return datetime.ToString("yy.MM.dd HH:mm:ss.fff");
        }

    }
}
