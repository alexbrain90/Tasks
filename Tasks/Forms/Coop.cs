using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    public class Coop:Form
    {
        Label l_Info, l_List;
        TextBox tb_Worker;
        ListBox lb_Filter, lb_List;
        Button b_Add, b_Del;

        private long currentID;
        private int[] coopList, selectList;

        public Coop(long id)
        {
            currentID = id;

            this.ClientSize = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Icon = Tasks.Properties.Resources.icon_Main;
            this.Text = "Совместная работа";

            this.Controls.Add(l_Info = new Label());
            l_Info.Text = "Поиск сотрудника";
            this.Controls.Add(tb_Worker = new TextBox());
            this.Controls.Add(lb_Filter = new ListBox());
            this.Controls.Add(b_Add = new Button());
            b_Add.Size = new Size(32, 32);
            b_Add.Image = Tasks.Properties.Resources.button_MovePlus;
            b_Add.Click += b_Add_Click;
            this.Controls.Add(b_Del = new Button());
            b_Del.Size = b_Add.Size;
            b_Del.Image = Tasks.Properties.Resources.button_MoveMinus;
            b_Del.Click += b_Del_Click;
            this.Controls.Add(lb_List = new ListBox());
            this.Controls.Add(l_List = new Label());
            l_List.Text = "Связанные с задачей:";

            l_Info.Location = new Point(10, 10);
            l_Info.Size = new Size((this.ClientSize.Width - 30 - b_Add.Width) / 2, 14);
            tb_Worker.Location = new Point(l_Info.Left, l_Info.Bottom);
            tb_Worker.Size = new Size(l_Info.Width, tb_Worker.Height);
            lb_Filter.Location = new Point(tb_Worker.Left, tb_Worker.Bottom + 10);
            lb_Filter.Size = new Size(l_Info.Width, this.ClientSize.Height - lb_Filter.Top - 10);
            b_Add.Location = new Point(lb_Filter.Right + 5, lb_Filter.Top + (lb_Filter.Bottom - lb_Filter.Top) / 2 - b_Add.Height - 4);
            b_Del.Location = new Point(b_Add.Left, b_Add.Bottom + 8);
            lb_List.Location = new Point(b_Add.Right + 5, lb_Filter.Top);
            lb_List.Size = lb_Filter.Size;
            l_List.Size = new Size(lb_List.Width, 14);
            l_List.Location = new Point(lb_List.Left, lb_List.Top - l_List.Height);

            tb_Worker.TextChanged += tb_Worker_TextChanged;

            this.Shown += Coop_Shown;

            tb_Worker.Enabled = !Config.flag_ReadOnly;
            lb_Filter.Enabled = !Config.flag_ReadOnly;
            lb_List.Enabled = !Config.flag_ReadOnly;
            b_Add.Enabled = !Config.flag_ReadOnly;
            b_Del.Enabled = !Config.flag_ReadOnly;
        }

        private void Coop_Shown(object sender, EventArgs e)
        {
            showCoopList();
        }
        private void tb_Worker_TextChanged(object sender, EventArgs e)
        {
            string[,] list;
            if (tb_Worker.Text == "")
                list = new string[0, 0];
            else
                list = Network.Coop_Filter(currentID, tb_Worker.Text);

            lb_Filter.BeginUpdate();
            lb_Filter.Items.Clear();
            selectList = new int[list.Length / 2];
            for (int i = 0; i < list.Length / 2; i++)
            {
                selectList[i] = Convert.ToInt32(list[i, 0]);
                lb_Filter.Items.Add(list[i, 1]);
            }
            lb_Filter.EndUpdate();
        }

        private void b_Add_Click(object sender, EventArgs e)
        {
            if (lb_Filter.SelectedIndex == -1)
                return;
            int tUser = selectList[lb_Filter.SelectedIndex];
            for (int i = 0; i < coopList.Length; i++)
                if (coopList[i] == tUser)
                    return;

            if (Network.Coop_Add(currentID, tUser) == false)
            {
                MessageBox.Show("Не удалось связать сотрудника с задачей", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            showCoopList();
        }
        private void b_Del_Click(object sender, EventArgs e)
        {
            if (lb_List.SelectedIndex == -1)
                return;
            int tUser = coopList[lb_List.SelectedIndex];

            if (Network.Coop_Delete(currentID, tUser) == false)
            {
                MessageBox.Show("Не удалось отвязать сотрудника от задачи", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            showCoopList();
        }

        private void showCoopList()
        {
            string[,] list = Network.Coop_List(currentID);
            if (list.Length == 0)
            {
                MessageBox.Show("Не удалось получить список сотрудников, связанных с этой задачей", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lb_List.BeginUpdate();
            lb_List.Items.Clear();
            coopList = new int[list.Length / 2];
            for (int i = 0; i < list.Length / 2; i++)
            {
                coopList[i] = Convert.ToInt32(list[i, 0]);
                lb_List.Items.Add(list[i, 1]);
            }
            lb_List.EndUpdate();
        }
    }
}