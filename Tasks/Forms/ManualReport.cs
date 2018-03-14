using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tasks.Forms
{
    class ManualReport : Form
    {
        RadioButton rb_Plan, rb_Report;
        DateTimePicker dt_Begin, dt_End;
        Button b_Make, b_Cancel;

        public ManualReport()
        {
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            InitialControls();

            this.Text = "Произвольный отчет";
            this.ClientSize = new Size(b_Cancel.Right + 10, b_Cancel.Bottom + 10);

        }
        private void InitialControls()
        {
            bool isPlan = Program.ConfigFile.Read("Reports", "Plan", "false") == "false" ? false : true;

            Graphics g = CreateGraphics();

            this.Controls.Add(rb_Plan = new RadioButton());
            rb_Plan.Location = new Point(10, 10);
            rb_Plan.Text = "План"; rb_Plan.AutoSize = true;
            rb_Plan.Checked = isPlan;
            rb_Plan.AutoCheck = true;

            this.Controls.Add(rb_Report = new RadioButton());
            rb_Report.Location = new Point(rb_Plan.Right + 10, rb_Plan.Top);
            rb_Report.Text = "Отчет"; rb_Report.AutoSize = true;
            rb_Report.Checked = !isPlan;
            rb_Report.AutoCheck = true;

            this.Controls.Add(dt_Begin = new DateTimePicker());
            dt_Begin.Location = new Point(rb_Plan.Left, rb_Plan.Bottom + 10);
            dt_Begin.Value = new DateTime(Convert.ToInt64(Program.ConfigFile.Read("Reports", "ManualDateBegin", DateTime.Now.Ticks.ToString())));
            this.Controls.Add(dt_End = new DateTimePicker());
            dt_End.Location = new Point(dt_Begin.Right + 10, dt_Begin.Top);
            dt_End.Value = new DateTime(Convert.ToInt64(Program.ConfigFile.Read("Reports", "ManualDateEnd", DateTime.Now.Ticks.ToString())));

            this.Controls.Add(b_Cancel = new Button());
            b_Cancel.AutoSize = true;
            b_Cancel.Text = "Отмена";
            b_Cancel.Location = new Point(dt_End.Right - b_Cancel.Width, dt_End.Bottom + 20);
            b_Cancel.Click += b_Cancel_Click;
            this.Controls.Add(b_Make = new Button());
            b_Make.AutoSize = true;
            b_Make.Text = "Сформировать";
            b_Make.Location = new Point(b_Cancel.Left - b_Make.Width - 10, b_Cancel.Top);
            b_Make.Click += b_Make_Click;
            b_Cancel.Height += 10;
            b_Make.Height += 10;
        }

        private void b_Make_Click(object sender, EventArgs e)
        {
            if (dt_Begin.Value.Ticks > dt_End.Value.Ticks)
            {
                MessageBox.Show("Неправильно указан период отчета", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (rb_Plan.Checked == true)
                Tools.MakePlan(dt_Begin.Value, dt_End.Value);
            else
                Tools.MakeReport(dt_Begin.Value, dt_End.Value);

            if (rb_Plan.Checked == true)
                Program.ConfigFile.Write("Reports", "Plan", "true");
            else
                Program.ConfigFile.Write("Reports", "Plan", "false");
            Program.ConfigFile.Write("Reports", "ManualDateBegin", dt_Begin.Value.Ticks.ToString());
            Program.ConfigFile.Write("Reports", "ManualDateEnd", dt_End.Value.Ticks.ToString());
            Program.ConfigFile.Save();
        }

        private void b_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
