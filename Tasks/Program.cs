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
            Application.SetCompatibleTextRenderingDefault(false);

            Config.AppExecutable = Application.ExecutablePath;
            Config.CurrentVersion = Application.ProductVersion;

            if (Update.checkLaunch(args) == true)
                return;

            System.IO.FileStream fs;
            try
            {
                fs = new System.IO.FileStream("lock.lck", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
            }
            catch
            {
                MessageBox.Show("Уже запущена другая копия программы", "Задачи - ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            clearReports();
            Tray.InitTray();
            Config.ReadConfig();
            Config.ApplyConfig();


            Threads.CheckNews.Start();
            new System.Threading.Thread(CheckUpdates).Start();

            Application.Run();
            isExiting = true;

            Threads.CheckNews.Stop();

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

        static private void CheckUpdates()
        {
            try
            {
                if (Tasks.Update.checkFiles() == false)
                {
                    Tray.ShowBaloon("Обновление", "Загружаются библиотеки для формирования отчетов");
                    if (Tasks.Update.downloadFiles() == true)
                        Tray.ShowBaloon("Обновление", "Библиотеки загружены. Теперь можно формировать отчеты");
                    else
                        Tray.ShowBaloon("Обновление", "Не удалось загрузить библиотеки. Формирование отчетов недоступно");
                }

                Tasks.Update.checkUpgrade();
                if (Config.CurrentVersion != Config.ServerVersion)
                {
                    Tray.SetStatusUpdate();
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
                Update.UploadDump(oneFile);
        }
        #endregion
    }
}
