using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    class Manager : Form
    {
        public int SelectedUserId;
        public string SelectedUserName;
        TreeView tv_List;
        Button b_Select, b_Cancel;

        public Manager()
        {
            SelectedUserId = -1;
            SelectedUserName = "";
            DialogResult = DialogResult.Cancel;

            this.Text = "Выбор сотрудника";
            this.ClientSize = new Size(400, 500);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.Shown += Manager_Shown;

            this.Controls.Add(tv_List = new TreeView());
            tv_List.Location = new Point(10, 10);
            tv_List.DoubleClick += Tv_List_DoubleClick;
            tv_List.AfterSelect += Tv_List_AfterSelect;
            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.Size = new Size(120, 25);
            b_Cancel.Location = new Point(this.ClientSize.Width - b_Cancel.Width - 10, this.ClientSize.Height - b_Cancel.Height - 10);
            b_Cancel.Text = "Отмена";
            b_Cancel.Click += B_Cancel_Click;
            this.Controls.Add(b_Select = new Button());
            b_Select.Size = b_Cancel.Size;
            b_Select.Location = new Point(b_Cancel.Left - b_Select.Size.Width - 10, b_Cancel.Top);
            b_Select.Text = "Выбрать";
            b_Select.Enabled = false;
            b_Select.Click += B_Select_Click;

            tv_List.Size = new Size(this.ClientSize.Width - 20, b_Cancel.Top - 20);
        }

        private void Tv_List_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tv_List.SelectedNode == null)
                b_Select.Enabled = false;
            else
                b_Select.Enabled = true;
        }
        private void Tv_List_DoubleClick(object sender, EventArgs e)
        {
            B_Select_Click(sender, e);
        }

        private void B_Select_Click(object sender, EventArgs e)
        {
            if (tv_List.SelectedNode == null)
                return;

            SelectedUserId = Convert.ToInt32(tv_List.SelectedNode.Tag);
            SelectedUserName = tv_List.SelectedNode.Text;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }
        private void B_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Manager_Shown(object sender, EventArgs e)
        {
            int tmp = -1;
            if (Config.user_ID != Config.user_IDMain)
                tmp = Config.user_ID;

            Network.Manager_Change(Config.user_IDMain);

            tv_List.BeginUpdate();
            tv_List.Nodes.Clear();

            string[,] list = Network.Manager_List();
            bool[] ready = new bool[list.Length / 3 + 1];
            while (ready[list.Length / 3] == false)
            {
                for (int i = 0; i < list.Length / 3; i++)
                    ready[i] = AddUser(tv_List.Nodes, list[i, 0], list[i, 1], list[i, 2]);

                ready[ready.Length - 1] = true;
                for (int i = 0; i < ready.Length - 1; i++)
                    if (ready[i] == false)
                        ready[ready.Length - 1] = false;
            }

            for (int i = 0; i < tv_List.Nodes.Count; i++)
                tv_List.Nodes[i].Expand();

            tv_List.EndUpdate();

            if (tmp != -1)
                Network.Manager_Change(tmp);
        }
        private bool AddUser(TreeNodeCollection nodes, string id, string name, string manager)
        {
            if (manager == Config.user_IDMain.ToString())
            {
                TreeNode tn = new TreeNode(name);
                tn.Tag = id;
                nodes.Add(tn);
                return true;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Tag.ToString() == manager)
                {
                    TreeNode tn = new TreeNode(name);
                    tn.Tag = id;
                    nodes[i].Nodes.Add(tn);
                    return true;
                }
                if (nodes[i].Nodes.Count > 0)
                    if (AddUser(nodes[i].Nodes, id, name, manager) == true)
                        return true;
            }

            return false;
        }
    }
}