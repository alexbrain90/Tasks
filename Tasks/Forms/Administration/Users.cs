using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms.Administration
{
    class Users:Form
    {
        ComboBox cb_Type;
        ListView lv_List;

        public Users()
        {
            this.ClientSize = new Size(1000, 600);
            this.Text = "Список пользователей";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.Icon = Properties.Resources.icon_Main;

            this.Controls.Add(cb_Type = new ComboBox());
            cb_Type.Location = new Point(10, 10);
            cb_Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_Type.Items.AddRange(new string[] { "Полный список", "Версии приложений (А-Я)", "Версии приложений (Я-А)", "С пустым паролем" });
            cb_Type.SelectedIndex = 0;
            cb_Type.SelectedIndexChanged += cb_Type_SelectedIndexChanged;
            cb_Type.Width *= 3;

            this.Controls.Add(lv_List = new ListView());
            lv_List.Location = new Point(10, cb_Type.Bottom + 10);
            lv_List.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - lv_List.Top - 10);
            lv_List.View = View.Details;
            lv_List.Columns.Add("Инициалы", (int)((lv_List.Width - 40) * 0.3));
            lv_List.Columns.Add("СБЕ", (int)((lv_List.Width - 40) * 0.25));
            lv_List.Columns.Add("Должность", (int)((lv_List.Width - 40) * 0.3));
            lv_List.Columns.Add("Версия ПО", (int)((lv_List.Width - 40) * 0.15));
            lv_List.GridLines = true;
            lv_List.FullRowSelect = true;
        }

        private void cb_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[,] list = Network.Admin_User(cb_Type.SelectedIndex);
            lv_List.BeginUpdate();
            lv_List.Items.Clear();
            for (int i = 0; i < list.Length / 4; i++)
                lv_List.Items.Add(new ListViewItem(new string[] { list[i, 0], list[i, 1], list[i, 2], list[i, 3] }));
            lv_List.EndUpdate();
        }
    }
}