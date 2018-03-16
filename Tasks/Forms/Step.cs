using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    public class Step:Form
    {
        Label l_Info;
        TextBox tb_Value;
        Button b_Save, b_Cancel;

        private long currentID, taskID;

        public Step(long id, long task)
        {
            taskID = task;

            this.Width = 500;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
            this.Icon = Tasks.Properties.Resources.icon_Main;
            if (id == -1)
                this.Text = "Ввод нового шага";
            else
                this.Text = "Редактирование шага";
            currentID = id;

            this.Controls.Add(l_Info = new Label());
            l_Info.Location = new Point(10, 10);
            l_Info.Size = new Size(this.ClientSize.Width - 20, 14);
            l_Info.Text = "Наименование шага:";

            this.Controls.Add(tb_Value = new TextBox());
            tb_Value.Location = new Point(l_Info.Left, l_Info.Bottom);
            tb_Value.Size = new Size(l_Info.Width, tb_Value.Height);

            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.Size = new Size(100, 25);
            b_Cancel.Location = new Point(this.ClientSize.Width - b_Cancel.Width - 10, tb_Value.Bottom + 10);
            b_Cancel.Text = "Отмена";
            b_Cancel.Click += b_Cancel_Click;

            this.Controls.Add(b_Save = new Button());
            b_Save.Size = b_Cancel.Size;
            b_Save.Location = new Point(b_Cancel.Left - b_Save.Width - 10, b_Cancel.Top);
            b_Save.Text = "Сохранить";
            b_Save.Click += b_Save_Click;

            this.ClientSize = new Size(this.ClientSize.Width, b_Cancel.Bottom + 10);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Shown += Step_Shown;
        }

        private void Step_Shown(object sender, EventArgs e)
        {
            if (currentID != -1)
            {
                string result = Network.Step_Info(currentID);
                if (result == "")
                {
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                }
                tb_Value.Text = result;
            }
        }

        private void b_Save_Click(object sender, EventArgs e)
        {

            if (tb_Value.Text == "")
            {
                MessageBox.Show("Не указано имя шага", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (currentID == -1)
            {
                if (Network.Step_Add(taskID, tb_Value.Text) == false)
                {
                    MessageBox.Show("Сервер сообщил об ошибке добавления нового шага", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.Abort;
                    return;
                }
            }
            else
            {
                if (Network.Step_Edit(currentID, tb_Value.Text) == false)
                {
                    MessageBox.Show("Сервер сообщил об ошибке изменения имени шага", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.Abort;
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }
        private void b_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}