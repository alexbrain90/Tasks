using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    class CopyTasks : Form
    {
        ComboBox cb_From, cb_To;
        ListBox lb_From;
        Label l_From, l_To;
        Button b_Cancel, b_Copy;
        ProgressBar pb_Progress;

        long[] TaskIDs = new long[0];

        public CopyTasks()
        {
            this.Text = "Копирование задач";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.HelpButton = true;
            this.HelpButtonClicked += CopyTasks_HelpButtonClicked;

            InitializeComponents();
            FillDates();

            this.ClientSize = new Size(cb_To.Right + 10, b_Cancel.Bottom + 10);
        }
        private void CopyTasks_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("Данное окно позволяет выполнить копирование задач из одного месяца в другой.\r\n\r\nПорядок действий:\r\n1. Выберите месяц, с которого нужно выполнить копирование\r\n2. Выберите месяц, в который нужно выполнить копирование задач\r\n3. Выберите необходимые задачи в списке\r\n4. Нажмите на кнопку \"Копировать\"", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Question);
            e.Cancel = true;
        }

        private void InitializeComponents()
        {
            this.Controls.Add(l_From = new Label());
            l_From.Location = new Point(10, 10);
            l_From.AutoSize = true;
            l_From.Text = "Копировать задачи c";
            this.Controls.Add(cb_From = new ComboBox());
            cb_From.Location = new Point(l_From.Left, l_From.Bottom + 4);
            cb_From.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_From.SelectedIndexChanged += cb_From_SelectedIndexChanged;

            this.Controls.Add(l_To = new Label());
            l_To.Location = new Point(cb_From.Right + 50, l_From.Top);
            l_To.AutoSize = true;
            l_To.Text = "в месяц";
            this.Controls.Add(cb_To = new ComboBox());
            cb_To.Location = new Point(l_To.Left, l_To.Bottom + 4);
            cb_To.DropDownStyle = ComboBoxStyle.DropDownList;

            this.Controls.Add(lb_From = new ListBox());
            lb_From.Location = new Point(l_From.Left, cb_From.Bottom + 20);
            lb_From.Size = new Size(cb_To.Right - cb_From.Left, 400);
            lb_From.SelectionMode = SelectionMode.MultiExtended;

            this.Controls.Add(pb_Progress = new ProgressBar());
            pb_Progress.Location = new Point(lb_From.Left, lb_From.Bottom + 10);
            pb_Progress.Size = new Size(lb_From.Width, 20);
            pb_Progress.Visible = false;

            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.AutoSize = true;
            b_Cancel.Text = "Отмена";
            b_Cancel.Height += 10;
            b_Cancel.Location = new Point(pb_Progress.Right - b_Cancel.Width, pb_Progress.Bottom + 10);
            b_Cancel.Click += b_Cancel_Click;
            this.Controls.Add(b_Copy = new Button());
            b_Copy.AutoSize = true;
            b_Copy.Text = "Копировать";
            b_Copy.Height += 10;
            b_Copy.Location = new Point(b_Cancel.Left - b_Copy.Width - 10, b_Cancel.Top);
            b_Copy.Click += b_Copy_Click;
        }
        private void FillDates()
        {

            for(int i =0; i < 12; i++)
            {

                cb_From.Items.Add(Tools.DateTimeToString(DateTime.Now.AddMonths(-i), "MMMMM YYYY"));
                cb_To.Items.Add(Tools.DateTimeToString(DateTime.Now.AddMonths(i+1), "MMMM YYYY"));
            }

            cb_From.SelectedIndex = 0;
            cb_To.SelectedIndex = 0;
        }

        private void cb_From_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime dt1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); dt1 = dt1.AddMonths(-cb_From.SelectedIndex);
            DateTime dt2 = dt1.AddMonths(1).AddTicks(-1);

            string[,] list = Network.Task_List(0, dt1.Ticks, dt2.Ticks, 0);
            lb_From.BeginUpdate();
            lb_From.Items.Clear();
            TaskIDs = new long[list.Length / 6];
            for(int i =0; i < list.Length / 6; i++)
            {
                lb_From.Items.Add(list[i, 0]);
                TaskIDs[i] = Convert.ToInt64(list[i, 1]);
            }
            lb_From.EndUpdate();
        }

        private void b_Copy_Click(object sender, EventArgs e)
        {
            if (lb_From.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Выберите задачи из списка, кторые необходимо скопировать", "Копирование", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            new System.Threading.Thread(CopyThread).Start();
        }
        private void b_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CopyThread()
        {
            cb_From.Enabled = false; cb_To.Enabled = false;
            lb_From.Enabled = false;
            b_Copy.Enabled = false; b_Cancel.Enabled = false;

            pb_Progress.Value = 0;
            pb_Progress.Maximum = lb_From.SelectedIndices.Count;
            pb_Progress.Visible = true;

            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dt = dt.AddMonths(cb_To.SelectedIndex + 1);
            for (int i = 0; i < lb_From.SelectedIndices.Count; i++)
            {
                pb_Progress.Value = i + 1;
                Application.DoEvents();
                CopyTask(TaskIDs[lb_From.SelectedIndices[i]], dt);
            }

            MessageBox.Show("Копирование задач успешно завершено", "Копирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        private void CopyTask(long id, DateTime dt)
        {
            string[] info = Network.Task_Info(id);
            string[,] steps = Network.Step_List(id);
            string[,] coop = Network.Coop_List(id);

            DateTime dt1 = new DateTime(Convert.ToInt64(info[2])), dt2 = new DateTime(Convert.ToInt64(info[3]));
            if (Tools.DateIntervalToID(dt1, dt2).StartsWith("M"))
            {
                dt1 = dt;
                dt2 = dt1.AddMonths(1).AddTicks(-1);
            }
            else
            {
                while (dt1.Ticks < dt.Ticks)
                {
                    dt1 = dt1.AddMonths(1);
                    dt2 = dt2.AddMonths(1);
                }
            }

            long newID = Network.Task_Add(info[0], info[1], dt1.Ticks, dt2.Ticks);
            for(int i =0; i < steps.Length / 3; i++)
                Network.Step_Add(newID, steps[i, 0]);
            for (int i = 0; i < coop.Length / 2; i++)
                if (coop[i,0] != Program.user_IDMain.ToString())
                    Network.Coop_Add(newID, Convert.ToInt32(coop[i, 0]));
        }
    }
}
