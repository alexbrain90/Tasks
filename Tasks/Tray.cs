using System;
using System.Windows.Forms;

namespace Tasks
{
    static class Tray
    {
        static private NotifyIcon tray_Icon = new NotifyIcon();
        static private ContextMenu cm_Tray = new ContextMenu();
        static private System.Threading.Thread t_Anim = new System.Threading.Thread(AnimFunc);
        static private bool StatusNormal = false, StatusMiss = false, StatusNew = false, StatusError = true;
        static private int StatusMissCount = 0;
        static private Forms.Main f_Main;

        static public void InitTray()
        {
            cm_Tray.MenuItems.Add("Открыть главное окно", tray_Open);
            cm_Tray.MenuItems.Add("-"); cm_Tray.MenuItems[1].Visible = false;
            cm_Tray.MenuItems.Add("Обновить программу", tray_Update); cm_Tray.MenuItems[2].Visible = false;
            cm_Tray.MenuItems.Add("-");
            cm_Tray.MenuItems.Add("Сменить пользователя", tray_Login);
            cm_Tray.MenuItems.Add("Закрыть программу", tray_Exit);

            tray_Icon.Icon = Properties.Resources.Tray_Error;
            tray_Icon.ContextMenu = cm_Tray;
            tray_Icon.MouseClick += tray_Click;
            tray_Icon.Visible = true;
            tray_Icon.Text = "Рабочие планы";

            t_Anim.Start();
        }

        static public void ExitTray()
        {
            t_Anim.Abort();
            tray_Icon.Visible = false;
        }

        static public void ShowMainForm()
        {
            tray_Open(new object(), new EventArgs());
        }
        private static void tray_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                tray_Open(sender, e);
        }
        private static void tray_Open(object sender, EventArgs e)
        {
            if (Threads.Connection.Connected == false)
                return;

            if (f_Main == null || f_Main.IsDisposed == true)
                f_Main = new Forms.Main();

            if (f_Main.Visible == false)
                f_Main.Show();
            if (f_Main.WindowState == FormWindowState.Minimized)
            {
                if (Config.form_Main_Maximized)
                    f_Main.WindowState = FormWindowState.Maximized;
                else
                    f_Main.WindowState = FormWindowState.Normal;
            }

            UnSetStatusNew();
        }
        private static void tray_Update(object sender, EventArgs e)
        {
            DialogResult dr = new Forms.Update().ShowDialog();
            if (dr == DialogResult.Yes)
                if (Tasks.Update.makeUpgrade(new string[0]) == true)
                    Application.Exit();
        }
        private static void tray_Login(object sender, EventArgs e)
        {
            Threads.Connection.Stop();
            Threads.CheckNews.Stop();
            Network.User_Exit();
            if (f_Main != null && f_Main.IsDisposed != true)
                f_Main.Close();

            Threads.Connection.Login();

            Threads.Connection.Start();
            Threads.CheckNews.Start();
        }
        private static void tray_Exit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static void SetStatusNormal()
        {
            if (StatusNormal == false && StatusError == true)
                tray_Icon.ShowBalloonTip(5000, "Подключение", "Связь с сервером успешно восстановлена", ToolTipIcon.Info);

            StatusNormal = true;
            StatusError = false;
        }
        public static void SetStatusError()
        {
            if (StatusNormal == true && StatusError == false)
                tray_Icon.ShowBalloonTip(5000, "Подключение", "Потеряна связь с сервером", ToolTipIcon.Error);

            StatusNormal = false;
            StatusError = true;
        }
        public static void SetStatusNew(string Text)
        {
            tray_Icon.ShowBalloonTip(15000, "Новое событие", Text, ToolTipIcon.None);
            StatusNew = true;
        }
        public static void UnSetStatusNew()
        {
            StatusNew = false;
        }
        public static void SetStatusMiss(int Count, string Text)
        {
            if (Count != 0)
            {
                if (StatusMissCount != Count)
                    tray_Icon.ShowBalloonTip(15000, "Состояние", "Есть задачи (" + Count.ToString() + "), которые требуют вашего внимания:\r\n" + Text, ToolTipIcon.Info);
                StatusMiss = true;
            }
            else
            {
                StatusMiss = false;
            }
            StatusMissCount = Count;
        }
        public static void SetStatusUpdate()
        {
            cm_Tray.MenuItems[1].Visible = true;
            cm_Tray.MenuItems[2].Visible = true;
            tray_Icon.ShowBalloonTip(10000, "Обновление", "Доступно обновление до версии: " + Config.ServerVersion, ToolTipIcon.None);
        }
        public static void ShowBaloon(string Caption, string Text)
        {
            tray_Icon.ShowBalloonTip(10000, Caption, Text, ToolTipIcon.None);
        }

        private static void AnimFunc()
        {
            System.Drawing.Icon icon_Error = Properties.Resources.Tray_Error;
            System.Drawing.Icon icon_New1 = Properties.Resources.Tray_New1;
            System.Drawing.Icon icon_New5 = Properties.Resources.Tray_New5;
            System.Drawing.Icon icon_Normal = Properties.Resources.Tray_Normal;
            System.Drawing.Icon icon_Miss = Properties.Resources.Tray_Miss;

            bool tick = false;
            while (true)
            {
                try
                {
                    if (StatusError == true)
                        tray_Icon.Icon = icon_Error;
                    else if (StatusNormal == true)
                    {
                        if (StatusNew == true)
                        {
                            if (tick == true)
                                tray_Icon.Icon = icon_New1;
                            else
                                tray_Icon.Icon = icon_New5;
                            tick = !tick;
                        }
                        else if (StatusMiss == true)
                            tray_Icon.Icon = icon_Miss;
                        else
                            tray_Icon.Icon = icon_Normal;
                    }
                }
                catch { }
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}