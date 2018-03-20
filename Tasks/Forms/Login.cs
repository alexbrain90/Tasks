using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms

{
    class Login : Form
    {
        public int UserID = -1;
        public string UserName, Password;

        private PictureBox pb_User;
        private TextBox tb_User, tb_Pass;
        private Label l_User, l_Pass;
        private CheckBox cb_AutoLogin;
        private Button b_Ok, b_Cancel;

        public Login()
        {
            this.Font = Config.fort_Main;
            this.ShowInTaskbar = true;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = Tasks.Properties.Resources.icon_Main;

            this.Text = Tasks.Language.Login_Caption;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ClientSize = new Size(440, 160);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Shown += Login_Shown;

            this.Controls.Add(pb_User = new PictureBox());
            pb_User.Location = new Point(10, 10);
            pb_User.Size = new Size(128, 128);
            pb_User.Image = Properties.Resources.Login_User;

            this.Controls.Add(l_User = new Label());
            l_User.Location = new Point(pb_User.Right + 20, 10);
            l_User.AutoSize = true;
            l_User.Text = Language.Login_UserName;
            l_User.TextAlign = ContentAlignment.BottomLeft;
            this.Controls.Add(tb_User = new TextBox());
            tb_User.Location = new Point(l_User.Left, l_User.Bottom + 4);
            tb_User.Size = new Size(this.ClientSize.Width - pb_User.Right - 30, 20);
            tb_User.TextChanged += Tb_User_TextChanged;

            this.Controls.Add(l_Pass = new Label());
            l_Pass.Location = new Point(l_User.Left, tb_User.Bottom + 6);
            l_Pass.AutoSize = true;
            l_Pass.Text = Language.Login_Password;
            l_Pass.TextAlign = ContentAlignment.BottomLeft;
            this.Controls.Add(tb_Pass = new TextBox());
            tb_Pass.Location = new Point(tb_User.Left, l_Pass.Bottom + 4);
            tb_Pass.Size = tb_User.Size;
            tb_Pass.UseSystemPasswordChar = true;
            tb_Pass.PreviewKeyDown += Tb_Pass_PreviewKeyDown;

            this.Controls.Add(cb_AutoLogin = new CheckBox());
            cb_AutoLogin.AutoSize = true;
            cb_AutoLogin.Location = new Point(tb_Pass.Left, tb_Pass.Bottom + 10);
            cb_AutoLogin.Text = "Сохранить пароль";
            cb_AutoLogin.Checked = Config.login_Auto;

            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.Size = new Size((this.ClientSize.Width - tb_Pass.Left - 30) / 2, 30);
            b_Cancel.Location = new Point(this.ClientSize.Width - b_Cancel.Width - 10, cb_AutoLogin.Bottom + 20);
            b_Cancel.Text = Language.Login_Cancel;
            b_Cancel.Click += b_Cancel_Click;
            this.Controls.Add(b_Ok = new Button());
            b_Ok.Size = b_Cancel.Size;
            b_Ok.Location = new Point(b_Cancel.Left - b_Ok.Width -10, b_Cancel.Top);
            b_Ok.Text = Language.Login_OK;
            b_Ok.Click += b_Ok_Click;


            this.ClientSize = new Size(this.ClientSize.Width, b_Cancel.Bottom + 10);


            tb_User.Text = Config.user_NameMain;
            tb_Pass.Text = Config.user_Pass;
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
            UserName = tb_User.Text;
            Password = tb_Pass.Text;
            Config.login_Auto = cb_AutoLogin.Checked;

            switch (Network.User_Auth(tb_User.Text, tb_Pass.Text))
            {
                case 1:
                    Config.user_Name = tb_User.Text;
                    Config.user_NameMain = Config.user_Name;
                    Config.user_Pass = tb_Pass.Text;

                    Config.ConfigFile.Write("Global", "LastUserName", tb_User.Text);
                    Config.ConfigFile.Save();
        
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
                case 2:
                    DialogResult dialog = new ChangePassword(tb_User.Text).ShowDialog();
                    tb_Pass.Text = "";
                    if (dialog == DialogResult.OK)
                    {
                        Config.user_Name = tb_User.Text;
                        Config.user_NameMain = Config.user_Name;

                        Config.ConfigFile.Write("Global", "LastUserName", tb_User.Text);
                        Config.ConfigFile.Save();

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    break;
                case 3:
                    Config.ConfigFile.Write("Global", "LastUserName", tb_User.Text);
                    Config.ConfigFile.Save();

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