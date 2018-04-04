using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    class Popup : Form
    {
        private Label l_Caption, l_Text;
        private Button b_Button1, b_Button2;
        private Timer timer;

        private PopupInfo info;

        public Popup(PopupInfo Info)
        {
            info = Info;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Opacity = 0;
            this.StartPosition = FormStartPosition.Manual;
            this.ClientSize = new Size(300, 1);
            this.Font = Config.font_Main;
            this.Shown += Popup_Shown;
            this.FormClosing += Popup_FormClosing;
            this.Click += Popup_Click;

            this.Controls.Add(l_Caption = new Label());
            l_Caption.Font = Config.font_Main;
            l_Caption.Text = info.Caption + " - Рабочие планы";
            l_Caption.ForeColor = Color.White;
            l_Caption.TextAlign = ContentAlignment.MiddleLeft;
            l_Caption.Click += Popup_Click;

            this.Controls.Add(l_Text = new Label());
            l_Text.Font = Config.font_MainSmall;
            l_Text.Text = info.Text;
            l_Text.Click += Popup_Click;

            this.Controls.Add(b_Button1 = new Button());
            this.Controls.Add(b_Button2 = new Button());
            b_Button1.Click += b_Button1_Click;
            b_Button2.Click += b_Button2_Click;
            b_Button1.Visible = false; b_Button2.Visible = false;
            b_Button1.Size = new Size(32, 32); b_Button2.Size = b_Button1.Size;

            timer = new Timer();
            timer.Tick += Timer_Tick;

            switch (info.Type)
            {
                case PopupType.Info:
                    this.BackColor = Color.LightCyan;
                    timer.Interval = 10000;
                    break;

                case PopupType.MissTask:
                    this.BackColor = Color.LightSalmon;
                    timer.Interval = 60000;
                    break;
                case PopupType.NearTask:
                    this.BackColor = Color.LightGoldenrodYellow;
                    timer.Interval = 20000;
                    break;
                case PopupType.NewEvent:
                    this.BackColor = Color.LightGreen;
                    timer.Interval = 10000;
                    break;

                case PopupType.Update:
                    this.BackColor = Color.LightSkyBlue;
                    b_Button1.Image = Properties.Resources.pb_OK;
                    b_Button2.Image = Properties.Resources.pb_Late;
                    b_Button1.Visible = true; b_Button2.Visible = true;
                    timer.Interval = 60000;
                    break;

                case PopupType.ServerError:
                    this.BackColor = Color.IndianRed;
                    timer.Interval = 5000;
                    break;
                case PopupType.ServerNormal:
                    this.BackColor = Color.LightGray;
                    timer.Interval = 10000;
                    break;
            }
            l_Caption.BackColor = Color.FromArgb(this.BackColor.R / 2, this.BackColor.G / 2, this.BackColor.B / 2);

            l_Caption.Left = 6; l_Caption.Top = 6;
            l_Caption.Width = this.Width - 12; Label_AutoSize(l_Caption, null);
            l_Text.Left = l_Caption.Left; l_Text.Top = l_Caption.Bottom + 2;
            l_Text.Width = l_Caption.Width; Label_AutoSize(l_Text, null);
            this.Height = l_Text.Bottom + 6;
            if (info.Type == PopupType.Update)
            {
                b_Button1.Left = l_Text.Left + 6; b_Button1.Top = l_Text.Bottom + 10;
                b_Button2.Left = b_Button1.Right + 10; b_Button2.Top = b_Button1.Top;

                this.Height = b_Button2.Bottom + 6;
            }

            if (this.Height < 80)
                this.Height = 80;

            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - this.Width - 10, Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - 10);

            timer.Enabled = true;
        }

        private void Popup_Click(object sender, EventArgs e)
        {
            //Tray.ShowMainForm();
            this.Close();
        }

        private void b_Button1_Click(object sender, EventArgs e)
        {
         if (info.Type == PopupType.Update)
         {
            DialogResult dr = new Tasks.Update.Changelog().ShowDialog();
            if (dr == DialogResult.Yes)
            {
               new Tasks.Update.Progress(0).ShowDialog();
               Application.Exit();
            }
         }
        }
        private void b_Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Popup_Shown(object sender, EventArgs e)
        {
            float tmp = 0;
            while(tmp < 1)
            {
                tmp += 0.01f;
                if (tmp > 1)
                    tmp = 1;
                this.Opacity = tmp;
                Application.DoEvents();
                System.Threading.Thread.Sleep(5);
            }
        }
        private void Popup_FormClosing(object sender, FormClosingEventArgs e)
        {
            float tmp = 1;
            while (tmp > 0)
            {
                tmp -= 0.01f;
                if (tmp < 0)
                    tmp = 0;
                this.Opacity = tmp;
                Application.DoEvents();
                System.Threading.Thread.Sleep(1);
            }
        }

        private void Label_AutoSize(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            Graphics g = this.CreateGraphics();
            label.Height = (int)g.MeasureString(label.Text, label.Font, label.Width).Height + 4;
        }
    }
}