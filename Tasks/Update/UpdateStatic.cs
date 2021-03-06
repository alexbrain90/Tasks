﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Tasks.Update
{
   static class Functions
   {
      /// <summary>
      /// Имя приложения для проверки и загрузк обновлений
      /// </summary>
      private const string AppName = "Tasks";
      private static string[] filesMustHave = new string[] { "DocumentFormat.OpenXml.dll", "ClosedXML.dll" };

      /// <summary>
      /// Проверка необходимости перезапуска для обновления
      /// </summary>
      /// <returns>Возвращает true если необходим перезапуск</returns>
      static public bool checkLaunch(string[] args)
      {
         try
         {
            // Проверяем необходимость обновления программы
            if (Config.AppExecutable.Length >= 8 && Config.AppExecutable.Substring(Config.AppExecutable.Length - 8) == "_new.exe")
            {
               string appName = Config.AppExecutable.Substring(0, Config.AppExecutable.Length - 8);
               appName = Path.GetFileNameWithoutExtension(appName);
               // Ожидаем завершения предыдущей копии программы
               for (int i = 0; i < 10; i++)
               {
                  if (Process.GetProcessesByName(appName).Length != 0)
                     Thread.Sleep(1000);
               }
               try
               {
                  foreach (Process process in Process.GetProcessesByName(appName))
                     process.Kill();
               }
               catch
               {
                  return true;
               }
               // Заменяем файл приложения
               File.Copy(Config.AppExecutable, Config.AppExecutable.Substring(0, Config.AppExecutable.Length - 8), true);
               // Запускаем обновленную версию программы
               Process P = new Process();
               P.StartInfo.FileName = Config.AppExecutable.Substring(0, Config.AppExecutable.Length - 8);
               P.StartInfo.Arguments = "";
               for (int i = 0; i < args.Length; i++)
                  P.StartInfo.Arguments += " " + args[i];
               P.StartInfo.WorkingDirectory = Path.GetDirectoryName(Config.AppExecutable);
               P.StartInfo.UseShellExecute = true;
               P.Start();
               return true;
            }
            else
               File.Delete(Config.AppExecutable + "_new.exe");
         }
         catch
         {
            return true;
         }

         return false;
      }

      /// <summary>
      /// Выполняет поиск новой версии программы
      /// </summary>
      /// <returns>Возвращает true если удалось получить сведения</returns>
      static public void checkUpgrade()
      {
         Config.ServerVersion = Config.CurrentVersion;
         // Получаем данные о версии программы
         string Text = getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/info_" + Language.Index + ".txt");
         if (Text == "")
            Text = getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/info.txt");

         // Проверяем корректность полученных данных
         if (Text.Length == 0 || Text.Substring(0, 1) != "v")
            return;

         // Ищем самый первый перенос строки
         int n = 0;
         for (int i = 0; i < Text.Length - 1; i++)
         {
            if (Text.Substring(i, 2) == "\r\n")
            {
               n = i;
               break;
            }
         }

         // Извлекаем версию
         Config.ServerVersion = Text.Substring(1, n - 1);
         // Извлекаем описание
         Config.ServerVersionInfo = Text.Substring(n + 2);
      }
      /// <summary>
      /// Выполняет загрузку новой версии приложения
      /// </summary>
      /// <returns>Возвращает true если обновление было скачано и необходим перезапуск</returns>
      static public bool makeUpgrade(string[] args)
      {
         // Если текущая версия программы не совпадает с самой новой, запускаем процесс обновления
         if (Config.ServerVersion != Config.CurrentVersion)
         {
            // Удаляем имеющийся файла
            if (File.Exists(Config.AppExecutable + "_new.exe") == true)
               File.Delete(Config.AppExecutable + "_new.exe");

            // Загружаем новую версию
            if (getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/App.exe", Config.AppExecutable + "_new.exe") == false)
               return false;

            // Запускаем скачанный файл
            Process P = new Process();
            P.StartInfo.FileName = Config.AppExecutable + "_new.exe";

            P.StartInfo.Arguments = "";
            for (int i = 0; i < args.Length; i++)
               P.StartInfo.Arguments += " " + args[i];

            P.StartInfo.WorkingDirectory = Path.GetDirectoryName(Config.AppExecutable);
            P.StartInfo.UseShellExecute = true;
            P.Start();
            return true;
         }
         else
         {
            return false;
         }
      }

      static public bool checkFiles()
      {
         for (int i = 0; i < filesMustHave.Length; i++)
            if (File.Exists(filesMustHave[i]) == false)
               return false;
         return true;
      }
      static public bool downloadFiles()
      {
         bool result = true;
         for (int i = 0; i < filesMustHave.Length; i++)
         {
            try
            {
               File.Delete(filesMustHave[i]);
            }
            catch { }
            try
            {
               if (getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/" + filesMustHave[i], filesMustHave[i]) == false)
                  result = false;
            }
            catch { }
         }

         return result;
      }

      /// <summary>
      /// Функция для загрузки текстовой информации
      /// </summary>
      /// <param name="url">Адрес к информации</param>
      /// <returns>Возвращает загруженную строку</returns>
      static private string getHttpPage(string url)
      {
         try
         {
            WebRequest req = WebRequest.Create(url);
            WebResponse res = req.GetResponse();
            using (StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
            {
               return reader.ReadToEnd();
            }
         }
         catch
         {
            return "";
         }
      }
      /// <summary>
      /// Функция для загрузки файла
      /// </summary>
      /// <param name="url">Адрес к файлу на сервере</param>
      /// <param name="fileName">Путь к файлу на компьютере</param>
      /// <returns>В случае успеха возвращает true</returns>
      static private bool getHttpPage(string url, string fileName)
      {
         try
         {
            WebClient wc = new WebClient();
            wc.DownloadFile(url, fileName);

            return true;
         }
         catch
         {
            return false;
         }
      }

      static public bool UploadDump(string fileName)
      {
         try
         {
            WebClient wc = new WebClient();
            wc.UploadFile("http://194.154.82.6:8080/Apps/" + AppName + "/", fileName);

            File.Delete(fileName);

            return true;
         }
         catch (Exception ex)
         {
            return false;
         }
      }

      //static public void UpdateForm()
      //{
      //   System.Windows.Forms.DialogResult dr = new Forms.Update().ShowDialog();
      //   if (dr == System.Windows.Forms.DialogResult.Yes)
      //      if (Tasks.Update.makeUpgrade(new string[0]) == true)
      //         System.Windows.Forms.Application.Exit();
      //}

      static public void CheckVersion()
      {
         try
         {
            Config.ServerVersion = Config.CurrentVersion;
            // Получаем данные о версии программы
            string Text = getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/info_" + Language.Index + ".txt");
            if (Text == "")
               Text = getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/info.txt");

            // Проверяем корректность полученных данных
            if (Text.Length == 0 || Text.Substring(0, 1) != "v")
               return;

            // Ищем самый первый перенос строки
            int n = 0;
            for (int i = 0; i < Text.Length - 1; i++)
            {
               if (Text.Substring(i, 2) == "\r\n")
               {
                  n = i;
                  break;
               }
            }

            // Извлекаем версию
            Config.ServerVersion = Text.Substring(1, n - 1);
            // Извлекаем описание
            Config.ServerVersionInfo = Text.Substring(n + 2);
         }
         catch
         {
            Config.ServerVersion = Config.CurrentVersion;
            Config.ServerVersionInfo = "";
         }
      }
      /// <summary>
      /// Download new version of application
      /// </summary>
      /// <returns>true - on success download, false - on error</returns>
      static public bool DownloadUpdate()
      {
         try
         {
            // Check is new version exist on server
            if (Config.ServerVersion != Config.CurrentVersion)
            {
               // Delete update if exist
               if (File.Exists(Config.AppExecutable + "_new.exe") == true)
                  File.Delete(Config.AppExecutable + "_new.exe");

               // Download update
               if (getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/App.exe", Config.AppExecutable + "_new.exe") == false)
                  return false;

               return true;
            }
            else
               return false;
         }
         catch
         {
            return false;
         }
      }
      /// <summary>
      /// Run new version of application
      /// </summary>
      /// <returns>true - on success run, false - on error</returns>
      static public bool RunUpdate()
      {
         try
         {
            Process P = new Process();
            P.StartInfo.FileName = Config.AppExecutable + "_new.exe";
            P.StartInfo.WorkingDirectory = Path.GetDirectoryName(Config.AppExecutable);
            P.StartInfo.UseShellExecute = true;
            P.Start();
            return true;
         }
         catch
         {
            return false;
         }
      }
      /// <summary>
      /// Closing old version of application
      /// </summary>
      /// <returns>true on success</returns>
      static public bool CloseApps()
      {
         try
         {
            // Проверяем необходимость обновления программы
            if (Config.AppExecutable.Length >= 8 && Config.AppExecutable.Substring(Config.AppExecutable.Length - 8) == "_new.exe")
            {
               string appName = Config.AppExecutable.Substring(0, Config.AppExecutable.Length - 8);
               appName = Path.GetFileNameWithoutExtension(appName);
               // Ожидаем завершения предыдущей копии программы
               for (int i = 0; i < 10; i++)
               {
                  if (Process.GetProcessesByName(appName).Length != 0)
                     Thread.Sleep(1000);
               }
               try
               {
                  foreach (Process process in Process.GetProcessesByName(appName))
                     process.Kill();
                  return true;
               }
               catch
               {
                  return false;
               }
            }
            return false;
         }
         catch
         {
            return false;
         }
      }
      /// <summary>
      /// Check is update in progress
      /// </summary>
      /// <returns>true if update in progress</returns>
      static public bool CheckLaunch()
      {
         if (Config.AppExecutable.Length >= 8 && Config.AppExecutable.Substring(Config.AppExecutable.Length - 8) == "_new.exe")
            return true;
         else
         {
            try
            {
               File.Delete(Config.AppExecutable + "_new.exe");
            }
            catch { }
            return false;
         }
      }
      /// <summary>
      /// Apply update
      /// </summary>
      /// <returns>true on success</returns>
      static public bool MakeUpdate()
      {
         try
         {
            File.Copy(Config.AppExecutable, Config.AppExecutable.Substring(0, Config.AppExecutable.Length - 8), true);
            return true;
         }
         catch
         {
            return false;
         }
      }
      /// <summary>
      /// Check additional files
      /// </summary>
      /// <returns>true on success</returns>
      static public bool CheckFiles()
      {
         bool result = true;
         for (int i = 0; i < filesMustHave.Length; i++)
            if (File.Exists(filesMustHave[i]) == false)
               result = false;

         if (result == true)
            return true;

         result = true;
         for (int i = 0; i < filesMustHave.Length; i++)
         {
            try
            {
               File.Delete(filesMustHave[i]);
            }
            catch { }
            try
            {
               if (getHttpPage("http://194.154.82.6:8080/Apps/" + AppName + "/" + filesMustHave[i], filesMustHave[i]) == false)
                  result = false;
            }
            catch { }
         }

         return result;
      }
      /// <summary>
      /// Start new version of application
      /// </summary>
      /// <returns></returns>
      static public bool RunNew()
      {
         try
         {
            Process P = new Process();
            P.StartInfo.FileName = Config.AppExecutable.Substring(0, Config.AppExecutable.Length - 8);
            P.StartInfo.WorkingDirectory = Path.GetDirectoryName(Config.AppExecutable);
            P.StartInfo.UseShellExecute = true;
            P.Start();
            return true;
         }
         catch
         {
            return false;
         }
      }
   }
}