using System.Threading;
using System.Collections.Generic;

namespace Tasks.Threads
{
    static class Popups
    {
        static private Thread mainThread;
        static private Queue<PopupInfo> popups = new Queue<PopupInfo>();

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

        static public void Add(PopupInfo popup)
        {
            popups.Enqueue(popup);
        }
        static public void Add(string Caption, string Text, PopupType Type)
        {
            PopupInfo popup = new PopupInfo();
            popup.Caption = Caption;
            popup.Text = Text;
            popup.Type = Type;
            popup.Event = EventType.None;
            Add(popup);
        }
        static public void Add(string Caption, string Text, PopupType Type, EventType Event)
        {
            PopupInfo popup = new PopupInfo();
            popup.Caption = Caption;
            popup.Text = Text;
            popup.Type = Type;
            popup.Event = Event;
            Add(popup);
        }

        static private void ThreadFunc()
        {
            while(true)
            {
                if (Program.isExiting == true)
                    return;
                try
                {
                    if (popups.Count == 0 )
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    new Forms.Popup(popups.Dequeue()).ShowDialog();
                    Thread.Sleep(1000);
                }
                catch { }
            }
        }
    }
}