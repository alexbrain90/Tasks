using System;
using System.Windows.Forms;

namespace Tasks
{
    static partial class Program
    {
        static public bool isExiting = false;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Application.ThreadException += UnhandledThreadException;

            Control.CheckForIllegalCrossThreadCalls = false;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

         Config.AppExecutable = Application.ExecutablePath;
            Config.CurrentVersion = Application.ProductVersion;

            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(Config.AppExecutable)).Length != 1)
            {
                MessageBox.Show("Уже запущена другая копия программы", "Задачи - ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

         if (Update.Functions.CheckLaunch() == true)
         {
            new Update.Progress(3).ShowDialog();
            return;
         }

            clearReports();
            Config.ReadConfig();
            Config.ApplyConfig();
         Tray.InitTray();

         Threads.Connection.Start();
            Threads.CheckNews.Start();
            Threads.Popups.Start();
            Threads.Update.Start();

            Application.Run();
            isExiting = true;

            Threads.Update.Stop();
            Threads.Popups.Stop();
            Threads.CheckNews.Stop();
            Threads.Connection.Stop();

            Config.WriteConfig();
            Network.User_Exit();
            clearReports();

            Tray.ExitTray();
        }

        static private void clearReports()
        {
            // Clear old reports
            try
            {
                foreach (string oneFile in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Config.AppExecutable), "tmp????.xlsx"))
                {
                    System.IO.File.Delete(oneFile);
                }
            }
            catch { }
        }

        #region Catch unhandled errors
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
            MessageBox.Show("Критическая ошибка. Приложение будет закрыто\r\n\r\n" + ex.Message + "\r\n" + ex.Source + "\r\n" + ex.StackTrace, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter("Tasks-" + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") +  DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + DateTime.Now.Millisecond.ToString("000") + ".dump", false, System.Text.Encoding.UTF8);
                sw.WriteLine(ex.Message);
                sw.WriteLine(ex.Source);
                sw.WriteLine(ex.StackTrace);
                sw.Close();
            }
            catch { }

            SendException();

            Application.Exit();
        }
        private static void SendException()
        {
            foreach (string oneFile in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Config.AppExecutable), "*.dump"))
                Update.Functions.UploadDump(oneFile);
        }
        #endregion
    }
}
