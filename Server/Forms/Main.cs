using System;
using System.Windows.Forms;

namespace Tasks_Server.Forms
{
    class Main : Form
    {
        TextBox tb_Log = new TextBox();
        ListBox lb_Con = new ListBox();
        Timer t_Con = new Timer();

        public Main()
        {
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Text = "Задачи. Серверная часть - " + Program.CurrentVersion;

            this.Controls.Add(tb_Log = new TextBox());
            tb_Log.Multiline = true;
            tb_Log.ScrollBars = ScrollBars.Vertical;
            tb_Log.ReadOnly = true;
            tb_Log.TextChanged += Tb_Log_TextChanged;

            this.Controls.Add(lb_Con = new ListBox());
            t_Con.Tick += t_Con_Tick;
            t_Con.Interval = 10000;
            t_Con.Enabled = true;

            Program.log.LogEvent += Log_LogEvent;
            this.Resize += Main_Resize;

            Main_Resize(this, new EventArgs());
        }

        private void t_Con_Tick(object sender, EventArgs e)
        {
            lb_Con.BeginUpdate();
            lb_Con.Items.Clear();
            DateTime dt;
            for(int i = 0; i < Program.net.c_List.Count; i++)
            {
                dt = new DateTime(Program.net.c_List[i].LastConnect);
                lb_Con.Items.Add(dt.Year.ToString() + "." + dt.Month.ToString("00") + "." + dt.Day.ToString("00") + " - " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00") + "\t" + Program.net.c_List[i].UserId.ToString());
            }
            lb_Con.EndUpdate();

            for (int i = 0; i < Program.net.c_List.Count; i++)
            {
                if (DateTime.Now.Ticks - Program.net.c_List[i].LastConnect > 36000000000)
                {
                    Program.net.Auth_Remove(Program.net.c_List[i].Id);
                    i--;
                }
            }
        }

        private void Tb_Log_TextChanged(object sender, EventArgs e)
        {
            if (tb_Log.Text.Length > 100000)
                tb_Log.Text = tb_Log.Text.Substring(0, 100000);
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            tb_Log.Location = new System.Drawing.Point(10, 10);
            tb_Log.Size = new System.Drawing.Size(this.ClientSize.Width - 250, this.ClientSize.Height - 20);

            lb_Con.Location = new System.Drawing.Point(tb_Log.Right + 10, tb_Log.Top);
            lb_Con.Size = new System.Drawing.Size(220, tb_Log.Height);
        }

        private void Log_LogEvent(LogEventArgs e)
        {
            if (e.error == true || e.text.StartsWith("Успешная авторизация"))
            {

                string tmp = e.dateTime.Year.ToString("0000") + "." + e.dateTime.Month.ToString("00") + "." + e.dateTime.Day.ToString("00") + " - " + e.dateTime.Hour.ToString("00") + ":" + e.dateTime.Minute.ToString("00") + ":" + e.dateTime.Second.ToString("00") + "." + e.dateTime.Millisecond.ToString("000");
                if (e.error == true)
                    tmp += " ###\t";
                else
                    tmp += "\t";
                tmp += e.text;

                tb_Log.Text = tmp + "\r\n" + tb_Log.Text;
            }
        }
    }
}