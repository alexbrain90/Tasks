using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms

{
    class Login : Form
    {
        public int UserID = -1;

        private TextBox tb_User, tb_Pass;
        private Label l_User, l_Pass;
        private Button b_Ok, b_Cancel;

        public Login()
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = Tasks.Properties.Resources.icon_Main;

            this.Text = Tasks.Language.Login_Caption;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ClientSize = new Size(270, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Shown += Login_Shown;

            this.Controls.Add(l_User = new Label());
            l_User.Location = new Point(20, 0);
            l_User.Size = new Size(this.ClientSize.Width - 40, 20);
            l_User.Text = Language.Login_UserName;
            l_User.TextAlign = ContentAlignment.BottomLeft;
            this.Controls.Add(tb_User = new TextBox());
            tb_User.Location = new Point(10, l_User.Bottom);
            tb_User.Size = new Size(this.ClientSize.Width - 20, 20);
            tb_User.TextChanged += Tb_User_TextChanged;

            this.Controls.Add(l_Pass = new Label());
            l_Pass.Location = new Point(l_User.Left, tb_User.Bottom);
            l_Pass.Size = l_User.Size;
            l_Pass.Text = Language.Login_Password;
            l_Pass.TextAlign = ContentAlignment.BottomLeft;
            this.Controls.Add(tb_Pass = new TextBox());
            tb_Pass.Location = new Point(tb_User.Left, l_Pass.Bottom);
            tb_Pass.Size = new Size(tb_User.Width, tb_User.Height);
            tb_Pass.UseSystemPasswordChar = true;
            tb_Pass.PreviewKeyDown += Tb_Pass_PreviewKeyDown;

            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.Location = new Point(this.ClientSize.Width / 2 + 5, tb_Pass.Bottom + 10);
            b_Cancel.Size = new Size((this.ClientSize.Width - 30) / 2, 25);
            b_Cancel.Text = Language.Login_Cancel;
            b_Cancel.Click += b_Cancel_Click;
            this.Controls.Add(b_Ok = new Button());
            b_Ok.Location = new Point(10, b_Cancel.Top);
            b_Ok.Size = b_Cancel.Size;
            b_Ok.Text = Language.Login_OK;
            b_Ok.Click += b_Ok_Click;


            this.ClientSize = new Size(this.ClientSize.Width, b_Cancel.Bottom + 10);


            string tmp = Program.ConfigFile.Read("Global", "LastUserName");
            if (tmp != "" && tmp != null)
                tb_User.Text = tmp;
        }

        private void Tb_User_TextChanged(object sender, EventArgs e)
        {
            int tmp = tb_User.SelectionStart;

            if (tb_User.Text.Length > 0)
            {
                tb_User.Text = tb_User.Text.Substring(0, 1).ToUpper() + tb_User.Text.Substring(1);
            }

            int n = 0;
            while ((n = tb_User.Text.IndexOf(" ", n)) != -1)
            {
                n++;

                if (n > tb_User.Text.Length - 1)
                    break;

                tb_User.Text = tb_User.Text.Substring(0, n) + tb_User.Text.Substring(n, 1).ToUpper() + tb_User.Text.Substring(n + 1);
            }

            tb_User.SelectionStart = tmp;
        }

        private void Tb_Pass_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                b_Ok_Click(sender, e);
        }

        private void Login_Shown(object sender, EventArgs e)
        {
            if (tb_User.Text != "")
                tb_Pass.Focus();
        }

        private void b_Ok_Click(object sender, EventArgs e)
        {
            switch(Network.User_Auth(tb_User.Text, tb_Pass.Text))
            {
                case 1:
                    Program.user_Name = tb_User.Text;
                    Program.user_NameMain = Program.user_Name;
                    Program.user_Pass = tb_Pass.Text;

                    Program.ConfigFile.Write("Global", "LastUserName", tb_User.Text);
                    Program.ConfigFile.Save();
        
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
                case 2:
                    DialogResult dialog = new ChangePassword(tb_User.Text).ShowDialog();
                    tb_Pass.Text = "";
                    if (dialog == DialogResult.OK)
                    {
                        Program.user_Name = tb_User.Text;
                        Program.user_NameMain = Program.user_Name;

                        Program.ConfigFile.Write("Global", "LastUserName", tb_User.Text);
                        Program.ConfigFile.Save();

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    break;
                case 3:
                    Program.ConfigFile.Write("Global", "LastUserName", tb_User.Text);
                    Program.ConfigFile.Save();

                    MessageBox.Show("Для дальнейшей работы необходимо выполнить обовление приложения до последней версии", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Tasks.Update.checkUpgrade();
                    if (new Tasks.Forms.Update().ShowDialog() == DialogResult.Yes)
                        if (Tasks.Update.makeUpgrade(new string[0]) == true)
                            Application.Exit();
                    break;
                case 0:
                    MessageBox.Show("Имя и/или пароль не распознаны", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case -1:
                    MessageBox.Show("Произошла внутренняя ошибка сервера", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }


            /*
            // Get
            object[] data = SQL.getData("SELECT ID, Password FROM Users WHERE Name = \'" + tb_User.Text + "\'");
            if (data == null)
            {
                MessageBox.Show(Language.Error_ServerConnection, Language.Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (data.Length == 0)
            {
                MessageBox.Show(Language.Error_UserNameNotFound, Language.Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            data = (object[])data[0];

            UserID = (Int32)data[0];

            Program.ConfigFile.Write("Global", "LastUserName", tb_User.Text);
            if (Program.ConfigFile.Save() == false)
                MessageBox.Show(Language.Error_SaveSettings, Language.Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

            this.DialogResult = DialogResult.OK;
            this.Close();
            */
        }
        private void b_Cancel_Click(object sender, EventArgs e)
        {
            this.UserID = -1;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
