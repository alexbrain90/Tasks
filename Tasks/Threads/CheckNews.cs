using System;
using System.Threading;
using System.Windows.Forms;

namespace Tasks.Threads
{
    static class CheckNews
    {
        static private Thread mainThread;
        static private Thread connectThread;
        static private bool Connected = false;

        static public void Start()
        {
            mainThread = new Thread(ThreadFunc);
            connectThread = new Thread(ConnectFunc);
            connectThread.Start();
            mainThread.Start();
        }
        static public void Stop()
        {
            while (true)
            {
                try
                {
                    mainThread.Abort();
                    break;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }

            while(true)
            {
                try
                {
                    connectThread.Abort();
                    break;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        static private void ConnectFunc()
        {
            while (true)
            {
                try
                {
                    Authentification();
                }
                catch { }
                Thread.Sleep(15000);
            }
        }
        static private void Authentification()
        {
            if (Config.user_ConID == -1)
            {
                Connected = false;
                int r = -1;
                if (Config.user_NameMain != "" && Config.user_Pass != "" && Config.login_Auto == true)
                {
                    r = Network.User_Auth(Config.user_NameMain, Config.user_Pass);

                    if (r == -1)
                    {
                        Tray.SetStatusError();
                        Thread.Sleep(5000);
                    }
                    else if (r == 0)
                    {
                        if (Login() == false)
                            return;
                    }
                    else if (r == 1)
                    {
                        Tray.SetStatusNormal();
                    }
                    else if (r == 2)
                    {
                        if (ChangePassword() == false)
                            return;
                    }
                    else if (r == 3)
                    {
                        DialogResult dr = new Forms.Update().ShowDialog();
                        if (dr == DialogResult.Yes)
                            if (Tasks.Update.makeUpgrade(new string[0]) == true)
                                Application.Exit();
                    }
                }
                else
                {
                    if (Login() == false)
                        return;
                }
            }
            else
                Connected = true;
        }
        static public bool Login()
        {
            Forms.Login login = new Forms.Login();
            DialogResult dr = login.ShowDialog();
            if (dr == DialogResult.Cancel)
            {
                Application.Exit();
                return false;
            }

            Config.user_NameMain = login.UserName;
            Config.user_Pass = login.Password;

            Config.WriteConfig();

            return true;
        }
        static private bool ChangePassword()
        {
            DialogResult dialog = new Forms.ChangePassword(Config.user_Name).ShowDialog();
            if (dialog == DialogResult.OK)
            {
                Config.user_NameMain = Config.user_Name;

                Config.ConfigFile.Write("Global", "LastUserName", Config.user_Name);
                Config.ConfigFile.Save();

                return true;
            }
            return false;
        }

        static private void ThreadFunc()
        {
            Thread.Sleep(15000);

            while(true)
            {
                if (Connected == true)
                {
                    try
                    {
                        getMissTasks();
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
            for(int i =0; i < list.Length / 6; i++)
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
                Tray.SetStatusMiss(n, text);
            else
                Tray.SetStatusNormal();
        }
    }
}