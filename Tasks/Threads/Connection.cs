using System;
using System.Threading;
using System.Windows.Forms;

namespace Tasks.Threads
{
    static class Connection
    {
        static private Thread mainThread;
        static public bool Connected = false;

        static public void Start()
        {
            mainThread = new Thread(ThreadFunc);
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
        }

        static private void ThreadFunc()
        {
            Connected = false;

            while (true)
            {
                try
                {
                    Authentification();
                }
                catch { }
                Thread.Sleep(5000);
            }
        }
        static private void Authentification()
        {
            if (Config.UserConID == -1)
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
                        Connected = true;
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
    }
}