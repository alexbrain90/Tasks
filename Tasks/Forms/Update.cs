using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    class Update:Form
    {
        Label l_Current, l_Server;
        TextBox tb_Info;
        Button b_Update, b_Cancel;

        public Update()
        {
            this.Text = "Обновление";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(400, 600);
            this.FormBorderStyle = FormBorderStyle.Fixed3D;

            this.Controls.Add(l_Current = new Label());
            l_Current.Location = new Point(10, 10);
            l_Current.Size = new Size(this.ClientSize.Width - 20, 20);
            l_Current.Text = "Текущая версия приложения: " + Application.ProductVersion;
            l_Current.Font = new Font(l_Current.Font.FontFamily, 10);
            this.Controls.Add(l_Server = new Label());
            l_Server.Location = new Point(l_Current.Left, l_Current.Bottom + 5);
            l_Server.Size = l_Current.Size;
            l_Server.Text = "Доступно обновление до версии: " + Config.ServerVersion;
            l_Server.Font = l_Current.Font;

            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.Size = new Size(120, 25);
            b_Cancel.Location = new Point(this.ClientSize.Width - b_Cancel.Width - 10, this.ClientSize.Height - b_Cancel.Height - 10);
            b_Cancel.Text = "Отмена";
            b_Cancel.Click += b_Cancel_Click;
            this.Controls.Add(b_Update = new Button());
            b_Update.Size = b_Cancel.Size;
            b_Update.Location = new Point(b_Cancel.Left - b_Update.Width - 10, b_Cancel.Top);
            b_Update.Text = "Обновить";
            b_Update.Click += b_Update_Click;

            this.Controls.Add(tb_Info = new TextBox());
            tb_Info.Location = new Point(l_Server.Left, l_Server.Bottom + 10);
            tb_Info.Size = new Size(l_Server.Width, b_Update.Top - l_Server.Bottom - 20);
            tb_Info.Text = "История версий:\r\n\r\n" + Config.ServerVersion + "\r\n" + Config.ServerVersionInfo.Replace("\r\nv", "\r\n");
            tb_Info.Multiline = true;
            tb_Info.ReadOnly = true;
            tb_Info.ScrollBars = ScrollBars.Vertical;
            tb_Info.WordWrap = true;

            for (int i = 0; i < tb_Info.Lines.Length; i++)
            {
                if (tb_Info.Lines[i] == Application.ProductVersion)
                {
                    if (i < 1)
                        break;

                    string[] newlines = new string[i - 1];
                    for (int j = 0; j < i - 1; j++)
                        newlines[j] = tb_Info.Lines[j];
                    tb_Info.Lines = newlines;
                    break;
                }
            }
        }

        private void b_Update_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Приложение будет перезапущено автоматически после обновления. Подождите", "Обновление", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }
        private void b_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}