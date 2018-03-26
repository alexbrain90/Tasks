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
            object[] sql = SQL.getData("SELECT DateAdd FROM Tasks");
            long id;
            DateTime dt1, dt2;
            for(int i =0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                id = (long)line[0];

                SQL.getData("INSERT INTO History VALUES (\'" + id.ToString() + "\', \'0\', \'1\', \'0\', \'\', \'\')");
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
