using System;
using System.IO;
using System.Windows.Forms;

namespace Tasks.Forms
{
   partial class Main : Form
   {
      private int filterStatus = 1;
      int filterCoop = -1;
      string filterDirection = "";
      

      private void m_Main_User_Manager(object Sender, EventArgs e)
      {
         Manager manager = new Manager();
         manager.ShowDialog();
         if (manager.DialogResult == DialogResult.Yes && Network.Manager_Change(manager.SelectedUserId) == true)
         {
            Config.flag_ReadOnly = true;
            Config.user_ID = manager.SelectedUserId;
            Config.user_Name = manager.SelectedUserName;

            this.Main_Shown(Sender, e);
         }
      }
      private void m_Main_User_Back(object Sender, EventArgs e)
      {
         if (Config.flag_ReadOnly == true && Network.Manager_Change(Config.user_IDMain) == true)
         {
            Config.user_ID = Config.user_IDMain;
            Config.user_Name = Config.user_NameMain;

            this.Main_Shown(Sender, e);
            Config.flag_ReadOnly = false;

            b_AddTask.Enabled = true;
            m_Main.MenuItems[1].MenuItems[0].Enabled = true;
            m_Main.MenuItems[1].MenuItems[1].Enabled = true;
         }
      }
      private void m_Main_User_ChangePassword(object Sender, EventArgs e)
      {
         new ChangePassword(Config.user_Name).ShowDialog();
      }
      private void m_Main_User_Exit(object Sender, EventArgs e)
      {
         this.Close();
      }

      private void m_Main_Tasks_New(object Sender, EventArgs e)
      {
         b_AddTask_Click(Sender, e);
      }
      private void m_Main_Tasks_Copy(object Sender, EventArgs e)
      {
         new CopyTasks().ShowDialog();
         getTasksList();
      }

      #region Сортировка
      int sortId = 0;
      // Корень 1-3
      // 0 - по наименованию
      // 1 - по дате добавления
      // 2 - по сроку выполнения
      // 3 - по окончанию срока выполнения
      // 4 - по дате выполнения

      private void m_Main_Sort_Reset()
      {
         for (int i = 0; i < m_Main.MenuItems[1].MenuItems[3].MenuItems.Count; i++)
            m_Main.MenuItems[1].MenuItems[3].MenuItems[i].Checked = false;
      }
      private void m_Main_Sort_Name(object Sender, EventArgs e)
      {
         m_Main_Sort_Reset();
         m_Main.MenuItems[1].MenuItems[3].MenuItems[0].Checked = true;

         sortId = 0;

         Config.ConfigFile.Write("MainForm", "Sort", "0");
         Config.ConfigFile.Save();

         getTasksList();
      }
      private void m_Main_Sort_DateAdd(object Sender, EventArgs e)
      {
         m_Main_Sort_Reset();
         m_Main.MenuItems[1].MenuItems[3].MenuItems[1].Checked = true;

         sortId = 1;

         Config.ConfigFile.Write("MainForm", "Sort", "1");
         Config.ConfigFile.Save();

         getTasksList();
      }
      private void m_Main_Sort_DateStart(object Sender, EventArgs e)
      {
         m_Main_Sort_Reset();
         m_Main.MenuItems[1].MenuItems[3].MenuItems[2].Checked = true;

         sortId = 2;

         Config.ConfigFile.Write("MainForm", "Sort", "2");
         Config.ConfigFile.Save();

         getTasksList();
      }
      private void m_Main_Sort_DateEnd(object Sender, EventArgs e)
      {
         m_Main_Sort_Reset();
         m_Main.MenuItems[1].MenuItems[3].MenuItems[3].Checked = true;

         sortId = 3;

         Config.ConfigFile.Write("MainForm", "Sort", "3");
         Config.ConfigFile.Save();

         getTasksList();
      }
      #endregion
      private string m_Main_Filter_String()
      {
         string result = "Установлен фильтр ";
         switch (filterStatus)
         {
            case 0:
               result += "по всем задачам";
               break;
            case 1:
               result += "по текущим задачам";
               break;
            case 2:
               result += "по выполненным задачам";
               break;
         }
         result += "\r\n";
         if (filterB == 0 && filterE == 0)
            result += "за все время";
         else if (filterB == 0 && filterE != 0)
            result += "до " + Tools.DateToString(filterE, "DD.MM.YYYY");
         else if (filterB != 0 && filterE == 0)
            result += "с " + Tools.DateToString(filterB, "DD.MM.YYYY");
         else
            result += "с " + Tools.DateToString(filterB, "DD.MM.YYYY") + " до " + Tools.DateToString(filterE, "DD.MM.YYYY");

         return result;
      }
      #region Фильтр по дате
      private long filterB = 0, filterE = 0;
      // Корень - 1-4
      // 0 - без фильтра
      // 2 - предыдущая неделя
      // 3 - текущая неделя
      // 4 - следующая неделя
      // 6 - предыдущий месяц
      // 7 - текущий месяц
      // 8 - следующий месяц

      private void m_Main_Filter_ResetDate()
      {
         for (int i = 0; i < m_Main.MenuItems[1].MenuItems[4].MenuItems.Count; i++)
            m_Main.MenuItems[1].MenuItems[4].MenuItems[i].Checked = false;
      }
      private void m_Main_Filter_No(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetDate();
         m_Main.MenuItems[1].MenuItems[4].MenuItems[0].Checked = true;

         filterB = 0;
         filterE = 0;

         Config.ConfigFile.Write("MainForm", "FilterDate", "0");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_PrevWeek(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetDate();
         m_Main.MenuItems[1].MenuItems[4].MenuItems[2].Checked = true;

         DateTime tmp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
         tmp = tmp.AddDays(-7);
         while (tmp.DayOfWeek != DayOfWeek.Monday)
            tmp = tmp.AddDays(-1);
         filterB = tmp.Ticks;

         tmp = tmp.AddDays(7);
         filterE = tmp.Ticks - 1;

         Config.ConfigFile.Write("MainForm", "FilterDate", "2");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_CurWeek(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetDate();
         m_Main.MenuItems[1].MenuItems[4].MenuItems[3].Checked = true;

         DateTime tmp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
         while (tmp.DayOfWeek != DayOfWeek.Monday)
            tmp = tmp.AddDays(-1);
         filterB = tmp.Ticks;

         tmp = tmp.AddDays(7);
         filterE = tmp.Ticks - 1;

         Config.ConfigFile.Write("MainForm", "FilterDate", "3");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_NextWeek(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetDate();
         m_Main.MenuItems[1].MenuItems[4].MenuItems[4].Checked = true;

         DateTime tmp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
         tmp = tmp.AddDays(7);
         while (tmp.DayOfWeek != DayOfWeek.Monday)
            tmp = tmp.AddDays(-1);
         filterB = tmp.Ticks;

         tmp = tmp.AddDays(7);
         filterE = tmp.Ticks - 1;

         Config.ConfigFile.Write("MainForm", "FilterDate", "4");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_PrevMonth(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetDate();
         m_Main.MenuItems[1].MenuItems[4].MenuItems[6].Checked = true;

         DateTime tmp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
         tmp = tmp.AddMonths(-1);
         while (tmp.Day != 1)
            tmp = tmp.AddDays(-1);
         filterB = tmp.Ticks;

         tmp = tmp.AddMonths(1);
         filterE = tmp.Ticks - 1;

         Config.ConfigFile.Write("MainForm", "FilterDate", "6");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_CurMonth(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetDate();
         m_Main.MenuItems[1].MenuItems[4].MenuItems[7].Checked = true;

         DateTime tmp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
         while (tmp.Day != 1)
            tmp = tmp.AddDays(-1);
         filterB = tmp.Ticks;

         tmp = tmp.AddMonths(1);
         filterE = tmp.Ticks - 1;

         Config.ConfigFile.Write("MainForm", "FilterDate", "7");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_NextMonth(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetDate();
         m_Main.MenuItems[1].MenuItems[4].MenuItems[8].Checked = true;

         DateTime tmp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
         tmp = tmp.AddMonths(1);
         while (tmp.Day != 1)
            tmp = tmp.AddDays(-1);
         filterB = tmp.Ticks;

         tmp = tmp.AddMonths(1);
         filterE = tmp.Ticks - 1;

         Config.ConfigFile.Write("MainForm", "FilterDate", "8");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      #endregion
      #region Фильтр по статусу
      
      // Корень - 1-5
      // 0 - все
      // 1 - в работе
      // 2 - выполненные
      // 3 - удаленные

      private void m_Main_Filter_ResetStatus()
      {
         for (int i = 0; i < m_Main.MenuItems[1].MenuItems[5].MenuItems.Count; i++)
            m_Main.MenuItems[1].MenuItems[5].MenuItems[i].Checked = false;
      }
      private void m_Main_Filter_All(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetStatus();
         m_Main.MenuItems[1].MenuItems[5].MenuItems[0].Checked = true;

         filterStatus = 0;

         Config.ConfigFile.Write("MainForm", "FilterStatus", "0");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_InWork(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetStatus();
         m_Main.MenuItems[1].MenuItems[5].MenuItems[1].Checked = true;

         filterStatus = 1;

         Config.ConfigFile.Write("MainForm", "FilterStatus", "1");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_Ready(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetStatus();
         m_Main.MenuItems[1].MenuItems[5].MenuItems[2].Checked = true;

         filterStatus = 2;

         Config.ConfigFile.Write("MainForm", "FilterStatus", "2");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      private void m_Main_Filter_Deleted(object Sender, EventArgs e)
      {
         m_Main_Filter_ResetStatus();
         m_Main.MenuItems[1].MenuItems[5].MenuItems[3].Checked = true;

         filterStatus = 3;

         Config.ConfigFile.Write("MainForm", "FilterStatus", "3");
         Config.ConfigFile.Save();

         l_SortInfo.Text = m_Main_Filter_String();
         getTasksList();
      }
      #endregion
      #region Фильтр по взаимодействию
      private void m_Main_FilterCoop(object Sender, EventArgs e)
      {
         MenuItem item = (MenuItem)Sender;
         if (item.Index > 1)
            filterCoop = (int)item.Tag;
         else if (item.Index == 1)
            filterCoop = Config.user_ID;
         else
            filterCoop = -1;
      }
      #endregion
      #region Фильтр по направлению
      private void m_Main_FilterDirection(object sender, EventArgs e)
      {
         MenuItem item = (MenuItem)sender;
         Menu parent = item.Parent;
         if (item.Index > 1)
         {
            filterDirection = (string)item.Tag;
         }
         else if (item.Index == 1)
            filterDirection = "%Main%";
         else
            filterDirection = "%All%";

         for (int i = 0; i < parent.MenuItems.Count; i++)
            parent.MenuItems[i].Checked = false;
         item.Checked = true;

         getTasksList();
      }
      private void m_Main_UpdateDirections()
      {
         MenuItem item = m_Main.MenuItems[1].MenuItems[6];
         string prev = "";
         foreach (MenuItem mi in item.MenuItems)
         {
            if (mi.Checked == true)
               prev = mi.Text;
            mi.Checked = false;
         }

         while (item.MenuItems.Count > 2)
            item.MenuItems.RemoveAt(2);
         string[] list = Network.User_Directions();
         
         for (int i = 0; i < list.Length; i++)
         {
            if (list[i] == "")
               continue;
            item.MenuItems.Add("Направление: " + list[i], m_Main_FilterDirection);
            item.MenuItems[item.MenuItems.Count - 1].Tag = list[i];
         }

         bool isChecked = false;
         foreach (MenuItem mi in item.MenuItems)
         {
            if (mi.Text == prev)
            {
               mi.Checked = true;
               isChecked = true;
            }
         }

            if (isChecked == false)
         {
            item.MenuItems[0].Checked = true;
            filterDirection = "%All%";
         }
      }
      #endregion

      #region Отчеты
      private void m_Main_Report_Plan(object Sender, EventArgs e)
      {
         DateTime dt1 = DateTime.Now;
         if (dt1.Day >= 25)
            dt1 = dt1.AddMonths(1);
         dt1 = new DateTime(dt1.Year, dt1.Month, 1);
         DateTime dt2 = dt1.AddMonths(1);
         dt2 = new DateTime(dt2.Ticks - 1);

         Tools.MakePlan(dt1, dt2);

      }
      private void m_Main_Report_Report(object Sender, EventArgs e)
      {
         DateTime dt1 = DateTime.Now;
         if (dt1.Day < 6)
            dt1 = dt1.AddMonths(-1);
         dt1 = new DateTime(dt1.Year, dt1.Month, 1);
         DateTime dt2 = dt1.AddMonths(1);
         dt2 = new DateTime(dt2.Ticks - 1);

         Tools.MakeReport(dt1, dt2);
      }
      private void m_Main_Report_Manual(object Sender, EventArgs e)
      {
         new ManualReport().ShowDialog();
      }
      #endregion

      private void m_Main_Empty(object Sender, EventArgs e)
      {
         MessageBox.Show("Функция находится в стадии разработки", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      #region Administration
      private void m_Main_AdminUsers(object Sender, EventArgs e)
      {
         new Forms.Administration.Users().ShowDialog();
      }
      #endregion
   }
}