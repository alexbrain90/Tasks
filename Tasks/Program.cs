using System;
using System.Windows.Forms;

namespace Tasks
{
    static partial class Program
    {
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

            Program.AppExecutable = Application.ExecutablePath;
            Program.CurrentVersion = Application.ProductVersion;

            if (Update.checkLaunch(args) == true)
                return;

            clearReports();
            Application.Run(new Forms.Main());
            clearReports();
        }
        static private void clearReports()
        {
            // Clear old reports
            try
            {
                foreach (string oneFile in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Program.AppExecutable), "tmp????.xlsx"))
                {
                    System.IO.File.Delete(oneFile);
                }
            }
            catch { }
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
            MessageBox.Show("Критическая ошибка. Приложение будет закрыто\r\n\r\n" + ex.Message + "\r\n" + ex.Source + "\r\n" + ex.StackTrace, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter("Tasks-" + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + DateTime.Now.Millisecond.ToString("000") + ".dump", false, System.Text.Encoding.UTF8);
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
            foreach (string oneFile in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Program.AppExecutable), "*.dump"))
                Update.UploadDump(oneFile);
        }
    }
}
