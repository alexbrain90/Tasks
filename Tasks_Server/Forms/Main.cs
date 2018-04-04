using System;
using System.Windows.Forms;

namespace Tasks_Server.Forms
{
    class Main : Form
    {
        TextBox tb_Log = new TextBox();
        Label l_Info = new Label();
        ListBox lb_Con = new ListBox();
        Timer t_Con = new Timer(), t_Info = new Timer();

        private double BytesR = 0, BytesS = 0, Requests = 0;
        private long TBytesR = 0, TBytesS = 0, TRequests = 0;

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

            this.Controls.Add(l_Info = new Label());
            l_Info.Text = "";
            t_Info.Tick += T_Info_Tick;
            t_Info.Interval = 1000;
            t_Info.Enabled = true;

            Program.log.LogEvent += Log_LogEvent;
            this.Resize += Main_Resize;

            Main_Resize(this, new EventArgs());
        }

        private void T_Info_Tick(object sender, EventArgs e)
        {
            Requests = CalcValue(Network.Requests, Requests, 1); TRequests += Network.Requests; Network.Requests = 0;
             BytesR = CalcValue(Network.BytesR, BytesR, 0); TBytesR += Network.BytesR; Network.BytesR = 0;
            BytesS = CalcValue(Network.BytesS, BytesS, 0); TBytesS += Network.BytesS; Network.BytesS = 0;

            l_Info.Text = "Запросы: " + Requests.ToString() + "/с (" + TRequests.ToString() + ")\r\n" +
                          "Получено: " + SizeToString((long)BytesR) + "/с (" + SizeToString(TBytesR) + ")\r\n" +
                          "Отправлено: " + SizeToString((long)BytesS) + "/с (" + SizeToString(TBytesS) + ")";
        }
        private double CalcValue(double oldD, double newD, int d)
        {
            return Math.Round(oldD * 0.5 + newD * 0.5, d);
        }
        private string SizeToString(long size)
        {
            string[] add = new string[] { "Б", "КБ", "МБ", "ГБ" };
            int n = 0;
            double s = (double)size;
            while (s >= 1000)
            {
                s /= 1024;
                n++;
            }

            string r = "";
            if (n == 0)
                r = size.ToString();
            else
            {
                if (s < 10)
                    r = s.ToString("0.00");
                else if (s < 100)
                    r = s.ToString("00.0");
                else
                    r = s.ToString("000");
            }

            r += " " + add[n];

            return r;
        }

        private void t_Con_Tick(object sender, EventArgs e)
        {
            lb_Con.BeginUpdate();
            lb_Con.Items.Clear();
            DateTime dt;
            for(int i = 0; i < Program.net.c_List.Count; i++)
            {
                dt = new DateTime(Program.net.c_List[i].LastConnect);
                lb_Con.Items.Add(dt.Day.ToString("00") + "-" + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + "  " + Program.net.c_List[i].UserName);
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
            tb_Log.Size = new System.Drawing.Size(this.ClientSize.Width - 350, this.ClientSize.Height - 20);

            lb_Con.Location = new System.Drawing.Point(tb_Log.Right + 10, tb_Log.Top);
            lb_Con.Size = new System.Drawing.Size(320, tb_Log.Height / 3 * 2);

            l_Info.Location = new System.Drawing.Point(lb_Con.Left, lb_Con.Bottom + 10);
            l_Info.Size = new System.Drawing.Size(lb_Con.Width, this.ClientSize.Height - l_Info.Top - 10);
        }

        private void Log_LogEvent(LogEventArgs e)
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