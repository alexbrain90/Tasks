using System;
using System.Windows.Forms;

namespace Tasks
{
    static class Tray
    {
        static private NotifyIcon tray_Icon = new NotifyIcon();
        static private ContextMenu cm_Tray = new ContextMenu();
        static private int Status = 1; static private System.Threading.Thread t_Anim = new System.Threading.Thread(AnimFunc);
        static private int Status_Miss = 0, Status_New = 0;
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

            t_Anim.Start();
        }

        static public void ExitTray()
        {
            t_Anim.Abort();
            tray_Icon.Visible = false;
        }

        private static void tray_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                tray_Open(sender, e);
        }
        private static void tray_Open(object sender, EventArgs e)
        {
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
            Threads.CheckNews.Stop();
            Network.User_Exit();
            f_Main.Close();

            Threads.CheckNews.Login();
            Threads.CheckNews.Start();
        }
        private static void tray_Exit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static void SetStatusNormal()
        {
            if (Status == -1)
                tray_Icon.ShowBalloonTip(5000, "Подключение", "Связь с сервером успешно восстановлена", ToolTipIcon.Info);

            if (Status_New != 0)
                Status = 0;
            else if (Status_Miss != 0)
                Status = 2;
            else
                Status = 1;
        }
        public static void SetStatusError()
        {
            if (Status != -1)
                tray_Icon.ShowBalloonTip(5000, "Подключение", "Потеряна связь с сервером", ToolTipIcon.Error);
            Status = -1;
        }
        public static void SetStatusNew(string Text)
        {
            Status_New++;
            tray_Icon.ShowBalloonTip(15000, "Новое событие", Text, ToolTipIcon.None);
            Status = 0;
        }
        public static void SetStatusNewMinus()
        {
            if (Status_New > 0)
                Status_New--;

            SetStatusNormal();
        }
        public static void SetStatusMiss(int Count, string Text)
        {
            if (Status_Miss != Count)
                tray_Icon.ShowBalloonTip(15000, "Состояние", "Есть задачи (" + Count.ToString() + "), которые требуют вашего внимания:\r\n" + Text, ToolTipIcon.Info);
            Status_Miss = Count;

            if (Status_New != 0)
                Status = 0;
            else
                Status = 2;
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
            bool tick = false;
            while(true)
            {
                try
                {
                    switch (Status)
                    {
                        case -1:
                            tray_Icon.Icon = Properties.Resources.Tray_Error;
                            break;
                        case 0:
                            if (tick == true)
                                tray_Icon.Icon = Properties.Resources.Tray_New1;
                            else
                                tray_Icon.Icon = Properties.Resources.Tray_New5;
                            tick = !tick;
                            break;
                        case 1:
                            tray_Icon.Icon = Properties.Resources.Tray_Normal;
                            break;
                        case 2:
                            tray_Icon.Icon = Properties.Resources.Tray_Miss;
                            break;
                    }
                }
                catch { }
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}