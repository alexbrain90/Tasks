using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Tasks.Forms
{
    partial class Main:Form
    {
        
        private MainMenu m_Main;

        private Label l_Updates;

        private Label l_Worker, l_Post, l_Sbe;
        private Button b_SelectWorker;

        private Button b_AddTask;
        private Label l_SortInfo;
        private ListBox lb_Tasks;

        private TextBox tb_Name, tb_Description;
        private Label l_NameInfo, l_DescriptionInfo, l_PregressInfo, l_FilesInfo, l_Dates, l_CoopInfo;
        private DateTimePicker dt_Begin, dt_End;
        private ListBox lb_Progress;
        private Button b_DoTask, b_SaveTask, b_CancelTask, b_AddStep, b_EditStep, b_DoStep, b_FilesShow, b_CoopShow;

        private Label l_MessagesInfo;
        private TextBox tb_Messages, tb_NewMessage;
        private Button b_SendMessage;

        private System.Windows.Forms.Timer t_UpdateTasks;

        private bool HaveChanges = false;
        private TaskInfo[] TasksID = new TaskInfo[0];
        private long[] StepsID = new long[0];
        private bool[] StepsIDMark = new bool[0];
        private long CurrentID = -1;
        private bool flag_NoTaskSelect = false;

        public Main()
        {
            this.Text = Language.Main_Caption;
            this.MinimumSize = new Size(1000, 600);
            this.Shown += Main_Shown;
            this.Resize += Main_Resize;
            this.Paint += Main_Paint;
            this.FormClosing += Main_FormClosing;
            this.Icon = Tasks.Properties.Resources.icon_Main;

            this.Controls.Add(l_Updates = new Label());
            l_Updates.Size = new Size(100, 16);
            l_Updates.Text = "";
            l_Updates.TextAlign = ContentAlignment.MiddleRight;
            l_Updates.BackColor = Color.Transparent;
            l_Updates.TextChanged += l_Updates_TextChanged;

            this.Controls.Add(l_Worker = new Label());
            l_Worker.Location = new Point(10, 10);
            l_Worker.Size = new Size(200, 18);
            l_Worker.Font = new Font(l_Worker.Font.ToString(), 10f);
            this.Controls.Add(l_Post = new Label());
            l_Post.Location = new Point(l_Worker.Left, l_Worker.Bottom);
            l_Post.Size = new Size(l_Worker.Width, 12);
            l_Post.Font = new Font(l_Post.Font.ToString(), 7f);
            this.Controls.Add(l_Sbe = new Label());
            l_Sbe.Location = new Point(l_Post.Left, l_Post.Bottom);
            l_Sbe.Size = new Size(l_Post.Width, l_Post.Height);
            l_Sbe.Font = new Font(l_Sbe.Font.ToString(), 7f);
            this.Controls.Add(b_SelectWorker = new Button());
            b_SelectWorker.Location = new Point(l_Worker.Right + 10, l_Worker.Top);
            b_SelectWorker.Size = new Size(100, l_Sbe.Bottom - l_Worker.Top);
            b_SelectWorker.Text = Language.Main_ButtonSelectWorker;
            b_SelectWorker.Visible = false;

            this.Controls.Add(b_AddTask = new Button());
            b_AddTask.Size = new Size(150, 25);
            b_AddTask.Location = new Point((b_SelectWorker.Right - l_Worker.Left - b_AddTask.Width) / 2, l_Sbe.Bottom + 10);
            b_AddTask.Text = Language.Main_ButtonAddTask;
            b_AddTask.Click += b_AddTask_Click;
            this.Controls.Add(l_SortInfo = new Label());
            l_SortInfo.Location = new Point(l_Worker.Left, b_AddTask.Bottom + 10);
            l_SortInfo.Text = m_Main_Filter_String();
            l_SortInfo.Size = new Size(b_SelectWorker.Right - 10, 30);
            l_SortInfo.TextAlign = ContentAlignment.MiddleLeft;

            this.Controls.Add(lb_Tasks = new ListBox());
            lb_Tasks.Font = new Font(lb_Tasks.Font.FontFamily, 10f);
            lb_Tasks.Location = new Point(l_SortInfo.Left, l_SortInfo.Bottom + 10);
            lb_Tasks.HorizontalScrollbar = false;
            lb_Tasks.DrawMode = DrawMode.OwnerDrawVariable;
            lb_Tasks.SelectedIndexChanged += lb_Tasks_SelectedIndexChanged;
            lb_Tasks.DrawItem += Lb_Tasks_DrawItem;
            lb_Tasks.MeasureItem += Lb_Tasks_MeasureItem;

            // -------------------------------------
            this.Controls.Add(l_NameInfo = new Label());
            l_NameInfo.Location = new Point(b_SelectWorker.Right + 20, l_Worker.Top);
            l_NameInfo.Text = Language.Main_NameInfo;
            l_NameInfo.Height = 14;
            this.Controls.Add(tb_Name = new TextBox());
            tb_Name.Location = new Point(l_NameInfo.Left, l_NameInfo.Bottom + 2);
            tb_Name.Font = new Font(tb_Name.Font.FontFamily, 14f);
            tb_Name.TextChanged += tb_Name_TextChanged;

            this.Controls.Add(l_Dates = new Label());
            l_Dates.Location = new Point(tb_Name.Left, tb_Name.Bottom + 10);
            l_Dates.Text = Language.Main_DatesInfo;
            l_Dates.Size = new Size((int)(this.CreateGraphics().MeasureString(l_Dates.Text, l_Dates.Font).Width) + 1, 14);
            this.Controls.Add(dt_Begin = new DateTimePicker());
            dt_Begin.Location = new Point(l_Dates.Left, l_Dates.Bottom + 2);
            dt_Begin.Width = 120;
            dt_Begin.ValueChanged += dt_Begin_ValueChanged;
            this.Controls.Add(dt_End = new DateTimePicker());
            dt_End.Location = new Point(dt_Begin.Right + 10, dt_Begin.Top);
            dt_End.Width = 120;
            dt_End.ValueChanged += dt_End_ValueChanged;

            this.Controls.Add(l_DescriptionInfo = new Label());
            l_DescriptionInfo.Location = new Point(l_Dates.Left, dt_End.Bottom + 10);
            l_DescriptionInfo.Text = Language.Main_DescriptionInfo;
            l_DescriptionInfo.Height = 14;
            this.Controls.Add(tb_Description = new TextBox());
            tb_Description.Location = new Point(l_DescriptionInfo.Left, l_DescriptionInfo.Bottom + 2);
            tb_Description.Font = new Font(tb_Name.Font.FontFamily, 10f);
            tb_Description.Multiline = true;
            tb_Description.ScrollBars = ScrollBars.Vertical;
            tb_Description.TextChanged += tb_Description_TextChanged;

            this.Controls.Add(l_FilesInfo = new Label());
            l_FilesInfo.TextAlign = ContentAlignment.MiddleLeft;
            l_FilesInfo.Font = new Font(l_FilesInfo.Font.FontFamily, 10f);
            l_FilesInfo.TextChanged += l_FilesInfo_TextChanged;
            this.Controls.Add(b_FilesShow = new Button());
            b_FilesShow.Size = new Size(100, 25);
            b_FilesShow.Text = Language.Main_ButtonShowFiles;
            l_FilesInfo.Height = b_FilesShow.Height;

            this.Controls.Add(l_CoopInfo = new Label());
            l_CoopInfo.TextAlign = ContentAlignment.MiddleLeft;
            l_CoopInfo.Font = new Font(l_FilesInfo.Font.FontFamily, 10f);
            l_CoopInfo.TextChanged += l_CoopInfo_TextChanged;
            this.Controls.Add(b_CoopShow = new Button());
            b_CoopShow.Size = new Size(100, 25);
            b_CoopShow.Text = Language.Main_ButtonShowFiles;
            b_CoopShow.Click += b_CoopShow_Click;
            l_CoopInfo.Height = b_CoopShow.Height;

            this.Controls.Add(l_PregressInfo = new Label());
            l_PregressInfo.Text = Language.Main_ProgressInfo;
            l_PregressInfo.Font = l_FilesInfo.Font;
            this.Controls.Add(lb_Progress = new ListBox());
            //lb_Progress.Font = new Font(lb_Progress.Font.FontFamily, 10);
            //lb_Progress.ItemHeight = 30;
            lb_Progress.DrawMode = DrawMode.OwnerDrawFixed;
            lb_Progress.DrawItem += lb_Progress_DrawItem;
            this.Controls.Add(b_AddStep = new Button());
            b_AddStep.Size = new Size(32, 32);
            b_AddStep.Image = Tasks.Properties.Resources.button_Add;
            b_AddStep.Click += b_AddStep_Click;
            this.Controls.Add(b_EditStep = new Button());
            b_EditStep.Size = b_AddStep.Size;
            b_EditStep.Image = Tasks.Properties.Resources.button_Edit;
            b_EditStep.Click += b_EditStep_Click;
            this.Controls.Add(b_DoStep = new Button());
            b_DoStep.Size = b_EditStep.Size;
            b_DoStep.Image = Tasks.Properties.Resources.button_Do;
            b_DoStep.Click += b_DoStep_Click;

            this.Controls.Add(b_DoTask = new Button());
            b_DoTask.Size = new Size(100, 25);
            b_DoTask.Text = Language.Main_ButtonDoTask;
            b_DoTask.Click += b_DoTask_Click;
            this.Controls.Add(b_SaveTask = new Button());
            b_SaveTask.Size = b_DoTask.Size;
            b_SaveTask.Text = Language.Main_ButtonSaveTask;
            b_SaveTask.Click += b_SaveTask_Click;
            this.Controls.Add(b_CancelTask = new Button());
            b_CancelTask.Size = b_SaveTask.Size;
            b_CancelTask.Text = Language.Main_ButtonCancelTask;
            b_CancelTask.Click += b_CancelTask_Click;

            //-----------------------------------------------------
            this.Controls.Add(l_MessagesInfo = new Label());
            l_MessagesInfo.Text = Language.Main_MessagesInfo;
            this.Controls.Add(tb_Messages = new TextBox());
            tb_Messages.Multiline = true;
            tb_Messages.ScrollBars = ScrollBars.Vertical;
            tb_Messages.ReadOnly = true;
            this.Controls.Add(tb_NewMessage = new TextBox());
            tb_NewMessage.Height = 60;
            tb_NewMessage.Multiline = true;
            tb_NewMessage.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(b_SendMessage = new Button());
            b_SendMessage.Size = new Size(100, 25);
            b_SendMessage.Text = "Отправить";
            b_SendMessage.Click += B_SendMessage_Click;

            setElementsVisible(false);

            DateTime dt1 = DateTime.Now;
            if (dt1.Day >= 25)
                dt1 = dt1.AddMonths(1);
            dt1 = new DateTime(dt1.Year, dt1.Month, 1);
            DateTime dt2 = DateTime.Now;
            if (dt2.Day < 6)
                dt2 = dt2.AddMonths(-1);
            dt2 = new DateTime(dt2.Year, dt2.Month, 1);

            m_Main = new MainMenu();
            m_Main.MenuItems.Add("Сотрудник", m_Main_Empty);
            m_Main.MenuItems[0].MenuItems.Add("Список подчиненных...", m_Main_User_Manager);
            m_Main.MenuItems[0].MenuItems.Add("Свои задачи", m_Main_User_Back);
            m_Main.MenuItems[0].MenuItems.Add("-");
            m_Main.MenuItems[0].MenuItems.Add("Сменить пароль", m_Main_User_ChangePassword);
            m_Main.MenuItems[0].MenuItems.Add("Выход", m_Main_User_Exit);
            m_Main.MenuItems.Add("Задачи", m_Main_Empty);
            m_Main.MenuItems[1].MenuItems.Add("Новая", m_Main_Tasks_New);
            m_Main.MenuItems[1].MenuItems.Add("Копировать...", m_Main_Tasks_Copy);
            m_Main.MenuItems[1].MenuItems.Add("-");
            m_Main.MenuItems[1].MenuItems.Add("Сортировка по");
            m_Main.MenuItems[1].MenuItems[3].MenuItems.Add("Наименованию", m_Main_Sort_Name);
            m_Main.MenuItems[1].MenuItems[3].MenuItems.Add("Дате добавления", m_Main_Sort_DateAdd);
            m_Main.MenuItems[1].MenuItems[3].MenuItems.Add("Дате начала работы", m_Main_Sort_DateStart);
            m_Main.MenuItems[1].MenuItems[3].MenuItems.Add("Сроку выполнения", m_Main_Sort_DateEnd);
            //m_Main.MenuItems[1].MenuItems[3].MenuItems.Add("-");
            //m_Main.MenuItems[1].MenuItems[3].MenuItems.Add("", m_Main_Filter_No);
            m_Main.MenuItems[1].MenuItems.Add("Фильтр по дате");
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("Без фильтра", m_Main_Filter_No);
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("-");
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("Предыдущая неделя", m_Main_Filter_PrevWeek);
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("Текущая неделя", m_Main_Filter_CurWeek);
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("Следующая неделя", m_Main_Filter_NextWeek);
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("-");
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("Предыдущий месяц", m_Main_Filter_PrevMonth);
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("Текущий месяц", m_Main_Filter_CurMonth);
            m_Main.MenuItems[1].MenuItems[4].MenuItems.Add("Следующий месяц", m_Main_Filter_NextMonth);
            m_Main.MenuItems[1].MenuItems.Add("Фильтр по статусу");
            m_Main.MenuItems[1].MenuItems[5].MenuItems.Add("Без фильтра", m_Main_Filter_All);
            m_Main.MenuItems[1].MenuItems[5].MenuItems.Add("В работе", m_Main_Filter_InWork);
            m_Main.MenuItems[1].MenuItems[5].MenuItems.Add("Выполненные", m_Main_Filter_Ready);
            m_Main.MenuItems.Add("Отчеты", m_Main_Empty);
            m_Main.MenuItems[2].MenuItems.Add("План на " + Tools.DateToString(dt1, "MMMM"), m_Main_Report_Plan);
            m_Main.MenuItems[2].MenuItems.Add("Отчет за " + Tools.DateToString(dt2, "MMMM"), m_Main_Report_Report);
            m_Main.MenuItems[2].MenuItems.Add("-");
            m_Main.MenuItems[2].MenuItems.Add("Произвольный...", m_Main_Report_Manual);
            m_Main.MenuItems.Add("Сервис", m_Main_Empty); m_Main.MenuItems[3].Enabled = false;
            m_Main.MenuItems[3].MenuItems.Add("Параметры", m_Main_Empty);
            m_Main.MenuItems.Add("Справка", m_Main_Empty); m_Main.MenuItems[4].Enabled = false;
            m_Main.MenuItems[4].MenuItems.Add("Помощь", m_Main_Empty);
            m_Main.MenuItems[4].MenuItems.Add("-");
            m_Main.MenuItems[4].MenuItems.Add("О программе", m_Main_Empty);
            this.Menu = m_Main;

            try
            {
                this.Left = Convert.ToInt32(Program.ConfigFile.Read("MainForm", "LocationX"));
                this.Top = Convert.ToInt32(Program.ConfigFile.Read("MainForm", "LocationY"));
                this.Width = Convert.ToInt32(Program.ConfigFile.Read("MainForm", "SizeX"));
                this.Height = Convert.ToInt32(Program.ConfigFile.Read("MainForm", "SizeY"));
                if (Program.ConfigFile.Read("MainForm", "Maximized") == "true")
                    this.WindowState = FormWindowState.Maximized;
                else
                    this.WindowState = FormWindowState.Normal;
            }
            catch
            {
                Program.ConfigFile.Write("MainForm", "Maximized", "false");
                Program.ConfigFile.Write("MainForm", "LocationX", this.Left.ToString());
                Program.ConfigFile.Write("MainForm", "LocationY", this.Top.ToString());
                Program.ConfigFile.Write("MainForm", "SizeX", this.Width.ToString());
                Program.ConfigFile.Write("MainForm", "SizeY", this.Height.ToString());
            }
            this.Main_Resize(this, new EventArgs());

            t_UpdateTasks = new System.Windows.Forms.Timer();
            t_UpdateTasks.Interval = 30000;
            t_UpdateTasks.Tick += t_UpdateTasks_Tick;
        }

        private void Lb_Tasks_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index == -1)
                return;
            if (TasksID[e.Index].Height != 0)
                e.ItemHeight = TasksID[e.Index].Height;
        }

        private void Lb_Tasks_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == -1)
                return;

            string line1 = TasksID[e.Index].Name;
            Font font1 = e.Font;
            Size size1 = e.Graphics.MeasureString(line1 + "\r\n" + line1, font1).ToSize();
            string line2 = TasksID[e.Index].Dates;
            Font font2 = new Font(e.Font.FontFamily, e.Font.Size - 2);
            Size size2 = e.Graphics.MeasureString(line2, font2).ToSize();
            TasksID[e.Index].Height = size1.Height + size2.Height + 4;

            Color color = Color.White;
            if (e.BackColor == lb_Tasks.BackColor)
            //{
            //    if (TasksID[e.Index].dFinish == 0)
            //        color = Color.White;
            //    if (TasksID[e.Index].dEnd < DateTime.Now.Ticks)
            //        color = Color.FromArgb(255, 144, 144);
            //    if (TasksID[e.Index].dFinish != 0)
            //        color = Color.FromArgb(144, 255, 144); ;
            //    if (TasksID[e.Index].dStart > DateTime.Now.Ticks)
            //        color = Color.FromArgb(144, 144, 255);
            //}
            //else
            {
                if (TasksID[e.Index].dFinish == 0)
                    color = Color.Black;
                if (TasksID[e.Index].dEnd < DateTime.Now.AddDays(2).Ticks && TasksID[e.Index].dEnd > DateTime.Now.Ticks)
                    color = Color.FromArgb(95, 95, 0);
                if (TasksID[e.Index].dEnd < DateTime.Now.Ticks)
                    color = Color.FromArgb(159, 0, 0);
                if (TasksID[e.Index].dFinish != 0)
                    color = Color.FromArgb(0, 159, 0);
                if (TasksID[e.Index].dStart > DateTime.Now.Ticks)
                    color = Color.FromArgb(0, 95, 159);
            }

            Rectangle bounds = new Rectangle(e.Bounds.X + 27, e.Bounds.Y, e.Bounds.Width - 27, size1.Height);
            //Rectangle bounds = new Rectangle(e.Bounds.X + 27, e.Bounds.Y, Bounds.Width - 27, size1.Height);
            e.Graphics.DrawString(line1, font1, new SolidBrush(color), bounds, StringFormat.GenericDefault);
            bounds = new Rectangle(bounds.X, bounds.Y + size1.Height, bounds.Width, size2.Height);
            e.Graphics.DrawString(line2, font2, new SolidBrush(color), bounds, StringFormat.GenericDefault);

            if (TasksID[e.Index].dStart > DateTime.Now.Ticks)
                e.Graphics.DrawImage(Properties.Resources.task_Future, e.Bounds.X, e.Bounds.Y);
            else if (TasksID[e.Index].dFinish != 0)
                e.Graphics.DrawImage(Properties.Resources.task_Ready, e.Bounds.X, e.Bounds.Y);
            else if (TasksID[e.Index].dEnd < DateTime.Now.AddDays(2).Ticks && TasksID[e.Index].dEnd > DateTime.Now.Ticks)
                e.Graphics.DrawImage(Properties.Resources.task_Near, e.Bounds.X, e.Bounds.Y);
            else if (TasksID[e.Index].dEnd < DateTime.Now.Ticks)
                e.Graphics.DrawImage(Properties.Resources.task_Miss, e.Bounds.X, e.Bounds.Y);
            else
                e.Graphics.DrawImage(Properties.Resources.task_Normal, e.Bounds.X, e.Bounds.Y);

            if (TasksID[e.Index].Coop != 1)
                e.Graphics.DrawImage(Properties.Resources.task_Coop, e.Bounds.X, e.Bounds.Y);

            e.Graphics.DrawLine(new Pen(Color.LightGray), e.Bounds.Left + 1, e.Bounds.Bottom - 1, e.Bounds.Right - 1, e.Bounds.Bottom - 1);

            e.DrawFocusRectangle();
        }

        private void lb_Progress_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index == -1)
                return;

            Brush brush;
            if (StepsIDMark[e.Index] == true)
            {
                if (e.BackColor != lb_Progress.BackColor)
                    brush = Brushes.LightGray;
                else
                    brush = Brushes.DimGray;
            }
            else
            {
                if (e.BackColor != lb_Progress.BackColor)
                    brush = Brushes.White;
                else
                    brush = Brushes.Black;
            }

            // Draw first line with name
            /*string text = lb_Progress.Items[e.Index].ToString();
            Rectangle bounds = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, (int)(e.Graphics.MeasureString(text,e.Font).Height + 1));
            e.Graphics.DrawString(text, e.Font, brush, bounds, StringFormat.GenericDefault);

            bounds = new Rectangle(e.Bounds.Left, e.Bounds.Top + e.Bounds.Height / 2, e.Bounds.Width, e.Bounds.Height / 2);
            text = "Добавлено: " + Tools.DateTimeToString(StepsID[e.Index], "DD.MM.YYYY  hh:mm");
            e.Graphics.DrawString(text, new Font(e.Font.FontFamily, e.Font.Size / 3 * 2), brush, bounds, StringFormat.GenericDefault);

            e.Graphics.DrawLine(new Pen(Color.LightGray), e.Bounds.Left + 1, e.Bounds.Bottom - 1, e.Bounds.Right - 1, e.Bounds.Bottom - 1);*/

            e.Graphics.DrawString(lb_Progress.Items[e.Index].ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);

            e.DrawFocusRectangle();
        }

        private void t_UpdateTasks_Tick(object sender, EventArgs e)
        {
            getTasksList();
        }

        private void B_SendMessage_Click(object sender, EventArgs e)
        {
            if (tb_NewMessage.Text == "" || CurrentID == -1)
                return;

            if (Network.Message_Add(CurrentID, tb_NewMessage.Text) == false)
            {
                MessageBox.Show("Не удалось отправить комментарий", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                showTaskInfo(CurrentID);
            }
        }

        private void b_CoopShow_Click(object sender, EventArgs e)
        {
            new Coop(CurrentID).ShowDialog();
            showTaskInfo(CurrentID);
        }

        private void b_AddStep_Click(object sender, EventArgs e)
        {
            Step dialog = new Step(-1, TasksID[lb_Tasks.SelectedIndex].dAdd);
            dialog.ShowDialog();
            
            showTaskInfo(CurrentID);
        }
        private void b_EditStep_Click(object sender, EventArgs e)
        {
            Step dialog;
            if (lb_Progress.SelectedIndex == -1)
                return;
            else
                dialog = new Step(StepsID[lb_Progress.SelectedIndex], TasksID[lb_Tasks.SelectedIndex].dAdd);
            dialog.ShowDialog();

            showTaskInfo(TasksID[lb_Tasks.SelectedIndex].dAdd);
        }
        private void b_DoStep_Click(object sender, EventArgs e)
        {
            
            if (lb_Progress.SelectedIndex == -1)
                return;
            else
            {
                DialogResult dr = MessageBox.Show("Вы действительно хотите поставить отметку выполнения для выбранного шага? Действие отменить невозможно\r\n\r\n" + (string)lb_Progress.SelectedItem, "Выполнение шага", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (Network.Step_Do(StepsID[lb_Progress.SelectedIndex]) == false)
                        MessageBox.Show("Ошибка при установки отметки выполнения шага " + (string)lb_Progress.SelectedItem, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            showTaskInfo(TasksID[lb_Tasks.SelectedIndex].dAdd);
        }

        private void b_AddTask_Click(object sender, EventArgs e)
        {
            dt_Begin.Value = new DateTime(dt_Begin.Value.Year, dt_Begin.Value.Month, dt_Begin.Value.Day);
            dt_End.Value = new DateTime(dt_End.Value.Year, dt_End.Value.Month, dt_End.Value.Day).AddDays(1).AddTicks(-1);

            saveTaskInfo(false);

            CurrentID = -1;

            resetTaskInfo(CurrentID);

            setElementsVisible(true);

            b_DoTask.Enabled = false;
            b_SaveTask.Enabled = false;
            b_CancelTask.Enabled = false;
        }
        private void b_DoTask_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Вы действительно хотите пометить задачу \"" + (string)lb_Tasks.SelectedItem + "\" как выполненную?", "Выполнение задачи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                if (Network.Task_Do(CurrentID) == false)
                {
                    MessageBox.Show("Ошибка при установки отметки выполнения задачи " + (string)lb_Progress.SelectedItem, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            getTasksList();
        }
        private void b_SaveTask_Click(object sender, EventArgs e)
        {
            if (tb_Name.Text == "" || tb_Description.Text == "")
            {
                MessageBox.Show("Необходимо заполнить поля \"Наименование\" и \"Описание\"", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dt_Begin.Value.Ticks > dt_End.Value.Ticks)
            {
                MessageBox.Show("Неправильно указан период выполнения задачи", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (saveTaskInfo(false) == true)
                getTasksList();
        }
        private void b_CancelTask_Click(object sender, EventArgs e)
        {
            saveTaskInfo(false);

            resetTaskInfo(CurrentID);

            b_SaveTask.Enabled = false;
            b_CancelTask.Enabled = false;
        }

        private void canSaveChanges()
        {
            HaveChanges = true;
            if (b_SaveTask.Enabled == false)
                b_SaveTask.Enabled = true;
            if (b_CancelTask.Enabled == false)
                b_CancelTask.Enabled = true;
        }
        private void tb_Name_TextChanged(object sender, EventArgs e)
        {
            canSaveChanges();
        }
        private void tb_Description_TextChanged(object sender, EventArgs e)
        {
            canSaveChanges();
        }
        private void dt_Begin_ValueChanged(object sender, EventArgs e)
        {
            canSaveChanges();
        }
        private void dt_End_ValueChanged(object sender, EventArgs e)
        {
            canSaveChanges();
        }

        private void l_FilesInfo_TextChanged(object sender, EventArgs e)
        {
            l_FilesInfo.Width = (int)this.CreateGraphics().MeasureString(l_FilesInfo.Text, l_FilesInfo.Font).Width + 10;
            b_FilesShow.Location = new Point(l_FilesInfo.Right + 10, l_FilesInfo.Top);
        }
        private void l_CoopInfo_TextChanged(object sender, EventArgs e)
        {
            l_CoopInfo.Width = (int)this.CreateGraphics().MeasureString(l_CoopInfo.Text, l_CoopInfo.Font).Width + 15;
            b_CoopShow.Location = new Point(l_CoopInfo.Right + 10, l_CoopInfo.Top);
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);
            e.Graphics.DrawRectangle(new Pen(Color.Black), l_Worker.Left - 5, l_Worker.Top - 5, b_SelectWorker.Right - l_Worker.Left + 10, b_AddTask.Bottom - l_Worker.Top + 10);
            e.Graphics.DrawRectangle(new Pen(Color.Black), l_SortInfo.Left - 5, l_SortInfo.Top - 5, l_SortInfo.Right - l_SortInfo.Left + 10, this.ClientSize.Height - l_SortInfo.Top);
            e.Graphics.DrawRectangle(new Pen(Color.Black), l_NameInfo.Left - 5, l_NameInfo.Top - 5, this.ClientSize.Width - l_NameInfo.Left, this.ClientSize.Height - l_NameInfo.Top);
            e.Graphics.DrawLine(new Pen(Color.Black), l_NameInfo.Right + 5, l_NameInfo.Top - 5, l_NameInfo.Right + 5, this.ClientSize.Height - 5);
        }
        private void Main_Resize(object sender, EventArgs e)
        {
            l_Updates.Location = new Point(this.ClientSize.Width - l_Updates.Width - 5, l_NameInfo.Top - 5);

            lb_Tasks.Size = new Size(l_SortInfo.Right - l_SortInfo.Left, this.ClientSize.Height - l_SortInfo.Bottom - 20);

            int x = this.ClientSize.Width - tb_Name.Left - 10, y = this.ClientSize.Height - 20;
            l_NameInfo.Size = new Size((int)(x * 0.6) - 5, l_NameInfo.Height);
            tb_Name.Size = new Size(l_NameInfo.Width, tb_Name.Height);
            l_DescriptionInfo.Size = new Size(l_NameInfo.Width, l_DescriptionInfo.Height);
            tb_Description.Size = new Size(l_NameInfo.Width, (int)(y * 0.4) - tb_Name.Bottom - 10);
            l_FilesInfo.Location = new Point(tb_Description.Left, tb_Description.Bottom + 10);
            b_FilesShow.Location = new Point(l_FilesInfo.Right + 10, l_FilesInfo.Top);
            l_CoopInfo.Location = new Point(l_FilesInfo.Left, l_FilesInfo.Bottom + 10);
            b_CoopShow.Location = new Point(l_CoopInfo.Right + 10, l_CoopInfo.Top);
            l_PregressInfo.Location = new Point(l_CoopInfo.Left, l_CoopInfo.Bottom + 10);
            l_PregressInfo.Size = new Size(tb_Description.Width, 20);
            lb_Progress.Location = new Point(l_PregressInfo.Left, l_PregressInfo.Bottom + 4);
            lb_Progress.Size = new Size(l_PregressInfo.Width - b_AddStep.Width - 10, this.ClientSize.Height - lb_Progress.Top - 45);
            b_AddStep.Location = new Point(lb_Progress.Right + 10, lb_Progress.Top);
            b_EditStep.Location = new Point(b_AddStep.Left, b_AddStep.Bottom + 5);
            b_DoStep.Location = new Point(b_EditStep.Left, b_EditStep.Bottom + 5);

            l_MessagesInfo.Location = new Point(l_NameInfo.Right + 10, l_NameInfo.Top);
            l_MessagesInfo.Size = new Size(this.ClientSize.Width - l_MessagesInfo.Left - 10, tb_Name.Top - l_MessagesInfo.Top - 2);
            b_SendMessage.Location = new Point(this.ClientSize.Width - b_SendMessage.Width - 10, this.ClientSize.Height - b_SendMessage.Height - 10);
            tb_NewMessage.Size = new Size(l_MessagesInfo.Width, tb_NewMessage.Height);
            tb_NewMessage.Location = new Point(l_MessagesInfo.Left, b_SendMessage.Top - tb_NewMessage.Height - 10);
            tb_Messages.Location = new Point(l_MessagesInfo.Left, tb_Name.Top);
            tb_Messages.Size = new Size(l_MessagesInfo.Width, tb_NewMessage.Top - tb_Messages.Top - 10);
            b_CancelTask.Location = new Point(b_DoStep.Right - b_CancelTask.Width, this.ClientSize.Height - b_CancelTask.Height - 10);
            b_SaveTask.Location = new Point(b_CancelTask.Left - b_SaveTask.Width - 10, b_CancelTask.Top);
            b_DoTask.Location = new Point(b_SaveTask.Left - b_DoTask.Width - 20, b_SaveTask.Top);

            this.OnPaint(new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
            lb_Progress.Refresh();
        }
        private void Main_Shown(object sender, EventArgs e)
        {
            if (Program.flag_ReadOnly == false)
            {
                Forms.Login f_Login = new Login();
                f_Login.ShowDialog();
                if (f_Login.DialogResult == DialogResult.Cancel)
                {
                    Application.Exit();
                    return;
                }

                new Thread(checkUpdates).Start();
            }

            if (Network.User_Info() == true)
            {
                this.Text = Program.user_Fio + " - " + Program.user_Post + " - " + Program.user_Sbe;
                l_Worker.Text = Program.user_Name;
                l_Post.Text = Program.user_Post;
                l_Sbe.Text = Program.user_Sbe;
            }

            b_AddTask.Enabled = !Program.flag_ReadOnly;
            m_Main.MenuItems[1].MenuItems[0].Enabled = !Program.flag_ReadOnly;
            m_Main.MenuItems[1].MenuItems[1].Enabled = !Program.flag_ReadOnly;

            //getTasksList();

            t_UpdateTasks.Enabled = true;

            switch(Program.ConfigFile.Read("MainForm", "Sort", "0"))
            {
                case "0":
                    m_Main_Sort_Name(sender, e);
                    break;
                case "1":
                    m_Main_Sort_DateAdd(sender, e);
                    break;
                case "2":
                    m_Main_Sort_DateStart(sender, e);
                    break;
                case "3":
                    m_Main_Sort_DateEnd(sender, e);
                    break;
            }
            switch (Program.ConfigFile.Read("MainForm", "FilterDate", "0"))
            {
                case "0":
                    m_Main_Filter_No(sender, e);
                    break;

                case "2":
                    m_Main_Filter_PrevWeek(sender, e);
                    break;
                case "3":
                    m_Main_Filter_CurWeek(sender, e);
                    break;
                case "4":
                    m_Main_Filter_NextWeek(sender, e);
                    break;

                case "6":
                    m_Main_Filter_PrevMonth(sender, e);
                    break;
                case "7":
                    m_Main_Filter_CurMonth(sender, e);
                    break;
                case "8":
                    m_Main_Filter_NextMonth(sender, e);
                    break;
            }
            switch (Program.ConfigFile.Read("MainForm", "FilterStatus", "1"))
            {
                case "0":
                    m_Main_Filter_All(sender, e);
                    break;
                case "1":
                    m_Main_Filter_InWork(sender, e);
                    break;
                case "2":
                    m_Main_Filter_Ready(sender, e);
                    break;
            }
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                Program.ConfigFile.Write("MainForm", "Maximized", "true");
            else if (this.WindowState == FormWindowState.Normal)
            {
                Program.ConfigFile.Write("MainForm", "Maximized", "false");
                Program.ConfigFile.Write("MainForm", "LocationX", this.Left.ToString());
                Program.ConfigFile.Write("MainForm", "LocationY", this.Top.ToString());
                Program.ConfigFile.Write("MainForm", "SizeX", this.Width.ToString());
                Program.ConfigFile.Write("MainForm", "SizeY", this.Height.ToString());
            }

            Program.ConfigFile.Save();
        }
        private void setElementsVisible(bool Value)
        {
            l_NameInfo.Visible = Value; tb_Name.Visible = Value;
            l_Dates.Visible = Value; dt_Begin.Visible = Value; dt_End.Visible = Value;
            l_DescriptionInfo.Visible = Value; tb_Description.Visible = Value;
            l_FilesInfo.Visible = Value; b_FilesShow.Visible = Value;
            l_CoopInfo.Visible = Value; b_CoopShow.Visible = Value;
            l_PregressInfo.Visible = Value; lb_Progress.Visible = Value; b_AddStep.Visible = Value; b_EditStep.Visible = Value; b_DoStep.Visible = Value;
            b_DoTask.Visible = Value; b_SaveTask.Visible = Value; b_CancelTask.Visible = Value;
            l_MessagesInfo.Visible = Value; tb_Messages.Visible = Value; tb_NewMessage.Visible = Value; b_SendMessage.Visible = Value;
        }

        private void checkUpdates()
        {
            try
            {
                m_Main.MenuItems[2].Enabled = false;
                l_Updates.Text = "Проверка библиотек...";
                Thread.Sleep(2500);

                if (Tasks.Update.checkFiles() == false)
                {
                    l_Updates.Text = "Загрузка библиотек...";
                    if (Tasks.Update.downloadFiles() == true)
                    {
                        m_Main.MenuItems[2].Enabled = true;
                        l_Updates.Text = "Библиотеки загружены";
                    }
                    else
                        l_Updates.Text = "Не удалось загрузить библиотеки";
                }
                else
                    m_Main.MenuItems[2].Enabled = true;

                Thread.Sleep(2500);
                l_Updates.Text = Language.Main_UpdateCheck;
                Thread.Sleep(2500);
                Tasks.Update.checkUpgrade();

                if (Program.CurrentVersion != Program.ServerVersion)
                {
                    l_Updates.Text = Language.Main_UpdateExist;
                    l_Updates.Font = new Font(l_Updates.Font, FontStyle.Underline);
                    l_Updates.ForeColor = Color.DarkRed;
                    l_Updates.Cursor = Cursors.Hand;
                    l_Updates.Click += l_Updates_Click;
                }
                else
                {
                    l_Updates.Text = Language.Main_UpdateNo;
                    l_Updates.ForeColor = Color.DarkGray;

                    Thread.Sleep(10000);
                    l_Updates.Visible = false;
                }
            }
            catch { }
        }
        private void l_Updates_Click(object sender, EventArgs e)
        {
            DialogResult dr = new Tasks.Forms.Update().ShowDialog();
            if (dr == DialogResult.Yes)
                if (Tasks.Update.makeUpgrade(new string[0]) == true)
                    Application.Exit();
        }
        private void l_Updates_TextChanged(object sender, EventArgs e)
        {
            l_Updates.Width = (int)(this.CreateGraphics().MeasureString(l_Updates.Text, l_Updates.Font).Width) + 1;
            l_Updates.Left = this.ClientSize.Width - l_Updates.Width - 5;
        }

        private void lb_Tasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flag_NoTaskSelect == true)
                return;

            if (lb_Tasks.SelectedIndex == -1)
                return;

            saveTaskInfo(false);

            showTaskInfo(TasksID[lb_Tasks.SelectedIndex].dAdd);
        }

        private void getTasksList()
        {
            flag_NoTaskSelect = true;

            long prevID = CurrentID;
            int n = -1;

            Application.DoEvents();

            string[,] list = Network.Task_List(filterStatus, filterB, filterE, sortId);

            lb_Tasks.BeginUpdate();
            TasksID = new TaskInfo[list.Length / 6];

            if (lb_Tasks.Items.Count > TasksID.Length)
                for (int i = lb_Tasks.Items.Count - 1; i >= TasksID.Length; i--)
                    lb_Tasks.Items.RemoveAt(i);
            if (lb_Tasks.Items.Count < TasksID.Length)
                for (int i = lb_Tasks.Items.Count; i < TasksID.Length; i++)
                    lb_Tasks.Items.Add(new object());

            for (int i = 0; i < TasksID.Length; i++)
            {
                TasksID[i].Name = list[i, 0];
                TasksID[i].dAdd = Convert.ToInt64(list[i, 1]);
                TasksID[i].dFinish = Convert.ToInt64(list[i, 2]);
                TasksID[i].dStart = Convert.ToInt64(list[i, 3]);
                TasksID[i].dEnd = Convert.ToInt64(list[i, 4]);
                TasksID[i].Coop = Convert.ToInt32(list[i, 5]);
                if (TasksID[i].dAdd == prevID)
                    n = i;

                string line1 = TasksID[i].Name;
                Font font1 = lb_Tasks.Font;
                Size size1 = CreateGraphics().MeasureString(line1 + "\r\n" + line1, font1).ToSize();
                string line2 = Tools.DateIntervalToString(TasksID[i].dStart, TasksID[i].dEnd);
                if (line2 == "")
                    line2 = Tools.DateTimeToString(TasksID[i].dStart, "DD.MM.YYYY") + " - " + Tools.DateTimeToString(TasksID[i].dEnd, "DD.MM.YYYY");
                Font font2 = new Font(font1.FontFamily, font1.Size - 2);
                Size size2 = CreateGraphics().MeasureString(line2, font2).ToSize();
                TasksID[i].Height = size1.Height + size2.Height + 4;
                TasksID[i].Dates = line2;

                DateTime dt = new DateTime(Convert.ToInt64(list[i, 2]));
                lb_Tasks.Items[i] = (object)list[i, 0];
                if (TasksID[i].dAdd == CurrentID)
                    lb_Tasks.SelectedIndex = i;
            }
            lb_Tasks.EndUpdate();
            lb_Tasks.ClearSelected();

            if (n != -1)
                lb_Tasks.SelectedIndex = n;

            flag_NoTaskSelect = false;
        }
        private void showTaskInfo(long id)
        {
            string[] list = Network.Task_Info(id);
            tb_Name.Text = list[0];
            tb_Description.Text = list[1].Replace("%newline%", "\r\n");
            dt_Begin.Value = new DateTime(Convert.ToInt64(list[2]));
            dt_End.Value = new DateTime(Convert.ToInt64(list[3]));
            l_FilesInfo.Text = "Прикрепленных файлов: " + list[4];
            if (list[5] == "1")
                l_CoopInfo.Text = "Совместная работа: только я";
            else
                l_CoopInfo.Text = "Совместная работа: я и еще " + (Convert.ToInt32(list[5]) - 1).ToString();

            string[,] list2 = Network.Step_List(id);
            lb_Progress.BeginUpdate();
            lb_Progress.Items.Clear();
            StepsID = new long[list2.Length / 3];
            StepsIDMark = new bool[StepsID.Length];

            int n = 0;
            for (int i = 0; i < StepsID.Length; i++)
            {
                StepsID[i] = Convert.ToInt64(list2[i, 2]);
                string temp = list2[i, 0];
                if (list2[i, 1] != "")
                {
                    StepsIDMark[i] = true;
                    n++;
                }
                lb_Progress.Items.Add(temp);
            }
            lb_Progress.EndUpdate();

            if (StepsID.Length > 0)
            {
                int p = (int)(n * 100 / StepsID.Length);
                l_PregressInfo.Text = "Процент выполнения задачи: " + p.ToString() + "%";
                b_EditStep.Enabled = !Program.flag_ReadOnly;
                b_DoStep.Enabled = !Program.flag_ReadOnly;

                if (p == 100 && list[6] == "0")
                    b_DoTask.Enabled = !Program.flag_ReadOnly;
                else
                    b_DoTask.Enabled = false;
            }
            else
            {
                l_PregressInfo.Text = "";
                b_EditStep.Enabled = false;
                b_DoStep.Enabled = false;

                if (list[6] == "0")
                    b_DoTask.Enabled = !Program.flag_ReadOnly;
                else
                    b_DoTask.Enabled = false;
            }

            tb_Messages.Text = ""; tb_NewMessage.Text = "";
            string[,] messages = Network.Message_List(id);
            for (int i = 0; i < messages.Length / 3; i++)
            {
                if (i != 0)
                    tb_Messages.Text += "\r\n\r\n";

                DateTime dt = new DateTime(Convert.ToInt64(messages[i, 1]));
                messages[i, 1] = dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + " " + dt.Day.ToString("00") + "." + dt.Month.ToString("00") + "." + dt.Year.ToString("0000");
                messages[i, 2] = messages[i, 2].Replace("%newline%", "\r\n");
                tb_Messages.Text += "     " + messages[i, 0] + " (" + messages[i, 1] + ")\r\n" + messages[i, 2];
            }
            if (tb_Messages.Text.Length > 0)
            {
                tb_Messages.SelectionStart = tb_Messages.Text.Length - 1;
                tb_Messages.ScrollToCaret();
            }

            CurrentID = id;
            setElementsVisible(true);
            b_FilesShow.Enabled = !Program.flag_ReadOnly;
            b_CoopShow.Enabled = true;
            b_AddStep.Enabled = !Program.flag_ReadOnly;

            HaveChanges = false;
            b_SaveTask.Enabled = false;
            b_CancelTask.Enabled = false;

            b_SendMessage.Enabled = !Program.flag_ReadOnly;
            tb_NewMessage.Enabled = !Program.flag_ReadOnly;
            tb_Name.ReadOnly = Program.flag_ReadOnly;
            tb_Description.ReadOnly = Program.flag_ReadOnly;
            lb_Progress.Enabled = !Program.flag_ReadOnly;
            dt_Begin.Enabled = !Program.flag_ReadOnly;
            dt_End.Enabled = !Program.flag_ReadOnly;
        }
        private bool saveTaskInfo(bool Force)
        {
            if (tb_Name.Text == "" || tb_Description.Text == "" || dt_Begin.Value.Ticks > dt_End.Value.Ticks)
                return false;

            if (HaveChanges == true)
            {
                DialogResult dr;
                if (Force == false && CurrentID != -1)
                    if (CurrentID == -1)
                        dr = MessageBox.Show("Добавить новую задачу с именем \"" + tb_Name.Text + "\"?", "Добавление задачи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    else
                    dr = MessageBox.Show("Вы действительно хотите сохранить изменения в задаче \"" + tb_Name.Text + "\"?", "Сохранение изменений", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                else
                    dr = DialogResult.Yes;

                if (dr == DialogResult.Yes)
                {
                    string Desc = tb_Description.Text.Replace("\r\n", "%newline%");
                    if (CurrentID == -1)
                    {
                        CurrentID = Network.Task_Add(tb_Name.Text, Desc, dt_Begin.Value.Ticks, dt_End.Value.Ticks);
                        if (CurrentID == -1)
                        {
                            MessageBox.Show("Не удалось добавить новую задачу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        showTaskInfo(CurrentID);
                    }
                    else
                    {
                        if (Network.Task_Edit(CurrentID, tb_Name.Text, Desc, dt_Begin.Value.Ticks, dt_End.Value.Ticks) == false)
                        {
                            MessageBox.Show("Не удалось обновить сведения о задаче", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            HaveChanges = false;
            return true;
        }
        private void resetTaskInfo(long id)
        {
            if (id == -1)
            {
                tb_Name.Text = "";
                dt_Begin.Value = DateTime.Now;
                dt_End.Value = DateTime.Now;
                tb_Description.Text = "";
                l_FilesInfo.Text = Language.Main_FilesInfo + "0";
                l_CoopInfo.Text = Language.Main_CoopInfo + Language.Main_CoopJustMe;
                l_PregressInfo.Text = Language.Main_ProgressInfo + "0%";
                lb_Progress.Items.Clear();
                tb_Messages.Text = "";
                tb_NewMessage.Text = "";
                b_DoTask.Enabled = false;

                b_FilesShow.Enabled = false;
                b_CoopShow.Enabled = false;
                b_AddStep.Enabled = false;
                b_EditStep.Enabled = false;
                b_DoStep.Enabled = false;
                b_SendMessage.Enabled = false;
            }
            else
            {
                showTaskInfo(id);

                b_FilesShow.Enabled = !Program.flag_ReadOnly;
                b_CoopShow.Enabled = !Program.flag_ReadOnly;
                b_AddStep.Enabled = !Program.flag_ReadOnly;
                b_EditStep.Enabled = !Program.flag_ReadOnly;
                b_DoStep.Enabled = !Program.flag_ReadOnly;
                b_SendMessage.Enabled = !Program.flag_ReadOnly;
            }
        }
    }

    struct TaskInfo
    {
        public int Height, Coop;
        public string Name, Dates;
        public long dAdd, dFinish, dStart, dEnd;
    }
}