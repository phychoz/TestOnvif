using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TestOnvif
{
    public enum EnumLoggerType { Config, LogFile, WindowsEvent, Output, DebugLog };

    public static class Logger
    {

        public delegate void VerboseEventHandler(string message);
        public static event VerboseEventHandler Verbose;

        private static EnumLoggerType loggerType = EnumLoggerType.LogFile;
        private static string strLogFilePath = string.Empty;
        private static bool bLoggingEnabled = false;
        //private static StreamWriter sw = null;

        // If the logfile path is null then it will update error info into LogFile.txt under application directory.
        public static string LogFilePath
        {
            set { strLogFilePath = value; }
            get { return strLogFilePath; }
        }

        // Type of Logger
        public static EnumLoggerType LoggerType
        {
            set { loggerType = value; }
            get { return loggerType; }
        }

        // Logging status
        public static bool LoggingEnabled
        {
            set { bLoggingEnabled = value; }
            get { return bLoggingEnabled; }
        }

        private static void OnVerbose(string message)
        {
            if (Verbose != null)
            {
                Verbose(message);
            }
        }
        // Write exception log entry
        public static bool Write(Exception objException, EnumLoggerType type = EnumLoggerType.Config)
        {
            if (bLoggingEnabled == false)
                return false;

            if (type == EnumLoggerType.Config)
                type = loggerType;

            try
            {
                // Write to Windows event log
                if (type == EnumLoggerType.WindowsEvent)
                {
                    string EventLogName = "Exception";

                    if (!EventLog.SourceExists(EventLogName))
                        EventLog.CreateEventSource(objException.Message, EventLogName);

                    EventLog Log = new EventLog();
                    Log.Source = EventLogName;
                    Log.WriteEntry(objException.Message, EventLogEntryType.Error);
                }

                // Custom text-based event log
                if (type == EnumLoggerType.LogFile)
                {
                    string path = GetLogFilePath();

                    if (path != string.Empty)
                    {
                        StreamWriter sw = new StreamWriter(path, true);

                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine("Source: " + objException.Source.Trim());
                        sb.AppendLine("Method: " + objException.TargetSite.Name);
                        sb.AppendLine(String.Format("DateTime: {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
                        sb.AppendLine("Error: " + objException.Message.Trim());
                        sb.AppendLine("Stack Trace: " + objException.StackTrace.Trim());
                        //sb.AppendLine(new string('-', 50));

                        sw.WriteLine(sb);

                        //sw.WriteLine("Source: " + objException.Source.Trim());
                        //sw.WriteLine("Method: " + objException.TargetSite.Name);
                        //sw.WriteLine(String.Format("DateTime: {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
                        //sw.WriteLine("Error: " + objException.Message.Trim());
                        //sw.WriteLine("Stack Trace: " + objException.StackTrace.Trim());
                        //sw.WriteLine("-------------------------------------------------------------------");

                        sw.Flush();
                        sw.Close();

                        OnVerbose(sb.ToString());
                    }
                }

                if (type == EnumLoggerType.Output)
                {

                    Console.WriteLine("-------------------------------------------------------------------");
                    Console.WriteLine("Source: " + objException.Source.Trim());
                    Console.WriteLine("Method: " + objException.TargetSite.Name);
                    Console.WriteLine("Time: " + DateTime.Now.ToLongTimeString());
                    Console.WriteLine("Error: " + objException.Message.Trim());
                    Console.WriteLine("Stack Trace: " + objException.StackTrace.Trim());
                    Console.WriteLine("-------------------------------------------------------------------");
                }



                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // Write string
        public static bool Write(string text, EnumLoggerType type = EnumLoggerType.Config)
        {
            if (bLoggingEnabled == false)
                return false;

            if (type == EnumLoggerType.Config)
                type = loggerType;

            try
            {
                if (type == EnumLoggerType.WindowsEvent)
                {
                    string EventLogName = "Message";

                    if (!EventLog.SourceExists(EventLogName))
                        EventLog.CreateEventSource(text, EventLogName);

                    EventLog Log = new EventLog() { Source = EventLogName };

                    Log.WriteEntry(text, EventLogEntryType.Information);
                }

                if (type == EnumLoggerType.LogFile)
                {
                    string path = GetLogFilePath();

                    if (path != string.Empty)
                    {
                        StreamWriter sw = new StreamWriter(path, true);
                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine(String.Format("DateTime: {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
                        sb.AppendLine(String.Format("Text: {0}", text.Trim()));
                        //sb.AppendLine(new string('-', 100));

                        sw.WriteLine(sb);

                        sw.Flush();
                        sw.Close();

                        OnVerbose(sb.ToString());

                    }
                }

                if (type == EnumLoggerType.Output)
                {
                    if (Verbose != null)
                        Verbose(text + Environment.NewLine);

                    //Console.WriteLine(String.Format("{0} - {1}", DateTime.Now.ToLongTimeString(), text.Trim()));
                }

                if (type == EnumLoggerType.DebugLog)
                {
                    string path = GetLogFilePath();

                    if (path != string.Empty)
                    {
                        StreamWriter sw = new StreamWriter(path, true);

                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine(String.Format("{0}; {1}; ", DateTime.Now.ToString("HH:mm:ss.ffff"), text.Trim()));
                        //sb.AppendLine(new string('-', 100));

                        //sw.WriteLine(String.Format("Text: {0}", text.Trim()));
                        //sw.WriteLine("-------------------------------------------------------------------");

                        sw.WriteLine(sb);

                        sw.Flush();
                        sw.Close();

                        OnVerbose(sb.ToString());
                      }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetLogFilePath()
        {
            string strPathName = string.Empty;
            if (strLogFilePath.Equals(string.Empty))
            {
                //string baseDir = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.RelativeSearchPath;

                string retFilePath = "LogFile.txt";

                if (File.Exists(retFilePath) == true)
                    strPathName = retFilePath;
                else
                {
                    if (CheckDirectory(retFilePath) == false)
                        strPathName = string.Empty;
                    else
                        strPathName = retFilePath;

                    FileStream fs = new FileStream(retFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }
            }
            else
            {
                if (File.Exists(strLogFilePath) == false)
                {
                    if (CheckDirectory(strLogFilePath) == false)
                        return string.Empty;

                    FileStream fs = new FileStream(strLogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }
                strPathName = strLogFilePath;
            }

            return strPathName;
        }

        private static bool CheckDirectory(string strLogPath)
        {
            try
            {
                int nFindSlashPos = strLogPath.Trim().LastIndexOf("\\");
                string strDirectoryname = strLogPath.Trim().Substring(0, nFindSlashPos);

                if (Directory.Exists(strDirectoryname) == false)
                    Directory.CreateDirectory(strDirectoryname);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

