using System;
using System.Windows.Forms;

namespace Tasks_Server
{
    static partial class Program
    {
        static public Network net;
        static public Log log;

        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Application.ThreadException += UnhandledThreadException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Control.CheckForIllegalCrossThreadCalls = false;

            Program.AppExecutable = Application.ExecutablePath;
            Program.CurrentVersion = Application.ProductVersion;

            if (ReadValues() == false)
            {
                // Show config form
            }

            log = new Log("log.txt", 1024*1024);
            log.WriteLine("Сервер запущен", false);

            net = new Network();
            net.Start();

            Application.Run(new Tasks_Server.Forms.Main());

            net.Abort();
            log.WriteLine("Сервер выключен", false);
            log.WriteLine("----------------------------------", false);
        }

        private static void UnhandledThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            CatchException(e.Exception);
        }
        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CatchException((Exception)e.ExceptionObject);
        }
        private static void CatchException(Exception ex)
        {
            Program.log.WriteLine(ex.Message + "\r\n" + ex.Source + "\r\n" + ex.StackTrace, true);
            Application.Restart();
        }

        private static void TempFunc()
        {
            object[] sql = SQL.getData("SELECT DateAdd, DateStart, DateEnd FROM Tasks");
            long id;
            DateTime dt1, dt2;
            for(int i =0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                id = (long)line[0];
                dt1 = new DateTime((long)line[1]);
                dt2 = new DateTime((long)line[2]);

                dt1 = new DateTime(dt1.Year, dt1.Month, dt1.Day);
                dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day).AddDays(1).AddTicks(-1);

                SQL.getData("UPDATE Tasks SET DateStart=\'" + dt1.Ticks.ToString() + "\', DateEnd=\'" + dt2.Ticks.ToString() + "\' WHERE DateAdd=\'" + id.ToString() + "\'");
            }
        }
    }

    /// <summary>
    /// Have info about connection
    /// </summary>
    class Connection
    {
        /// <summary>
        /// Connection ID
        /// </summary>
        public long Id;
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId;
        /// <summary>
        /// Time of last connect
        /// </summary>
        public long LastConnect;
    }
}
