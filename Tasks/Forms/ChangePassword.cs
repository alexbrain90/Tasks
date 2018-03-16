using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    class ChangePassword : Form
    {
        private TextBox tb_OldPass, tb_NewPass, tb_NewPass2;
        private Label l_Info, l_OldPass, l_NewPass, l_NewPass2;
        private Button b_Ok, b_Cancel;

        public ChangePassword(string userName)
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = Tasks.Properties.Resources.icon_Main;
            this.Text = "Смена пароля";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ClientSize = new Size(270, 270);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(l_Info = new Label());
            l_Info.Location = new Point(10, 10);
            l_Info.Size = new Size(this.ClientSize.Width - 20, 20);
            l_Info.TextAlign = ContentAlignment.MiddleLeft;
            l_Info.Text = userName;

            this.Controls.Add(l_OldPass = new Label());
            l_OldPass.Location = new Point(l_Info.Left, l_Info.Bottom + 10);
            l_OldPass.Size = new Size(l_Info.Width, 14);
            l_OldPass.TextAlign = ContentAlignment.BottomLeft;
            l_OldPass.Text = "Старый пароль";
            this.Controls.Add(tb_OldPass = new TextBox());
            tb_OldPass.Location = new Point(l_OldPass.Left, l_OldPass.Bottom + 2);
            tb_OldPass.Size = new Size(l_OldPass.Width, 20);
            tb_OldPass.UseSystemPasswordChar = true;

            this.Controls.Add(l_NewPass = new Label());
            l_NewPass.Location = new Point(tb_OldPass.Left, tb_OldPass.Bottom + 6);
            l_NewPass.Size = l_Info.Size;
            l_NewPass.TextAlign = ContentAlignment.BottomLeft;
            l_NewPass.Text = "Новый пароль";
            this.Controls.Add(tb_NewPass = new TextBox());
            tb_NewPass.Location = new Point(l_NewPass.Left, l_NewPass.Bottom + 2);
            tb_NewPass.Size = new Size(l_NewPass.Width, 20);
            tb_NewPass.UseSystemPasswordChar = true;

            this.Controls.Add(l_NewPass2 = new Label());
            l_NewPass2.Location = new Point(tb_NewPass.Left, tb_NewPass.Bottom);
            l_NewPass2.Size = l_Info.Size;
            l_NewPass2.TextAlign = ContentAlignment.BottomLeft;
            l_NewPass2.Text = "Новый пароль еще раз";
            this.Controls.Add(tb_NewPass2 = new TextBox());
            tb_NewPass2.Location = new Point(l_NewPass2.Left, l_NewPass2.Bottom + 2);
            tb_NewPass2.Size = new Size(l_NewPass2.Width, 20);
            tb_NewPass2.UseSystemPasswordChar = true;

            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.Location = new Point(this.ClientSize.Width / 2 + 5, tb_NewPass2.Bottom + 10);
            b_Cancel.Size = new Size((this.ClientSize.Width - 30) / 2, 25);
            b_Cancel.Text = Language.Login_Cancel;
            b_Cancel.Click += B_Cancel_Click;
            this.Controls.Add(b_Ok = new Button());
            b_Ok.Location = new Point(10, b_Cancel.Top);
            b_Ok.Size = b_Cancel.Size;
            b_Ok.Text = Language.Login_OK;
            b_Ok.Click += B_Ok_Click;

            this.ClientSize = new Size(this.ClientSize.Width, b_Ok.Bottom + 10);
        }

        private void B_Ok_Click(object sender, EventArgs e)
        {
            if (tb_NewPass.Text != tb_NewPass2.Text)
            {
                MessageBox.Show("Новый пароль не совпадает с проверочным", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tb_NewPass.Text = "";
                tb_NewPass2.Text = "";
                return;
            }

            if (tb_NewPass.Text == "")
            {
                MessageBox.Show("Новый пароль не может быть пустым", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tb_NewPass.Text = "";
                tb_NewPass2.Text = "";
                return;
            }

            if (Network.User_Password(tb_OldPass.Text, tb_NewPass.Text) == true)
            {
                MessageBox.Show("Пароль был успешно изменен", "Изменение пароля", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Не удалось изменить пароль. Проверьте правильность ввода данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tb_OldPass.Text = "";
                tb_NewPass.Text = "";
                tb_NewPass2.Text = "";
                return;
            }
        }

        private void B_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}