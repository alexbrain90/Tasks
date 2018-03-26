using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Tasks.Forms
{
    partial class Main:Form
    {
        private MainMenu m_Main;
        private ToolTip tt_Main;

        private GroupBox gb_Info, gb_Steps, gb_Coop, gb_Messages;

        private Label l_Worker, l_Post, l_Sbe;

        private Button b_AddTask;
        private Label l_SortInfo;
        private ListBox lb_Tasks;

        private TextBox tb_Name, tb_Description, tb_Direction;
        private Label l_Info, l_NameInfo, l_DescriptionInfo, l_Dates, l_Direction, l_Type, l_PregressInfo, l_CoopInfo;
        private ComboBox cb_Type;
        private DateTimePicker dt_Begin, dt_End;
        private ListBox lb_Progress, lb_CoopList;
        private Button b_DoTask, b_SaveTask, b_CancelTask, b_CopyTask, b_DeleteTask, b_History, b_AddStep, b_EditStep, b_DoStep, b_CoopShow;

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
            this.Font = Config.fort_Main;
            this.DoubleBuffered = true;

            tt_Main = new ToolTip();
            tt_Main.AutoPopDelay = 2500;
            tt_Main.InitialDelay = 1000;
            tt_Main.ReshowDelay = 500;
            tt_Main.ShowAlways = true;

            #region Left panel
            this.Controls.Add(l_Worker = new Label());
            l_Worker.TextChanged += Label_AutoSize;
            this.Controls.Add(l_Post = new Label());
            l_Post.Font = new Font(this.Font.ToString(), 8f);
            l_Post.TextChanged += Label_AutoSize;
            this.Controls.Add(l_Sbe = new Label());
            l_Sbe.Font = new Font(this.Font.ToString(), 8f);
            l_Sbe.TextChanged += Label_AutoSize;

            this.Controls.Add(b_AddTask = new Button());
            b_AddTask.Size = new Size(150, 25);
            b_AddTask.Text = Language.Main_ButtonAddTask;
            b_AddTask.Click += b_AddTask_Click;
            this.Controls.Add(l_SortInfo = new Label());
            l_SortInfo.TextChanged += Label_AutoSize;

            this.Controls.Add(lb_Tasks = new ListBox());
            lb_Tasks.Location = new Point(l_SortInfo.Left, l_SortInfo.Bottom + 10);
            lb_Tasks.HorizontalScrollbar = false;
            lb_Tasks.DrawMode = DrawMode.OwnerDrawVariable;
            lb_Tasks.ItemHeight = SetTasksItemHeight();
            lb_Tasks.SelectedIndexChanged += lb_Tasks_SelectedIndexChanged;
            lb_Tasks.DrawItem += Lb_Tasks_DrawItem;
            #endregion

            #region Top buttons
            this.Controls.Add(b_CancelTask = new Button());
            b_CancelTask.Size = new Size(110, 30);
            b_CancelTask.Text = "Отменить";
            b_CancelTask.Click += b_CancelTask_Click;
            this.Controls.Add(b_SaveTask = new Button());
            b_SaveTask.Size = b_CancelTask.Size;
            b_SaveTask.Text = "Сохранить";
            b_SaveTask.Click += b_SaveTask_Click;
            this.Controls.Add(b_DoTask = new Button());
            b_DoTask.Size = b_CancelTask.Size;
            b_DoTask.Text = "Выполнить";
            b_DoTask.Click += b_DoTask_Click;
            this.Controls.Add(b_CopyTask = new Button());
            b_CopyTask.Size = b_CancelTask.Size;
            b_CopyTask.Text = "Копировать";
            b_CopyTask.Click += b_CopyTask_Click;
            this.Controls.Add(b_DeleteTask = new Button());
            b_DeleteTask.Size = b_CancelTask.Size;
            b_DeleteTask.Text = "Удалить";
            b_DeleteTask.Click += b_DeleteTask_Click;
            this.Controls.Add(b_History = new Button());
            b_History.Size = b_CancelTask.Size;
            b_History.Text = "История";
            b_History.Click += b_HistoryTask_Click;

            b_CopyTask.Enabled = false;
            b_DeleteTask.Enabled = false;
            b_History.Enabled = false;
            #endregion

            #region Task info
            this.Controls.Add(gb_Info = new GroupBox());
            gb_Info.Text = "Основные сведения о задаче";

            gb_Info.Controls.Add(l_NameInfo = new Label());
            l_NameInfo.Font = l_Post.Font;
            l_NameInfo.TextChanged += Label_AutoSize;
            l_NameInfo.Text = Language.Main_NameInfo;
            gb_Info.Controls.Add(tb_Name = new TextBox());
            tb_Name.TextChanged += TaskInfo_Changed;

            gb_Info.Controls.Add(l_DescriptionInfo = new Label());
            l_DescriptionInfo.Font = l_Post.Font;
            l_DescriptionInfo.TextChanged += Label_AutoSize;
            l_DescriptionInfo.Text = Language.Main_DescriptionInfo;
            l_DescriptionInfo.Height = 14;
            gb_Info.Controls.Add(tb_Description = new TextBox());
            tb_Description.Font = new Font(tb_Name.Font.FontFamily, 10f);
            tb_Description.Multiline = true;
            tb_Description.ScrollBars = ScrollBars.Vertical;
            tb_Description.TextChanged += TaskInfo_Changed;

            gb_Info.Controls.Add(l_Dates = new Label());
            l_Dates.Font = l_Post.Font;
            l_Dates.TextChanged += Label_AutoSize;
            l_Dates.Text = Language.Main_DatesInfo;
            gb_Info.Controls.Add(dt_Begin = new DateTimePicker());
            dt_Begin.ValueChanged += TaskInfo_Changed;
            gb_Info.Controls.Add(dt_End = new DateTimePicker());
            dt_End.ValueChanged += TaskInfo_Changed;

            gb_Info.Controls.Add(l_Direction = new Label());
            l_Direction.Font = l_Post.Font;
            l_Direction.TextChanged += Label_AutoSize;
            l_Direction.Text = "Направление";
            gb_Info.Controls.Add(tb_Direction = new TextBox());
            tb_Direction.TextChanged += TaskInfo_Changed;

            gb_Info.Controls.Add(l_Type = new Label());
            l_Type.Font = l_Post.Font;
            l_Type.TextChanged += Label_AutoSize;
            l_Type.Text = "Тип задачи";
            gb_Info.Controls.Add(cb_Type = new ComboBox());
            cb_Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_Type.Items.Add("Основная");
            cb_Type.Items.Add("Контроль");
            cb_Type.SelectedIndexChanged += TaskInfo_Changed;

            gb_Info.Controls.Add(l_Info = new Label());
            l_Info.Font = l_Post.Font;
            l_Info.TextChanged += Label_AutoSize;
            l_Info.Text = "";
            #endregion

            #region Cooperation
            this.Controls.Add(gb_Coop = new GroupBox());
            gb_Coop.Text = "Совместная работа";

            gb_Coop.Controls.Add(l_CoopInfo = new Label());
            l_CoopInfo.Font = new Font(this.Font.FontFamily, 8f);
            l_CoopInfo.Text = "Список связанным сотрудников";
            l_CoopInfo.TextChanged += Label_AutoSize;
            gb_Coop.Controls.Add(lb_CoopList = new ListBox());
            lb_CoopList.HorizontalScrollbar = false;
            lb_CoopList.DrawMode = DrawMode.OwnerDrawVariable;
            lb_CoopList.ItemHeight = SetCoopsItemHeight();
            lb_CoopList.DrawItem += Lb_CoopList_DrawItem;   
            gb_Coop.Controls.Add(b_CoopShow = new Button());
            b_CoopShow.Size = new Size(100, 25);
            b_CoopShow.Text = "Изменить";
            b_CoopShow.Click += b_CoopShow_Click;
            #endregion

            #region Steps
            this.Controls.Add(gb_Steps = new GroupBox());
            gb_Steps.Text = "Шаги";

            gb_Steps.Controls.Add(l_PregressInfo = new Label());
            l_PregressInfo.Text = Language.Main_ProgressInfo;
            gb_Steps.Controls.Add(lb_Progress = new ListBox());
            lb_Progress.ItemHeight = SetStepsItemHeight();
            lb_Progress.DrawMode = DrawMode.OwnerDrawVariable;
            lb_Progress.DrawItem += lb_Progress_DrawItem;
            gb_Steps.Controls.Add(b_AddStep = new Button());
            b_AddStep.Size = new Size(32, 32);
            b_AddStep.Image = Tasks.Properties.Resources.button_Add;
            b_AddStep.Click += b_AddStep_Click;
            gb_Steps.Controls.Add(b_EditStep = new Button());
            b_EditStep.Size = b_AddStep.Size;
            b_EditStep.Image = Tasks.Properties.Resources.button_Edit;
            b_EditStep.Click += b_EditStep_Click;
            gb_Steps.Controls.Add(b_DoStep = new Button());
            b_DoStep.Size = b_EditStep.Size;
            b_DoStep.Image = Tasks.Properties.Resources.button_Do;
            b_DoStep.Click += b_DoStep_Click;
            #endregion

            #region Messages
            this.Controls.Add(gb_Messages = new GroupBox());
            gb_Messages.Text = "Комментарии";

            gb_Messages.Controls.Add(tb_Messages = new TextBox());
            tb_Messages.Multiline = true;
            tb_Messages.ScrollBars = ScrollBars.Vertical;
            tb_Messages.ReadOnly = true;
            gb_Messages.Controls.Add(tb_NewMessage = new TextBox());
            tb_NewMessage.Height = 60;
            tb_NewMessage.Multiline = true;
            tb_NewMessage.ScrollBars = ScrollBars.Vertical;
            gb_Messages.Controls.Add(b_SendMessage = new Button());
            b_SendMessage.Size = new Size(64, 64);
            b_SendMessage.Text = "Отправить";
            b_SendMessage.Click += B_SendMessage_Click;
            #endregion

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
            m_Main.MenuItems[1].MenuItems[5].MenuItems.Add("Удаленные", m_Main_Filter_Deleted);
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
                this.Left = Convert.ToInt32(Config.ConfigFile.Read("MainForm", "LocationX"));
                this.Top = Convert.ToInt32(Config.ConfigFile.Read("MainForm", "LocationY"));
                this.Width = Convert.ToInt32(Config.ConfigFile.Read("MainForm", "SizeX"));
                this.Height = Convert.ToInt32(Config.ConfigFile.Read("MainForm", "SizeY"));
                if (Config.ConfigFile.Read("MainForm", "Maximized") == "true")
                    this.WindowState = FormWindowState.Maximized;
                else
                    this.WindowState = FormWindowState.Normal;
            }
            catch
            {
                Config.ConfigFile.Write("MainForm", "Maximized", "false");
                Config.ConfigFile.Write("MainForm", "LocationX", this.Left.ToString());
                Config.ConfigFile.Write("MainForm", "LocationY", this.Top.ToString());
                Config.ConfigFile.Write("MainForm", "SizeX", this.Width.ToString());
                Config.ConfigFile.Write("MainForm", "SizeY", this.Height.ToString());
            }
            this.Main_Resize(this, null);
            setElementsEnable(0);

            t_UpdateTasks = new System.Windows.Forms.Timer();
            t_UpdateTasks.Interval = 15000;
            t_UpdateTasks.Tick += t_UpdateTasks_Tick;
        }

        private void TaskInfo_Changed(object sender, EventArgs e)
        {
            HaveChanges = true;
            b_SaveTask.Enabled = true;
            b_CancelTask.Enabled = true;
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
            if (TasksID[e.Index].Direction != "")
                line2 = "[" + TasksID[e.Index].Direction + "]   " + line2;
            
            Font font2 = new Font(e.Font.FontFamily, e.Font.Size - 2);
            Size size2 = e.Graphics.MeasureString(line2, font2).ToSize();

            Color color = Color.White;
            if (e.BackColor == lb_Tasks.BackColor)
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
                if (TasksID[e.Index].Deleted == true)
                    color = Color.FromArgb(255, 0, 0);
            }

            if (TasksID[e.Index].Deleted == true)
            {
                e.Graphics.DrawImage(Properties.Resources.task_Deleted, e.Bounds.X, e.Bounds.Y);
                font1 = new Font(font1, FontStyle.Strikeout);
            }
            else if(TasksID[e.Index].dStart > DateTime.Now.Ticks)
                e.Graphics.DrawImage(Properties.Resources.task_Future, e.Bounds.X, e.Bounds.Y);
            else if (TasksID[e.Index].dFinish != 0)
                e.Graphics.DrawImage(Properties.Resources.task_Ready, e.Bounds.X, e.Bounds.Y);
            else if (TasksID[e.Index].dEnd < DateTime.Now.AddDays(2).Ticks && TasksID[e.Index].dEnd > DateTime.Now.Ticks)
                e.Graphics.DrawImage(Properties.Resources.task_Near, e.Bounds.X, e.Bounds.Y);
            else if (TasksID[e.Index].dEnd < DateTime.Now.Ticks)
                e.Graphics.DrawImage(Properties.Resources.task_Miss, e.Bounds.X, e.Bounds.Y);

            else
                e.Graphics.DrawImage(Properties.Resources.task_Normal, e.Bounds.X, e.Bounds.Y);

            int x = e.Bounds.X;
            if (TasksID[e.Index].Coop != 1)
            {
                e.Graphics.DrawImage(Properties.Resources.task_Coop, x, e.Bounds.Y + 33);
                x += 25;
            }
            if (TasksID[e.Index].Type == 1)
            {
                e.Graphics.DrawImage(Properties.Resources.task_Manage, x, e.Bounds.Y + 33);
                x += 25;
            }

            if (TasksID[e.Index].Events == true)
            {
                font1 = new Font(font1, FontStyle.Bold);
                font2 = new Font(font2, FontStyle.Bold);
            }

            e.Graphics.DrawRectangle(new Pen(Color.LightGray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);

            Rectangle bounds = new Rectangle(e.Bounds.X + 27, e.Bounds.Y, e.Bounds.Width - 27, size1.Height);
            e.Graphics.DrawString(line1, font1, new SolidBrush(color), bounds, StringFormat.GenericDefault);
            bounds = new Rectangle(x, bounds.Y + size1.Height, bounds.Width, size2.Height);
            e.Graphics.DrawString(line2, font2, new SolidBrush(color), bounds, StringFormat.GenericDefault);

            e.DrawFocusRectangle();
        }
        private void Lb_CoopList_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index == -1)
                return;

            e.Graphics.DrawRectangle(new Pen(Color.LightGray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);

            e.Graphics.DrawString(lb_CoopList.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, StringFormat.GenericDefault);

            e.DrawFocusRectangle();
        }
        private void lb_Progress_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index == -1)
                return;

            Brush brush;
            Font font;
            if (StepsIDMark[e.Index] == true)
            {
                if (e.BackColor != lb_Progress.BackColor)
                    brush = Brushes.LightGray;
                else
                    brush = Brushes.DimGray;

                font = new Font(e.Font, FontStyle.Strikeout);
            }
            else
            {
                if (e.BackColor != lb_Progress.BackColor)
                    brush = Brushes.White;
                else
                    brush = Brushes.Black;
                font = e.Font;
            }

            e.Graphics.DrawRectangle(new Pen(Color.LightGray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);

            // Draw first line with name
            /*string text = lb_Progress.Items[e.Index].ToString();
            Rectangle bounds = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, (int)(e.Graphics.MeasureString(text,e.Font).Height + 1));
            e.Graphics.DrawString(text, e.Font, brush, bounds, StringFormat.GenericDefault);

            bounds = new Rectangle(e.Bounds.Left, e.Bounds.Top + e.Bounds.Height / 2, e.Bounds.Width, e.Bounds.Height / 2);
            text = "Добавлено: " + Tools.DateTimeToString(StepsID[e.Index], "DD.MM.YYYY  hh:mm");
            e.Graphics.DrawString(text, new Font(e.Font.FontFamily, e.Font.Size / 3 * 2), brush, bounds, StringFormat.GenericDefault);

            e.Graphics.DrawLine(new Pen(Color.LightGray), e.Bounds.Left + 1, e.Bounds.Bottom - 1, e.Bounds.Right - 1, e.Bounds.Bottom - 1);*/

            e.Graphics.DrawString(lb_Progress.Items[e.Index].ToString(), font, brush, e.Bounds, StringFormat.GenericDefault);

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

            tb_Name.Enabled = true;
            tb_Description.Enabled = true;
            dt_Begin.Enabled = true;
            dt_End.Enabled = true;
            cb_Type.Enabled = true;
            tb_Direction.Enabled = true;
        }
        private void b_DoTask_Click(object sender, EventArgs e)
        {
            if (b_DoTask.Text == "Выполнить")
            {
                DialogResult dr = MessageBox.Show("Вы действительно хотите пометить задачу \"" + (string)lb_Tasks.SelectedItem + "\" как выполненную?", "Выполнение задачи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (Network.Task_Do(CurrentID) == false)
                    {
                        MessageBox.Show("Ошибка при установки отметки выполнения задачи " + (string)lb_Progress.SelectedItem, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                DialogResult dr = MessageBox.Show("Вы действительно хотите снять отметку выполнения для задачи \"" + (string)lb_Tasks.SelectedItem + "\"?", "Выполнение задачи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (Network.Task_UnDo(CurrentID) == false)
                    {
                        MessageBox.Show("Ошибка при удалении отметки выполнения задачи " + (string)lb_Progress.SelectedItem, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            showTaskInfo(CurrentID);
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
            if (CurrentID != -1)
                saveTaskInfo(false);

            resetTaskInfo(CurrentID);

            b_SaveTask.Enabled = false;
            b_CancelTask.Enabled = false;
        }
        private void b_DeleteTask_Click(object sender, EventArgs e)
        {
            if (b_DeleteTask.Text == "Удалить")
            {
                DialogResult dr = MessageBox.Show("Вы действительно хотите удалить задачу \"" + (string)lb_Tasks.SelectedItem + "\" как выполненную?", "Удаление задачи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (Network.Task_Delete(CurrentID) == false)
                    {
                        MessageBox.Show("Ошибка при удалении задачи " + (string)lb_Progress.SelectedItem, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                DialogResult dr = MessageBox.Show("Вы действительно хотите восстановить задачу \"" + (string)lb_Tasks.SelectedItem + "\"?", "Восстановление задачи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (Network.Task_UnDelete(CurrentID) == false)
                    {
                        MessageBox.Show("Ошибка при восстановлении задачи " + (string)lb_Progress.SelectedItem, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            showTaskInfo(CurrentID);
            getTasksList();
        }
        private void b_CopyTask_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Создать копию текущей задачи: " + tb_Name.Text + "?", "Копирование задачи", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string[] info = Network.Task_Info(CurrentID);
                string[,] steps = Network.Step_List(CurrentID);
                string[,] coop = Network.Coop_List(CurrentID);

                long newID = Network.Task_Add(info[0], info[1], dt_Begin.Value.Ticks, dt_End.Value.Ticks, Convert.ToInt32(info[6]), info[7]);
                for (int i = 0; i < steps.Length / 3; i++)
                    Network.Step_Add(newID, steps[i, 0]);
                for (int i = 0; i < coop.Length / 2; i++)
                    if (coop[i, 0] != Config.user_IDMain.ToString())
                        Network.Coop_Add(newID, Convert.ToInt32(coop[i, 0]));

                showTaskInfo(newID);
                getTasksList();
            }
        }
        private void b_HistoryTask_Click(object sender, EventArgs e)
        {
            MessageBox.Show("В разработке");
        }

        private void l_CoopInfo_TextChanged(object sender, EventArgs e)
        {
            l_CoopInfo.Width = (int)this.CreateGraphics().MeasureString(l_CoopInfo.Text, l_CoopInfo.Font).Width + 15;
            b_CoopShow.Location = new Point(l_CoopInfo.Right + 10, l_CoopInfo.Top);
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            // Crear everything
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);
            e.Graphics.DrawRectangle(new Pen(Color.DarkGray), 5, 5, this.ClientSize.Width - 10, this.ClientSize.Height - 10);
            // Draw worker info
            e.Graphics.DrawRectangle(new Pen(Color.DarkGray), 5, 5, l_Worker.Width + 10, b_AddTask.Bottom - l_Worker.Top + 10);
            // Draw tasks list
            e.Graphics.DrawRectangle(new Pen(Color.DarkGray), 5, l_SortInfo.Top - 5, l_SortInfo.Right - l_SortInfo.Left + 10, this.ClientSize.Height - l_SortInfo.Top);
            // Draw top buttons
            e.Graphics.DrawRectangle(new Pen(Color.DarkGray), b_CancelTask.Left - 5, b_CancelTask.Top - 5, this.ClientSize.Width - b_CancelTask.Left, b_CancelTask.Height + 10);
            e.Graphics.DrawLine(new Pen(Color.LightGray), b_CancelTask.Right + 10, b_CancelTask.Top + 5, b_CancelTask.Right + 10, b_CancelTask.Bottom - 5);
            e.Graphics.DrawLine(new Pen(Color.LightGray), b_CopyTask.Right + 10, b_CopyTask.Top + 5, b_CopyTask.Right + 10, b_CopyTask.Bottom - 5);
            e.Graphics.DrawLine(new Pen(Color.LightGray), b_DeleteTask.Right + 10, b_DeleteTask.Top + 5, b_DeleteTask.Right + 10, b_DeleteTask.Bottom - 5);


        }
        private void Main_Resize(object sender, EventArgs e)
        {
            int w = this.ClientSize.Width / 4;

            #region Left panel
            l_Worker.Left = 10; l_Worker.Top = 10;
            l_Worker.Width = w; Label_AutoSize(l_Worker, null);
            l_Post.Left = l_Worker.Left; l_Post.Top = l_Worker.Bottom + 4;
            l_Post.Width = w; Label_AutoSize(l_Post, null);
            l_Sbe.Left = l_Post.Left; l_Sbe.Top = l_Post.Bottom + 4;
            l_Sbe.Width = w; Label_AutoSize(l_Sbe, null);

            b_AddTask.Left = l_Sbe.Left + (w - b_AddTask.Width) / 2; b_AddTask.Top = l_Sbe.Bottom + 10;
            l_SortInfo.Left = l_Sbe.Left; l_SortInfo.Top = b_AddTask.Bottom + 10;
            l_SortInfo.Width = w; Label_AutoSize(l_SortInfo, null);

            lb_Tasks.Left = l_SortInfo.Left; lb_Tasks.Top = l_SortInfo.Bottom + 10;
            lb_Tasks.Width = w; lb_Tasks.Height = this.ClientSize.Height - lb_Tasks.Top - 10;
            #endregion
            #region Top buttons
            b_CancelTask.Left = lb_Tasks.Right + 10; b_CancelTask.Top = l_Worker.Top;
            b_CancelTask.Width = 110; b_CancelTask.Height = 35;

            b_SaveTask.Left = b_CancelTask.Right + 20; b_SaveTask.Top = b_CancelTask.Top;
            b_SaveTask.Size = b_CancelTask.Size;
            b_DoTask.Left = b_SaveTask.Right + 10; b_DoTask.Top = b_SaveTask.Top;
            b_DoTask.Size = b_CancelTask.Size;
            b_CopyTask.Left = b_DoTask.Right + 10; b_CopyTask.Top = b_SaveTask.Top;
            b_CopyTask.Size = b_CancelTask.Size;

            b_DeleteTask.Left = b_CopyTask.Right + 20; b_DeleteTask.Top = b_SaveTask.Top;
            b_DeleteTask.Size = b_CancelTask.Size;

            b_History.Left = b_DeleteTask.Right + 20; b_History.Top = b_SaveTask.Top;
            b_History.Size = b_CancelTask.Size;
            #endregion
            #region Task info
            gb_Info.Left = b_CancelTask.Left; gb_Info.Top = b_CancelTask.Bottom + 10;
            gb_Info.Width = this.ClientSize.Width - gb_Info.Left - 10; gb_Info.Height = (int)((this.ClientSize.Height - b_CancelTask.Bottom - 20) * 0.5);

            l_NameInfo.Left = 10; l_NameInfo.Top = 20;
            l_NameInfo.Width = w * 2; Label_AutoSize(l_NameInfo, null);
            tb_Name.Left = l_NameInfo.Left; tb_Name.Top = l_NameInfo.Bottom;
            tb_Name.Width = l_NameInfo.Width;

            l_DescriptionInfo.Left = l_NameInfo.Left; l_DescriptionInfo.Top = tb_Name.Bottom + 10;
            l_DescriptionInfo.Width = l_NameInfo.Width; Label_AutoSize(l_DescriptionInfo, null);
            tb_Description.Left = l_DescriptionInfo.Left; tb_Description.Top = l_DescriptionInfo.Bottom;
            tb_Description.Width = l_DescriptionInfo.Width; tb_Description.Height = gb_Info.Height - tb_Description.Top - 10;

            l_Dates.Left = l_NameInfo.Right + 20; l_Dates.Top = l_NameInfo.Top;
            l_Dates.Width = gb_Info.Width - l_Dates.Left - 10; Label_AutoSize(l_Dates, null);
            dt_Begin.Left = l_Dates.Left; dt_Begin.Top = l_Dates.Bottom;
            dt_Begin.Width = l_Dates.Width;
            dt_End.Left = dt_Begin.Left; dt_End.Top = dt_Begin.Bottom + 4;
            dt_End.Width = dt_Begin.Width;

            l_Type.Left = l_Dates.Left; l_Type.Top = dt_End.Bottom + 10;
            l_Type.Width = l_Dates.Width; Label_AutoSize(l_Type, null);
            cb_Type.Left = l_Type.Left; cb_Type.Top = l_Type.Bottom;
            cb_Type.Width = l_Type.Width;

            l_Direction.Left = l_Type.Left; l_Direction.Top = cb_Type.Bottom + 10;
            l_Direction.Width = l_Type.Width; Label_AutoSize(l_Direction, null);
            tb_Direction.Left = l_Direction.Left; tb_Direction.Top = l_Direction.Bottom;
            tb_Direction.Width = l_Direction.Width;

            l_Info.Left = l_Direction.Left; l_Info.Top = tb_Direction.Bottom + 20;
            l_Info.Width = l_Direction.Width; l_Info.Height = gb_Info.ClientSize.Height - l_Info.Top - 10;
            #endregion
            #region Steps
            gb_Steps.Left = gb_Info.Left; gb_Steps.Top = gb_Info.Bottom + 10;
            gb_Steps.Width = gb_Info.Width / 5 * 2; gb_Steps.Height = this.ClientSize.Height - gb_Steps.Top - 10;

            l_PregressInfo.Left = 10; l_PregressInfo.Top = 20;
            l_PregressInfo.Width = gb_Steps.Width - 20; Label_AutoSize(l_PregressInfo, null);
            b_AddStep.Left = l_PregressInfo.Left; b_AddStep.Top = l_PregressInfo.Bottom + 10;
            b_EditStep.Left = b_AddStep.Left; b_EditStep.Top = b_AddStep.Bottom + 10;
            b_DoStep.Left = b_EditStep.Left; b_DoStep.Top = b_EditStep.Bottom + 10;

            lb_Progress.Left = b_AddStep.Right + 10; lb_Progress.Top = b_AddStep.Top;
            lb_Progress.Width = gb_Steps.Width - lb_Progress.Left - 10; lb_Progress.Height = gb_Steps.Height - lb_Progress.Top - 10;
            #endregion
            #region Cooperation
            gb_Coop.Left = gb_Steps.Right + 10; gb_Coop.Top = gb_Steps.Top;
            gb_Coop.Width = (this.ClientSize.Width - gb_Coop.Left - 20) / 3; gb_Coop.Height = gb_Steps.Height;

            l_CoopInfo.Left = 10; l_CoopInfo.Top = 20;
            l_CoopInfo.Width = gb_Coop.Width - 20; Label_AutoSize(l_CoopInfo, null);
            b_CoopShow.Left = gb_Coop.Width - b_CoopShow.Width - 10; b_CoopShow.Top = gb_Coop.Height - b_CoopShow.Height - 10;
            lb_CoopList.Left = l_CoopInfo.Left; lb_CoopList.Top = l_CoopInfo.Bottom;
            lb_CoopList.Width = l_CoopInfo.Width;lb_CoopList.Height = b_CoopShow.Top - lb_CoopList.Top - 10;
            #endregion
            #region Messages
            gb_Messages.Left = gb_Coop.Right + 10; gb_Messages.Top = gb_Coop.Top;
            gb_Messages.Width = this.ClientSize.Width - gb_Messages.Left - 10; gb_Messages.Height = gb_Coop.Height;

            b_SendMessage.Left = gb_Messages.Width - b_SendMessage.Width - 10; b_SendMessage.Top = gb_Messages.Height - b_SendMessage.Height - 10;
            tb_NewMessage.Left = 10; tb_NewMessage.Top = b_SendMessage.Top;
            tb_NewMessage.Width = b_SendMessage.Left - 20; tb_NewMessage.Height = b_SendMessage.Height;
            tb_Messages.Left = tb_NewMessage.Left; tb_Messages.Top = 20;
            tb_Messages.Width = gb_Messages.Width - 20; tb_Messages.Height = tb_NewMessage.Top - 30;
            #endregion

            this.OnPaint(new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
            lb_Progress.Refresh();
            lb_Tasks.Refresh();
            lb_CoopList.Refresh();

            Config.form_Main_Maximized = this.WindowState == FormWindowState.Maximized ? true : false;
        }
        private void Main_Shown(object sender, EventArgs e)
        {
            if (Network.User_Info() == true)
            {
                this.Text = Config.user_Fio + " - " + Config.user_Post + " - " + Config.user_Sbe;
                l_Worker.Text = Config.user_Name;
                l_Post.Text = Config.user_Post;
                l_Sbe.Text = Config.user_Sbe;
            }

            b_AddTask.Enabled = !Config.flag_ReadOnly;
            m_Main.MenuItems[1].MenuItems[0].Enabled = !Config.flag_ReadOnly;
            m_Main.MenuItems[1].MenuItems[1].Enabled = !Config.flag_ReadOnly;

            //getTasksList();

            t_UpdateTasks.Enabled = true;

            switch(Config.ConfigFile.Read("MainForm", "Sort", "0"))
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
            switch (Config.ConfigFile.Read("MainForm", "FilterDate", "0"))
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
            switch (Config.ConfigFile.Read("MainForm", "FilterStatus", "1"))
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

            Main_Resize(this, null);
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            t_UpdateTasks.Enabled = false;

            if (this.WindowState == FormWindowState.Maximized)
                Config.ConfigFile.Write("MainForm", "Maximized", "true");
            else if (this.WindowState == FormWindowState.Normal)
            {
                Config.ConfigFile.Write("MainForm", "Maximized", "false");
                Config.ConfigFile.Write("MainForm", "LocationX", this.Left.ToString());
                Config.ConfigFile.Write("MainForm", "LocationY", this.Top.ToString());
                Config.ConfigFile.Write("MainForm", "SizeX", this.Width.ToString());
                Config.ConfigFile.Write("MainForm", "SizeY", this.Height.ToString());
            }

            Config.ConfigFile.Save();
        }
        private void Label_AutoSize(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            Graphics g = this.CreateGraphics();
            label.Height = (int)g.MeasureString(label.Text, label.Font, label.Width).Height + 2;
        }
        private int SetTasksItemHeight()
        {
            int result = 0;
            Graphics g = this.CreateGraphics();
            result += (int)g.MeasureString("Yy\r\nYy", lb_Tasks.Font).Height + 2;
            result += (int)g.MeasureString("Yy", new Font(lb_Tasks.Font.FontFamily, 8)).Height + 2;
            return result;
        }
        private int SetStepsItemHeight()
        {
            int result = 0;
            Graphics g = this.CreateGraphics();
            result += (int)g.MeasureString("Yy\r\nYy", lb_Progress.Font).Height + 2;
            return result;
        }
        private int SetCoopsItemHeight()
        {
            int result = 0;
            Graphics g = this.CreateGraphics();
            result += (int)g.MeasureString("Yy", lb_CoopList.Font).Height + 2;
            return result;
        }

        private void lb_Tasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flag_NoTaskSelect == true)
                return;

            if (lb_Tasks.SelectedIndex == -1)
            {
                resetTaskInfo(-1);
                return;
            }

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
            TasksID = new TaskInfo[list.Length / 10];

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
                TasksID[i].Type = Convert.ToInt32(list[i, 6]);
                TasksID[i].Direction = list[i, 7];
                TasksID[i].Deleted = list[i, 8] == "1" ? true : false;
                TasksID[i].Events = list[i, 9] == "1" ? true : false;
                if (TasksID[i].dAdd == prevID)
                    n = i;

                TasksID[i].Dates = Tools.DateIntervalToString(TasksID[i].dStart, TasksID[i].dEnd);
                if (TasksID[i].Dates == "")
                    TasksID[i].Dates = Tools.DateTimeToString(TasksID[i].dStart, "DD.MM.YYYY") + " - " + Tools.DateTimeToString(TasksID[i].dEnd, "DD.MM.YYYY");

                DateTime dt = new DateTime(Convert.ToInt64(list[i, 2]));
                lb_Tasks.Items[i] = (object)list[i, 0];
                if (TasksID[i].dAdd == CurrentID)
                    lb_Tasks.SelectedIndex = i;

                if (Config.user_ID != Config.user_IDMain)
                    TasksID[i].Events = false;
            }
            lb_Tasks.EndUpdate();
            lb_Tasks.ClearSelected();

            if (n != -1)
                lb_Tasks.SelectedIndex = n;

            flag_NoTaskSelect = false;
        }
        private void showTaskInfo(long id)
        {
            setElementsEnable(0);

            string[] list = Network.Task_Info(id);
            if (list.Length == 0)
                return;
            #region Task info
            tb_Name.Text = list[0];
            tb_Description.Text = list[1].Replace("%newline%", "\r\n");
            dt_Begin.Value = new DateTime(Convert.ToInt64(list[2]));
            dt_End.Value = new DateTime(Convert.ToInt64(list[3]));
            cb_Type.SelectedIndex = Convert.ToInt32(list[6]);
            tb_Direction.Text = list[7];
            DateTime dtInfo = new DateTime(Convert.ToInt64(list[10]));
            list[9] = Tools.FioToShort(list[9]); list[11] = Tools.FioToShort(list[11]);
            l_Info.Text = "Автор:\r\n" + list[9] + " (" + dtInfo.Day.ToString("00") + "." + dtInfo.Month.ToString("00") + "." + dtInfo.Year.ToString("0000") + " - " + dtInfo.Hour.ToString("00") + ":" + dtInfo.Minute.ToString("00") + ")";
            l_Info.Text += "\r\n\r\n";
            dtInfo = new DateTime(Convert.ToInt64(list[12]));
            l_Info.Text+= "Последнее изменение:\r\n" + list[11] + " (" + dtInfo.Day.ToString("00") + "." + dtInfo.Month.ToString("00") + "." + dtInfo.Year.ToString("0000") + " - " + dtInfo.Hour.ToString("00") + ":" + dtInfo.Minute.ToString("00") + ")";
            #endregion
            #region Cooperation
            lb_CoopList.BeginUpdate();
            lb_CoopList.Items.Clear();
            if (list[4] == "1")
                lb_CoopList.Items.Add(Config.user_Name);
            else
            {
                string[,] coops = Network.Coop_List(id);
                for (int i = 0; i < coops.Length / 2; i++)
                    lb_CoopList.Items.Add(coops[i, 1]);
            }
            lb_CoopList.EndUpdate();
            #endregion
            #region Steps
            string[,] steps = Network.Step_List(id);
            lb_Progress.BeginUpdate();
            lb_Progress.Items.Clear();
            StepsID = new long[steps.Length / 3];
            StepsIDMark = new bool[StepsID.Length];

            int n = 0;
            for (int i = 0; i < StepsID.Length; i++)
            {
                StepsID[i] = Convert.ToInt64(steps[i, 2]);
                string temp = steps[i, 0];
                if (steps[i, 1] != "")
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

                if (p == 100)
                    b_DoTask.Enabled = true;
                else
                    b_DoTask.Enabled = false;
            }
            else
            {
                l_PregressInfo.Text = "";
                b_EditStep.Enabled = false;
                b_DoStep.Enabled = false;

                b_DoTask.Enabled = true;
            }
            #endregion
            #region Messages
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
            #endregion

            CurrentID = id;

            HaveChanges = false;
            b_SaveTask.Enabled = false;
            b_CancelTask.Enabled = false;

            if (Config.user_ID != Config.user_IDMain)
                setElementsEnable(-3);
            else
            {
                setElementsEnable(2);
                if (list[5] == "0")
                    b_DoTask.Text = "Выполнить";
                else
                {
                    b_DoTask.Text = "В работу";
                    setElementsEnable(-2);
                }
                if (list[8] == "0")
                    b_DeleteTask.Text = "Удалить";
                else
                {
                    b_DeleteTask.Text = "Восстановить";
                    setElementsEnable(-1);
                }

                Network.Event_Delete(id);
                if (lb_Tasks.SelectedIndex != -1)
                {
                    TasksID[lb_Tasks.SelectedIndex].Events = false;
                    lb_Tasks.Refresh();
                }
            }
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
                        dr = MessageBox.Show("Сохранить изменения в задаче \"" + tb_Name.Text + "\"?", "Сохранение изменений", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                else
                    dr = DialogResult.Yes;

                if (dr == DialogResult.Yes)
                {
                    string Desc = tb_Description.Text.Replace("\r\n", "%newline%");
                    if (CurrentID == -1)
                    {
                        CurrentID = Network.Task_Add(tb_Name.Text, Desc, dt_Begin.Value.Ticks, dt_End.Value.Ticks, cb_Type.SelectedIndex, tb_Direction.Text);
                        if (CurrentID == -1)
                        {
                            MessageBox.Show("Не удалось добавить новую задачу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        showTaskInfo(CurrentID);
                    }
                    else
                    {
                        if (Network.Task_Edit(CurrentID, tb_Name.Text, Desc, dt_Begin.Value.Ticks, dt_End.Value.Ticks, cb_Type.SelectedIndex, tb_Direction.Text) == false)
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
            tb_Name.Text = "";
            tb_Description.Text = "";
            dt_Begin.Value = DateTime.Now;
            dt_End.Value = DateTime.Now;
            cb_Type.SelectedIndex = 0;
            tb_Direction.Text = "";

            lb_Progress.Items.Clear();
            lb_CoopList.Items.Clear();

            tb_Messages.Text = "";
            tb_NewMessage.Text = "";

            setElementsEnable(1);

            if (id != -1)
                showTaskInfo(id);
        }
        private void setElementsEnable(int value)
        {
            b_CancelTask.Enabled = false;
            b_SaveTask.Enabled = false;
            b_DoTask.Enabled = false;
            b_CopyTask.Enabled = false;
            b_DeleteTask.Enabled = false;
            b_History.Enabled = false;

            tb_Name.Enabled = false;
            tb_Description.Enabled = false;
            dt_Begin.Enabled = false;
            dt_End.Enabled = false;
            cb_Type.Enabled = false;
            tb_Direction.Enabled = false;

            b_AddStep.Enabled = false;
            b_EditStep.Enabled = false;
            b_DoStep.Enabled = false;
            lb_Progress.Enabled = false;

            lb_CoopList.Enabled = false;
            b_CoopShow.Enabled = false;

            tb_Messages.Enabled = false;
            tb_NewMessage.Enabled = false;
            b_SendMessage.Enabled = false;
            
            if (value == -1)
                b_DeleteTask.Enabled = true;
            else if (value == -2)
                b_DoTask.Enabled = true;
            if (value < 0)
            {
                lb_Progress.Enabled = true;
                lb_CoopList.Enabled = true;
            }


            if (value >= 1)
            {
                b_CancelTask.Enabled = true;
                b_SaveTask.Enabled = true;

                tb_Name.Enabled = true;
                tb_Description.Enabled = true;
                dt_Begin.Enabled = true;
                dt_End.Enabled = true;
                cb_Type.Enabled = true;
                tb_Direction.Enabled = true;
            }
            if (value >= 2)
            {
                b_DoTask.Enabled = true;
                b_CopyTask.Enabled = true;
                b_DeleteTask.Enabled = true;
                b_History.Enabled = true;

                b_AddStep.Enabled = true;
                b_EditStep.Enabled = true;
                b_DoStep.Enabled = true;
                lb_Progress.Enabled = true;

                lb_CoopList.Enabled = true;
                b_CoopShow.Enabled = true;

                tb_Messages.Enabled = true;
                tb_NewMessage.Enabled = true;
                b_SendMessage.Enabled = true;
            }
        }
    }

    struct TaskInfo
    {
        public int Coop, Type;
        public string Name, Dates, Direction;
        public long dAdd, dFinish, dStart, dEnd;
        public bool Deleted, Events;
    }
}