
using System.Threading;

namespace Tasks.Threads
{
    static class Update
    {
        static private Thread mainThread;

        static public void Start()
        {
            mainThread = new Thread(ThreadFunc);
            mainThread.Start();
        }
        static public void Stop()
        {
            try
            {
                mainThread.Abort();
            }
            catch { }
        }

        static private void ThreadFunc()
        {
            while (true)
            {
                try
                {
                    if (Tasks.Update.checkFiles() == false)
                    {
                        Threads.Popups.Add("Обновление", "Загружаются библиотеки для формирования отчетов", PopupType.Info);
                        if (Tasks.Update.downloadFiles() == true)
                            Threads.Popups.Add("Обновление", "Библиотеки загружены. Теперь можно формировать отчеты", PopupType.Info);
                        else
                            Threads.Popups.Add("Обновление", "Не удалось загрузить библиотеки. Формирование отчетов недоступно", PopupType.Info);
                    }

                    Tasks.Update.checkUpgrade();
                    if (Config.CurrentVersion != Config.ServerVersion)
                        Tray.SetStatusUpdate();
                }
                catch { }

                for (int i = 0; i < 7200; i++)
                {
                    if (Program.isExiting == true)
                        return;
                    Thread.Sleep(1000);
                }
            }
        }
    }
}