using System;
using System.Threading;
using System.Windows.Forms;

namespace Tasks.Threads
{
    static class CheckNews
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
            Thread.Sleep(15000);

            while(true)
            {
                if (Program.isExiting == true)
                    return;

                if (Connection.Connected == true && Config.user_ID == Config.user_IDMain)
                {
                    try
                    {
                        getMissTasks();
                        getNewEvents();
                    }
                    catch { }
                }
                Thread.Sleep(30000);
            }
        }
        static private void getMissTasks()
        {
            int n = 0;
            string text = "";
            string[,] list = Network.Task_List(1, 0, DateTime.Now.Ticks, 3);
            for(int i =0; i < list.Length / 10; i++)
            {
                if (Convert.ToInt64(list[i, 4]) < DateTime.Now.Ticks)
                {
                    n++;
                    text += n.ToString() + ". " + list[i, 0] + "\r\n";
                }
            }
            if (text.Length > 2)
                text = text.Substring(0, text.Length - 2);

            if (n != 0)
            {
                Tray.SetStatusMiss(n, text);
                Thread.Sleep(15000);
            }
            else
                Tray.SetStatusNormal();
        }
        static private void getNewEvents()
        {
            string[,] list = Network.Event_List();

            if (list.Length == 0)
                Tray.UnSetStatusNew();

            for (int i = 0; i < list.Length / 5; i++)
            {
                Tray.SetStatusNew("Изменения в задаче: " + list[i, 0]);
                Thread.Sleep(15000);
            }
        }
    }
}