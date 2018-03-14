using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Tasks.Forms.Messages
{
    class ReAuth:Form
    {
        private Thread thread;
        Label l_Info;

        public ReAuth()
        {
            thread = new Thread(ThreadFunc);

            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(400, 60);
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Нет связи с сервером";

            this.Controls.Add(l_Info = new Label());
            l_Info.Location = new Point(10, 10);
            l_Info.Size = new Size(380, 20);

            ProgressBar pb = new ProgressBar();
            this.Controls.Add(pb);
            pb.Location = new Point(10, 35);
            pb.Size = new Size(380, 20);
            pb.Style = ProgressBarStyle.Marquee;

            this.DialogResult = DialogResult.Cancel;

            this.Shown += ReAuth_Shown;
            this.FormClosing += ReAuth_FormClosing;
        }

        private void ReAuth_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                thread.Abort();
            }
            catch { };
        }

        private void ReAuth_Shown(object sender, EventArgs e)
        {
            thread.Start();
        }
        private void ThreadFunc()
        {
            int n = 0;
            while(true)
            {
                n++;
                l_Info.Text = "Попытка повторного соединения: " + n.ToString();
                int r = Network.User_Auth(Program.user_NameMain, Program.user_Pass);

                if (r == 1)
                    this.DialogResult = DialogResult.OK;
                else if (r == 2)
                {
                    DialogResult dialog = new ChangePassword(Program.user_NameMain).ShowDialog();
                    if (dialog == DialogResult.OK)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                }
                else if (r == 3)
                {
                    MessageBox.Show("Для дальнейшей работы необходимо выполнить обовление приложения до последней версии", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Tasks.Update.checkUpgrade();
                    if (new Tasks.Forms.Update().ShowDialog() == DialogResult.Yes)
                        if (Tasks.Update.makeUpgrade(new string[0]) == true)
                            Application.Exit();
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }
                else
                {
                    Thread.Sleep(10000);
                    continue;
                }

                this.Close();
                return;
            }
        }
    }
}
