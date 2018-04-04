using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Tasks.Update
{
   class Progress : Form
   {
      // Steps
      // 0 - Prepare
      // 1 - Download new app
      // 2 - Run update app
      // 3 - Close previous app
      // 4 - Update
      // 5 - Dowload dlls
      // 6 - Run new app
      private int CurrentStep = 0;
      private Thread MainThread;

      private ListBox lb_Steps;

      public Progress(int Step)
      {
         CurrentStep = Step;
         this.Text = "Обновление приложения";
         this.StartPosition = FormStartPosition.CenterScreen;
         this.Icon = Properties.Resources.icon_Main;
         this.ShowInTaskbar = false;
         this.FormBorderStyle = FormBorderStyle.FixedSingle;
         this.MaximizeBox = false; this.MinimizeBox = false;
         this.ClientSize = new Size(300, 300);
         this.FormClosing += UpdateProgress_FormClosing;
         this.Shown += UpdateProgress_Shown;
         this.Font = Config.font_Main;

         MainThread = new Thread(ThreadFunc);

         this.Controls.Add(lb_Steps = new ListBox());
         lb_Steps.Location = new Point(10, 10);
         lb_Steps.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 20);
         lb_Steps.Font = Config.font_Main;
         //lb_Steps.DrawMode = DrawMode.OwnerDrawVariable;
         //lb_Steps.DrawItem += lb_Steps_DrawItem;
      }

      private void UpdateProgress_Shown(object sender, EventArgs e)
      {
         MainThread.Start();
      }
      private void UpdateProgress_FormClosing(object sender, FormClosingEventArgs e)
      {
         try
         {
            MainThread.Abort();
         }
         catch { }
      }
      private int SetItemHeight()
      {
         int result = 0;
         Graphics g = this.CreateGraphics();
         result += (int)g.MeasureString("Yy", lb_Steps.Font).Height + 4;
         return result;
      }
      private void lb_Steps_DrawItem(object sender, DrawItemEventArgs e)
      {
         throw new NotImplementedException();
      }

      private void ThreadFunc()
      {
         for (int i = 0; i < 7; i++)
         {
            switch (i)
            {
               case 0:
                  lb_Steps.Items.Add("Подготовка"); lb_Steps.SelectedIndex = i;
                  if (i >= CurrentStep)
                  {
                     Thread.Sleep(1000);
                  }
                  break;
               case 1:
                  lb_Steps.Items.Add("Загрузка обновления"); lb_Steps.SelectedIndex = i;
                  if (i >= CurrentStep)
                  {
                     Thread.Sleep(1000);
                     if (Functions.DownloadUpdate() == false)
                     {
                        this.Close();
                        return;
                     }
                  }
                  break;
               case 2:
                  lb_Steps.Items.Add("Перезапуск приложения"); lb_Steps.SelectedIndex = i;
                  if (i >= CurrentStep)
                  {
                     Thread.Sleep(1000);
                     Functions.RunUpdate();
                     this.Close();
                     return;
                  }
                  break;
               case 3:
                  lb_Steps.Items.Add("Ожидание закрытия старой версии"); lb_Steps.SelectedIndex = i;
                  if (i >= CurrentStep)
                  {
                     Thread.Sleep(1000);
                     if (Functions.CloseApps() == false)
                     {
                        this.Close();
                        return;
                     }
                  }
                  break;
               case 4:
                  lb_Steps.Items.Add("Применение обновления"); lb_Steps.SelectedIndex = i;
                  if (i >= CurrentStep)
                  {
                     Thread.Sleep(1000);
                     if (Functions.MakeUpdate() == false)
                     {
                        this.Close();
                        return;
                     }
                  }
                  break;
               case 5:
                  lb_Steps.Items.Add("Загрузка дополнительных библиотек"); lb_Steps.SelectedIndex = i;
                  if (i >= CurrentStep)
                  {
                     Thread.Sleep(1000);
                     if (Functions.CheckFiles() == false)
                     {
                        this.Close();
                        return;
                     }
                  }
                  break;
               case 6:
                  lb_Steps.Items.Add("Запуск новой версии"); lb_Steps.SelectedIndex = i;
                  if (i >= CurrentStep)
                  {
                     Thread.Sleep(1000);
                     Functions.RunNew();
                     this.Close();
                     return;
                  }
                  break;
            }
         }
         this.Close();
      }
   }
}