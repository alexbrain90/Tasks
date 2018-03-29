using System;
using System.IO;
using System.Text;

namespace Tasks_Server
{
    class Log
    {
        public string FileName { get; }
        public int MaxLength { get; }

        public Log(string Name, int Length)
        {
            try
            {
                FileName = Name;
                MaxLength = Length;

                StreamWriter sw = new StreamWriter(this.FileName, true, Encoding.UTF8);
                sw.Close();
            }
            catch { }
        }

        public bool WriteLine(string text, bool error)
        {
            DateTime time = DateTime.Now;
            riseLogEvent(time, text, error);

            if (ToFile(time, text, error) == true && ToDataBase(time, text, error) == true)
                return true;
            else
                return false;
        }
        private bool ToFile(DateTime time,string text, bool error)
        {
            try
            {
                string log = time.Year.ToString("0000") + "." + time.Month.ToString("00") + "." + time.Day.ToString("00") + " " + time.Hour.ToString("00") + ":" + time.Minute.ToString("00") + ":" + time.Second.ToString("00") + "." + time.Millisecond.ToString("000");
                if (error == true)
                    log += " # ";
                else
                    log += " ";
                log += text + "\r\n";

                StreamReader sr = new StreamReader(FileName, Encoding.Default);
                log += sr.ReadToEnd();
                sr.Close();

                StreamWriter sw = new StreamWriter(this.FileName, false, Encoding.Default);
                sw.Write(log);
                sw.Close();

                cutFile();

                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool ToDataBase(DateTime time, string text, bool error)
        {
            try
            {
                object sql = SQL.getData("INSERT INTO Log_Server VALUES (\'" + time.Ticks.ToString() + "\', \'" + text.Replace("\r\n", "%newline%") + "\', \'" + (error ? "1" : "0") + "\')");
                if (sql == null)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Cut log file if it became more than max size
        /// </summary>
        private void cutFile()
        {
            try
            {
                if (new FileInfo(FileName).Length > MaxLength)
                {
                    FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.SetLength(MaxLength);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch { }
        }

        public delegate void LogEventHandler(LogEventArgs e);
        public event LogEventHandler LogEvent;
        private void riseLogEvent(DateTime dateTime, string text, bool error)
        {
            LogEventHandler handler = LogEvent;
            LogEventArgs args = new LogEventArgs();
            args.dateTime = dateTime;
            args.text = text;
            args.error = error;
            handler?.Invoke(args);
        }
    }

    public class LogEventArgs : EventArgs
    {
        public DateTime dateTime { get; set; }
        public string text { get; set; }
        public bool error { get; set; }
    }
}