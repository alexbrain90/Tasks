
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
                    Tasks.Update.Functions.CheckVersion();
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