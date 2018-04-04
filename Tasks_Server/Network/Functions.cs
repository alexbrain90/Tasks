using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Tasks_Server
{
    partial class Network
    {
        private bool CheckVersion(string version)
        {
            string[] list = new string[] { "0.10.5.46" };
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == version)
                    return true;
            }
            return false;
        }

        #region Users
        /// <summary>
        /// Check expired connection thread
        /// </summary>
        Thread t_AuthTimer;
        /// <summary>
        /// Mutex for work with list of connections
        /// </summary>
        Mutex m_Auth = new Mutex();
        /// <summary>
        /// List of connections
        /// </summary>
        public List<Connection> c_List = new List<Connection>();

        private void User_Auth(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string Name = ByteToString(data);
            data = ByteCut(data, StringToByte(Name).Length);
            string Hash = ByteToString(data);
            data = ByteCut(data, StringToByte(Hash).Length);
            string Version = "old";
            try
            {
                Version = ByteToString(data);
            }
            catch { }

            object[] sql = SQL.getData("SELECT Id, Password FROM Users WHERE Name=\'" + Name + "\'");

            if (sql.Length == 1)
            {
                if (CheckVersion(Version) == false)
                {
                    data = StringToByte("Update");
                    //data = StringToByte("203");
                    SendMessage(tcp, data);
                    return;
                }
                sql = (object[])sql[0];
                if (sql[1] == DBNull.Value || sql[1].ToString() == "9069CA78E7450A285173431B3E52C5C25299E473")
                {
                    data = StringToByte("202");
                    data = ByteAdd(data, StringToByte(Auth_New((int)sql[0], Name).ToString()));
                    data = ByteAdd(data, StringToByte(((int)sql[0]).ToString()));
                    SendMessage(tcp, data);
                    return;
                }
                string tmp = sql[1].ToString();
                if (Hash == tmp)
                {
                    data = StringToByte("200");
                    data = ByteAdd(data, StringToByte(Auth_New((int)sql[0], Name).ToString()));
                    data = ByteAdd(data, StringToByte(sql[0].ToString()));
                    SendMessage(tcp, data);

                    SQL.getData("UPDATE [Users] SET [Version]=\'" + Version + "\' WHERE [ID]=\'" + sql[0].ToString() + "\'");

                    sw.Stop();
                    Program.log.WriteLine("Успешная авторизация: " + Name + ". Хэш пароля: " + Hash + ". Версия ПО: " + Version + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
                    return;
                }
            }

            data = new byte[0];
            data = ByteAdd(data, StringToByte("204"));
            SendMessage(tcp, data);

            sw.Stop();
            Program.log.WriteLine("Пользователь или пароль не распознаны: " + Name + ". Хэш пароля: " + Hash + ". Версия ПО: " + Version + "   [" + sw.ElapsedMilliseconds.ToString() + "]", true);
            return;
        }
        private void User_Info(TcpClient tcp, byte[] data)
        {
            long id = Convert.ToInt64(ByteToString(data));
            id = Auth_Check(id);
            if (Auth_SendError(tcp, (int)id))
                return;

            object[] sql = SQL.getData("SELECT FIO, Sbe, Post FROM Users WHERE Id=\'" + id.ToString() + "\'");
            if (sql.Length != 1)
            {
                data = StringToByte("204");
                SendMessage(tcp, data);
                return;
            }

            sql = (object[])sql[0];
            string fio = sql[0].ToString();
            string sbe = sql[1].ToString();
            string post = sql[2].ToString();
            bool workers = false;

            sql = SQL.getData("SELECT Id FROM Users WHERE Manager=\'" + id.ToString() + "\'");
            if (sql.Length == 0)
                workers = false;
            else
                workers = true;

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(fio));
            data = ByteAdd(data, StringToByte(sbe));
            data = ByteAdd(data, StringToByte(post));
            data = ByteAdd(data, StringToByte(workers.ToString().ToLower()));
            SendMessage(tcp, data);
        }
        private void User_Password(TcpClient tcp, byte[] data)
        {
            long id = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string oldP = ByteToString(data); data = ByteCut(data);
            string newP = ByteToString(data); data = ByteCut(data);

            id = Auth_Check(id);
            if (Auth_SendError(tcp, (int)id))
                return;

            if (oldP == "9069CA78E7450A285173431B3E52C5C25299E473")
                SQL.getData("UPDATE Users SET Password=\'" + newP + "\' WHERE Id=\'" + id.ToString() + "\' AND (Password IS NULL OR Password=\'" + oldP + "\')");
            else
                SQL.getData("UPDATE Users SET Password=\'" + newP + "\' WHERE Id=\'" + id.ToString() + "\' AND Password=\'" + oldP + "\'");

            data = StringToByte("100");

            SendMessage(tcp, data);
        }
        private void User_Exit(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long id = Convert.ToInt64(ByteToString(data));

            Auth_Remove(id);

            sw.Stop();
            Program.log.WriteLine("Выход из программы: " + id.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void User_Directions(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT [Direction] FROM [Tasks_Users] WHERE [UserName]=\'" + idU.ToString() + "\' GROUP BY [Direction] ORDER BY [Direction]");

            if (sql == null || sql.Length == 0)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось получить список направлений: " + idC.ToString(), true);
            }
            else
            {
                data = StringToByte("100");
                data = ByteAdd(data, StringToByte(sql.Length.ToString()));
                for(int i =0; i < sql.Length; i++)
                    data = ByteAdd(data, StringToByte((string)((object[])sql[i])[0]));
            }

            SendMessage(tcp, data);
        }

        private int Auth_Check(long id)
        {
            int result = -1;
            m_Auth.WaitOne(1000);

            int n = c_List.FindIndex(x => x.Id.Equals(id));

            if (n != -1)
            {
                c_List[n].LastConnect = DateTime.Now.Ticks;
                result = c_List[n].UserId;
            }

            m_Auth.ReleaseMutex();
            return result;
        }
        private bool Auth_Replace(long id, int newID)
        {
            bool result = false;
            m_Auth.WaitOne(1000);

            int n = c_List.FindIndex(x => x.Id.Equals(id));

            if (n != -1)
            {
                c_List[n].LastConnect = DateTime.Now.Ticks;
                c_List[n].UserId = newID;
                result = true;
            }

            m_Auth.ReleaseMutex();
            return result;
        }
        private long Auth_New(int user, string name)
        {
            m_Auth.WaitOne(1000);
            long result = DateTime.Now.Ticks;

            c_List.Add(new Connection { Id = result, LastConnect = result, UserId = user, UserName = name });

            m_Auth.ReleaseMutex();
            return result;
        }
        public bool Auth_Remove(long id)
        {
            bool result = false;
            m_Auth.WaitOne(1000);

            int n = c_List.FindIndex(x => x.Id.Equals(id));

            if (n != -1)
            {
                c_List.RemoveAt(n);
                result = true;
            }

            m_Auth.ReleaseMutex();
            return result;
        }
        private void Auth_Timer()
        {
            while(true)
            {
                try
                {
                    m_Auth.WaitOne();

                    long tmp = DateTime.Now.AddHours(-1).Ticks;

                    for(int i =0; i < c_List.Count; i++)
                    {
                        if (c_List[i].LastConnect < tmp)
                        {
                            Program.log.WriteLine("Обнаружено незакрытое подключение " + c_List[i].Id.ToString() + " - " + c_List[i].UserId.ToString(), true);
                            c_List.RemoveAt(i);
                            break;
                        }
                    }

                    m_Auth.ReleaseMutex();
                }
                catch { }

                Thread.Sleep(60000);
            }
        }
        private bool Auth_SendError(TcpClient tcp, int id)
        {
            if (id == -1)
            {
                SendMessage(tcp, StringToByte("201"));
                return true;
            }
            return false;
        }
        #endregion

        #region Manager
        private void Manager_List(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data));
            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            string[,] list = Manager_ListCreate(new string[0, 3], idU);

            int n = list.Length / 3;
            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(n.ToString()));
            for(int i =0; i < n; i++)
            {
                data = ByteAdd(data, StringToByte(list[i, 0]));
                data = ByteAdd(data, StringToByte(list[i, 1]));
                data = ByteAdd(data, StringToByte(list[i, 2]));
            }

            SendMessage(tcp, data);
        }
        private string[,] Manager_ListCreate(string[,] list, int idU)
        {
            string[,] result = list;
            object[] sql = SQL.getData("SELECT Id, Name, Manager FROM Users WHERE Manager=\'" + idU.ToString() + "\' ORDER BY Name");
            if (sql == null || sql.Length == 0)
                return result;

            for(int i =0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                result = StringAdd(result, new string[,] { { line[0].ToString(), line[1].ToString(), line[2].ToString() } }, 3);
                result = Manager_ListCreate(result, Convert.ToInt32(line[0].ToString()));
            }
            return result;
        }
        private void Manager_Change(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int idOU = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
            int idNU = Convert.ToInt32(ByteToString(data));
            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            if (Auth_Replace(idC, idNU) == true)
                data = StringToByte("100");
            else
                data = StringToByte("300");

            SendMessage(tcp, data);
        }
        #endregion

        #region Tasks
        private void Task_List(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int status = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
            long filterB = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long filterE = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int sortId = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
         int filterCoop = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
         string filterDirection = ByteToString(data); data = ByteCut(data);
         string filterName = ByteToString(data); data = ByteCut(data);

         int id = Auth_Check(idC);
            if (Auth_SendError(tcp, id))
                return;

            string request = "SELECT [Tasks].[Name], [Tasks].[DateAdd], [Tasks].[DateFinish], [Tasks].[DateStart], [Tasks].[DateEnd], [Tasks_Users].[Type], [Tasks_Users].[Direction], [Tasks].[Deleted] FROM [Tasks_Users] INNER JOIN [Tasks] ON [Tasks_Users].[Task]=[Tasks].[DateAdd] WHERE [Tasks_Users].[UserName]=\'" + id.ToString() + "\'";

            if (status == 1)
                request += " AND [Tasks].[DateFinish] IS NULL";
            else if (status == 2)
                request += " AND [Tasks].[DateFinish] IS NOT NULL";
            else if (status == 3)
                request += " AND [Tasks].[Deleted]=\'1\'";
            if (status == 1 || status == 2)
                request += " AND [Tasks].[Deleted]=\'0\'";

         if (filterDirection == "%Main%")
            request += " AND [Tasks_Users].[Direction]=\'\'";
         else if (filterDirection == "%All%")
            request += "";
         else
            request += " AND [Tasks_Users].[Direction]=\'" + filterDirection + "\'";

            if (filterB != 0 && filterE != 0)
                request += " AND (([Tasks].[DateStart]>=\'" + filterB.ToString() + "\' AND [Tasks].[DateStart]<=\'" + filterE.ToString() + "\') OR ([Tasks].[DateEnd] >= \'" + filterB.ToString() + "\' AND [Tasks].[DateEnd] <= \'" + filterE.ToString() + "\') OR ([Tasks].[DateStart] <= \'" + filterB.ToString() + "\' AND [Tasks].[DateEnd] >= \'" + filterE.ToString() + "\'))";

         if (filterName != "")
            request += " AND [Tasks].[Name] LIKE \'%" + filterName + "%\'";

            if (sortId == 0)
                request += " ORDER BY [Tasks].[Name]";
            else if (sortId == 1)
                request += " ORDER BY [Tasks].[DateAdd]";
            else if (sortId == 2)
                request += " ORDER BY [Tasks].[DateStart]";
            else if (sortId == 3)
                request += " ORDER BY [Tasks].[DateEnd]";
            else if (sortId == 4)
                request += " ORDER BY [Tasks].[DateFinish]";

            object[] sql = SQL.getData(request);

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for(int i =0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte(line[0].ToString()));
                data = ByteAdd(data, StringToByte(((long)line[1]).ToString()));
                data = ByteAdd(data, line[2] == DBNull.Value ? StringToByte("0") : StringToByte(((long)line[2]).ToString()));
                data = ByteAdd(data, StringToByte(((long)line[3]).ToString()));
                data = ByteAdd(data, StringToByte(((long)line[4]).ToString()));
                object[] sql2 = SQL.getData("SELECT UserName FROM Tasks_Users WHERE Task=\'" + ((long)line[1]).ToString() + "\'");
                data = ByteAdd(data, StringToByte(sql2.Length.ToString()));
                data = ByteAdd(data, StringToByte(((int)line[5]).ToString()));
                data = ByteAdd(data, StringToByte((string)line[6]));
                data = ByteAdd(data, StringToByte(Convert.ToByte((bool)line[7]).ToString()));
                sql2 = SQL.getData("SELECT TOP 1 [Type] FROM [Events] WHERE [TaskId]=\'" + ((long)line[1]).ToString() + "\' AND [UserId]=\'" +id.ToString() + "\'");
                data = ByteAdd(data, StringToByte(sql2.Length.ToString()));
            }

            SendMessage(tcp, data);
        }
        private void Task_Info(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data));
            data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT [Tasks].[Name], [Tasks].[Description], [Tasks].[DateStart], [Tasks].[DateEnd], [Tasks].[DateFinish], [Tasks_Users].[Type], [Tasks_Users].[Direction], [Tasks].[Deleted] FROM [Tasks] INNER JOIN [Tasks_Users] ON [Tasks].[DateAdd]=[Tasks_Users].[Task] WHERE [Tasks].[DateAdd]=\'" + idT.ToString() + "\' AND [Tasks_Users].[UserName]=\'" + idU.ToString() + "\'");

            if (sql.Length != 1)
            {
                data = StringToByte("302");
                SendMessage(tcp, data);
                Program.log.WriteLine("Задача не найдена: " + idT.ToString(), true);
                return;
            }

            sql = (object[])sql[0];
            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql[0].ToString()));
            data = ByteAdd(data, StringToByte(sql[1].ToString()));
            data = ByteAdd(data, StringToByte(((long)sql[2]).ToString()));
            data = ByteAdd(data, StringToByte(((long)sql[3]).ToString()));
            //data = ByteAdd(data, StringToByte(((long)sql[4]).ToString()));

            object[] sql2 = SQL.getData("SELECT UserName FROM Tasks_Users WHERE Task=\'" + idT.ToString() + "\'");
            data = ByteAdd(data, StringToByte(sql2.Length.ToString()));

            data = ByteAdd(data, sql[4] == DBNull.Value ? StringToByte("0") : StringToByte(((long)sql[3]).ToString()));
            data = ByteAdd(data, StringToByte(((int)sql[5]).ToString()));
            data = ByteAdd(data, StringToByte(sql[6].ToString()));
            data = ByteAdd(data, StringToByte(Convert.ToByte((bool)sql[7]).ToString()));

            sql2 = SQL.getData("SELECT TOP 1 [Users].[FIO], [History].[DateTime] FROM [History] INNER JOIN [Users] ON [History].[UserId]=[Users].[ID] WHERE [TaskId]=\'" + idT.ToString() + "\' ORDER BY [History].[DateTime]");
            if (sql2 == null || sql2.Length != 1)
            {
                data = StringToByte("300");
                SendMessage(tcp, data);
                Program.log.WriteLine("Ошибка полученя сведений: " + idT.ToString(), true);
                return;
            }
            sql2 = (object[])sql2[0];
            data = ByteAdd(data, StringToByte((string)sql2[0]));
            data = ByteAdd(data, StringToByte(((long)sql2[1]).ToString()));

            sql2 = SQL.getData("SELECT TOP 1 [Users].[FIO], [History].[DateTime] FROM [History] INNER JOIN [Users] ON [History].[UserId]=[Users].[ID] WHERE [TaskId]=\'" + idT.ToString() + "\' ORDER BY [History].[DateTime] DESC");
            if (sql2 == null || sql2.Length != 1)
            {
                data = StringToByte("300");
                SendMessage(tcp, data);
                Program.log.WriteLine("Ошибка полученя сведений: " + idT.ToString(), true);
                return;
            }
            sql2 = (object[])sql2[0];
            data = ByteAdd(data, StringToByte((string)sql2[0]));
            data = ByteAdd(data, StringToByte(((long)sql2[1]).ToString()));

            SendMessage(tcp, data);
        }
        private void Task_Add(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data); data = ByteCut(data);
            string desc = ByteToString(data); data = ByteCut(data);
            long ds = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long df = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int type = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
            string dire = ByteToString(data); data = ByteCut(data);
            long id = DateTime.Now.Ticks;

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            #region Create events
            Task_Add(idU, id);

            Task_CheckName(true, idU, id, "", name);
            Task_CheckDesc(true, idU, id, "", desc);
            Task_CheckDateS(true, idU, id, 0, ds);
            Task_CheckDateE(true, idU, id, 0, df);
            Task_CheckDire(true, idU, id, "", dire);
            Task_CheckType(true, idU, id, -1, type);
            #endregion

            object sql = SQL.getData("INSERT INTO [Tasks] VALUES(\'" + id.ToString() + "\', NULL, \'" + name + "\', \'" + desc + "\', \'" + ds.ToString() + "\', \'" + df.ToString() + "\',\'0\')");
            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось добавить задачу", true);
            }
            else
            {
                sql = SQL.getData("INSERT INTO Tasks_Users VALUES(\'" + id.ToString() + "\', \'" + idU.ToString() + "\', \'" + type.ToString() + "\',\'" + dire + "\')");
                if (sql == null)
                {
                    data = StringToByte("300");
                    Program.log.WriteLine("Не удалось добавить связь сотрудник-задача", true);
                }
                else
                {
                    data = StringToByte("100");
                    data = ByteAdd(data, StringToByte(id.ToString()));
                }
            }
            SendMessage(tcp, data);
        }
        private void Task_Edit(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long taskID = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data); data = ByteCut(data);
            string desc = ByteToString(data); data = ByteCut(data);
            long ds = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long df = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int type = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
            string dire = ByteToString(data); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            #region Create events
            object[] current = SQL.getData("SELECT [Tasks].[Name],[Tasks].[Description],[Tasks].[DateStart],[Tasks].[DateEnd],[Tasks_Users].[Direction],[Tasks_Users].[Type] FROM [Tasks] INNER JOIN [Tasks_Users] ON [Tasks].[DateAdd]=[Tasks_Users].[Task] WHERE [Tasks].[DateAdd]=\'" + taskID.ToString() + "\' AND [Tasks_Users].[UserName]=\'" + idU.ToString() + "\'");
            if (current == null || current.Length != 1)
            {
                SendMessage(tcp, StringToByte("300"));
                return;
            }
            current = (object[])current[0];
            Task_CheckName(false, idU, taskID, (string)current[0], name);
            Task_CheckDesc(false, idU, taskID, (string)current[1], desc);
            Task_CheckDateS(false, idU, taskID, (long)current[2], ds);
            Task_CheckDateE(false, idU, taskID, (long)current[3], df);
            Task_CheckDire(false, idU, taskID, (string)current[4], dire);
            Task_CheckType(false, idU, taskID, (int)current[5], type);
            #endregion

            object sql = SQL.getData("UPDATE [Tasks] SET [Name]=\'" + name + "\', [Description]=\'" + desc + "\', [DateStart]=\'" + ds.ToString() + "\', [DateEnd]=\'" + df.ToString() + "\' WHERE [DateAdd]=\'" + taskID.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось обновить задачу: " + taskID.ToString(), true);
            }
            else
            {
                sql = SQL.getData("UPDATE [Tasks_Users] SET [Type]=\'" + type.ToString() + "\', [Direction]=\'" + dire + "\' WHERE [Task]=\'" + taskID.ToString() + "\' AND [UserName]=\'" + idU.ToString() + "\'");

                if (sql == null)
                {
                    data = StringToByte("300");
                    Program.log.WriteLine("Не удалось обновить задачу: " + taskID.ToString(), true);
                }
                else
                    data = StringToByte("100");
            }
            SendMessage(tcp, data);
        }
        private void Task_Do(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Task_Do(idU, idT);

            object sql = SQL.getData("UPDATE Tasks SET DateFinish=\'" + DateTime.Now.Ticks.ToString() + "\' WHERE DateAdd=\'" + idT.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось поставить отметку выполнения: " + idT.ToString(), true);
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }
        private void Task_UnDo(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Task_UnDo(idU, idT);

            object sql = SQL.getData("UPDATE Tasks SET DateFinish=NULL WHERE DateAdd=\'" + idT.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось убрать отметку выполнения: " + idT.ToString(), true);
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }
        private void Task_List_Plan(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks1 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks2 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            DateTime dt1, dt2;
            dt1 = new DateTime(ticks1);
            dt2 = new DateTime(ticks2);

            object[] sql = SQL.getData("SELECT Tasks.DateAdd, Tasks.Description, Tasks.DateStart, Tasks.DateEnd, [Tasks_Users].[Direction], [Tasks_Users].[Type] FROM Tasks_Users INNER JOIN Tasks ON Tasks_Users.Task=Tasks.DateAdd WHERE Tasks.Deleted=\'0\' AND Tasks_Users.UserName=\'" + idU.ToString() + "\' AND ((Tasks.DateStart>=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateStart<=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateEnd >=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd <=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateStart <=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd >=\'" + dt2.Ticks.ToString() + "\')) AND Tasks.DateAdd<=\'" + dt1.Ticks.ToString() + "\' AND ([Tasks].[DateFinish] IS NULL OR [Tasks].[DateFinish]<=\'" + dt2.Ticks.ToString() + "\') ORDER BY Tasks.DateStart");

            data = StringToByte("100");

            data = ByteAdd(data, StringToByte(sql.Length .ToString()));
            for (int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte(line[1].ToString()));
                data = ByteAdd(data, StringToByte(((long)line[2]).ToString()));
                data = ByteAdd(data, StringToByte(((long)line[3]).ToString()));

                string coopList = "";
                object[] sql2 = SQL.getData("SELECT Users.Id, Users.FIO FROM Users INNER JOIN Tasks_Users ON Users.Id=Tasks_Users.UserName WHERE Tasks_Users.Task=\'" + line[0].ToString() + "\' ORDER BY Users.Name");
                if (sql2 != null)
                {
                    for(int j = 0; j < sql2.Length; j++)
                    {
                        object[] line2 = (object[])sql2[j];
                        if ((int)line2[0] == idU)
                            continue;
                        coopList += ", " + FioToShort(line2[1].ToString());
                    }
                }
                if (coopList.Length > 2)
                    coopList = coopList.Substring(2);
                data = ByteAdd(data, StringToByte(coopList));
                data = ByteAdd(data, StringToByte((string)line[4]));
                data = ByteAdd(data, StringToByte((int)line[5]));
            }

            SendMessage(tcp, data);
        }
        private void Task_List_Report(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks1 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks2 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            DateTime dt1, dt2;
            dt1 = new DateTime(ticks1);
            dt2 = new DateTime(ticks2);

            object[] sql = SQL.getData("SELECT Tasks.DateAdd, Tasks.DateFinish, Tasks.Description, Tasks.DateStart, Tasks.DateEnd, [Tasks_Users].[Direction], [Tasks_Users].[Type] FROM Tasks_Users INNER JOIN Tasks ON Tasks_Users.Task=Tasks.DateAdd WHERE Tasks.Deleted=\'0\' AND Tasks_Users.UserName=\'" + idU.ToString() + "\' AND ((Tasks.DateStart>=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateStart<=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateEnd >=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd <=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateStart <=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd >=\'" + dt2.Ticks.ToString() + "\')) ORDER BY Tasks.DateStart");

            data = StringToByte("100");

            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for (int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte(line[2].ToString()));
                data = ByteAdd(data, StringToByte(((long)line[3]).ToString()));
                data = ByteAdd(data, StringToByte(((long)line[4]).ToString()));
                data = ByteAdd(data, StringToByte(((long)line[0]).ToString()));
                data = ByteAdd(data, line[1] == DBNull.Value ? StringToByte("0") : StringToByte(((long)line[1]).ToString()));

                string coopList = "";
                object[] sql2 = SQL.getData("SELECT Users.Id, Users.FIO FROM Users INNER JOIN Tasks_Users ON Users.Id=Tasks_Users.UserName WHERE Tasks_Users.Task=\'" + line[0].ToString() + "\' ORDER BY Users.Name");
                if (sql2 != null)
                {
                    for (int j = 0; j < sql2.Length; j++)
                    {
                        object[] line2 = (object[])sql2[j];
                        if ((int)line2[0] == idU)
                            continue;
                        coopList += ", " + FioToShort(line2[1].ToString());
                    }
                }
                if (coopList.Length > 2)
                    coopList = coopList.Substring(2);
                data = ByteAdd(data, StringToByte(coopList));

                string stepList1 = "", stepList2 = "";
                int n = 0;
                sql2 = SQL.getData("SELECT DateFinish, Name FROM Steps WHERE Task=\'" + line[0].ToString() + "\' ORDER BY DateAdd");
                if (sql2 != null && sql2.Length != 0)
                {
                    for (int j = 0; j < sql2.Length; j++)
                    {
                        object[] line2 = (object[])sql2[j];
                        if (line2[0] != DBNull.Value)
                        {
                            stepList1 += "%ns%" + line2[1];
                            n++;
                        }
                        else
                        {
                            stepList2 += "%ns%" + line2[1];
                        }
                    }
                    n = 100 * n / sql2.Length;
                    if (stepList1.Length > 4)
                        stepList1 = stepList1.Substring(4);
                    if (stepList2.Length > 4)
                        stepList2 = stepList2.Substring(4);
                }
                data = ByteAdd(data, StringToByte(n.ToString()));
                data = ByteAdd(data, StringToByte(stepList1));
                data = ByteAdd(data, StringToByte(stepList2));

                string messList = "";
                sql2 = SQL.getData("SELECT Messages.Message, Users.Name FROM Messages INNER JOIN Users ON Messages.UserName=Users.Id WHERE Messages.Task=\'" + line[0].ToString() + "\' ORDER BY Messages.DateTime");
                if (sql2 != null && sql2.Length != 0)
                {
                    for (int j = 0; j < sql2.Length; j++)
                    {
                        object[] line2 = (object[])sql2[j];
                        messList += "%nm%\t" + line2[1] + "\r\n" + line2[0];
                    }
                    if (messList.Length > 4)
                        messList = messList.Substring(4);
                }
                data = ByteAdd(data, StringToByte(messList));
                data = ByteAdd(data, StringToByte((string)line[5]));
                data = ByteAdd(data, StringToByte((int)line[6]));
            }

            SendMessage(tcp, data);
        }
        private void Task_Delete(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Task_Delete(idU, idT);

            object sql = SQL.getData("UPDATE [Tasks] SET [Deleted]=\'1\' WHERE [DateAdd]=\'" + idT.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось удалить задачу: " + idT.ToString(), true);
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }
        private void Task_UnDelete(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Task_UnDelete(idU, idT);

            object sql = SQL.getData("UPDATE [Tasks] SET [Deleted]=\'0\' WHERE DateAdd=\'" + idT.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось поставить отметку выполнения: " + idT.ToString(), true);
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }

        private void Task_CheckName(bool isNew, int user, long task, string valueOld, string valueNew)
        {
            if (valueOld == valueNew)
                return;

            History_Add(user, task, 101, valueOld, valueNew, isNew);
        }
        private void Task_CheckDesc(bool isNew, int user, long task, string valueOld, string valueNew)
        {
            if (valueOld == valueNew)
                return;

            History_Add(user, task, 102, valueOld, valueNew, isNew);
        }
        private void Task_CheckDateS(bool isNew, int user, long task, long valueOld, long valueNew)
        {
            if (valueOld == valueNew)
                return;

            History_Add(user, task, 103, DateTimeToString(valueOld), DateTimeToString(valueNew), isNew);
        }
        private void Task_CheckDateE(bool isNew, int user, long task, long valueOld, long valueNew)
        {
            if (valueOld == valueNew)
                return;

            History_Add(user, task, 104, DateTimeToString(valueOld), DateTimeToString(valueNew), isNew);
        }
        private void Task_CheckDire(bool isNew, int user, long task, string valueOld, string valueNew)
        {
            if (valueOld == valueNew)
                return;

            History_Add(user, task, 105, valueOld, valueNew, isNew);
        }
        private void Task_CheckType(bool isNew, int user, long task, int valueOld, int valueNew)
        {
            if (valueOld == valueNew)
                return;

            string text1 = "";
            if (valueOld == 0)
                text1 = "Основная";
            else if (valueOld == 1)
                text1 = "Контроль";
            string text2 = "";
            if (valueNew == 0)
                text2 = "Основная";
            else if (valueNew == 1)
                text2 = "Контроль";

            History_Add(user, task, 106, text1, text2, isNew);
        }

        private void Task_Add(int user, long task)
        {
            History_Add(user, task, 111, "", "", false);
        }
        private void Task_Do(int user, long task)
        {
            History_Add(user, task, 112, "", "", false);
        }
        private void Task_UnDo(int user, long task)
        {
            History_Add(user, task, 113, "", "", false);
        }
        private void Task_Delete(int user, long task)
        {
            History_Add(user, task, 114, "", "", false);
        }
        private void Task_UnDelete(int user, long task)
        {
            History_Add(user, task, 115, "", "", false);
        }
        private void Task_Copy(int user, long task)
        {
            History_Add(user, task, 116, "", "", false);
        }
        #endregion

        #region Steps
        private void Step_List(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT Name, DateFinish, DateAdd FROM Steps WHERE Task=\'" + idT.ToString() + "\' ORDER BY DateAdd");

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for (int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte(line[0].ToString()));
                data = line[1] == DBNull.Value ? ByteAdd(data, StringToByte("")) : ByteAdd(data, StringToByte(((long)line[1]).ToString()));
                data = ByteAdd(data, StringToByte(((long)line[2]).ToString()));
            }

            SendMessage(tcp, data);
        }
        private void Step_Info(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idS = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT Name FROM Steps WHERE DateAdd=\'" + idS.ToString() + "\'");
            if (sql.Length != 1)
            {
                SendMessage(tcp, StringToByte("300"));
                Program.log.WriteLine("Не удалось получить сведения о шаге: " + idS.ToString(), true);
            }
            else
            {
                sql = (object[])sql[0];
                data = StringToByte("100");
                data = ByteAdd(data, StringToByte(sql[0].ToString()));
                SendMessage(tcp, data);
            }
        }
        private void Step_Add(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Step_Add(idU, idT, name);

            object sql = SQL.getData("INSERT INTO Steps VALUES (\'" + DateTime.Now.Ticks.ToString() + "\', NULL, \'" + idT.ToString() + "\', \'" + name + "\')");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось добавить шаг к задаче: " + idT.ToString(), true);
                SendMessage(tcp, data);
                return;
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }
        private void Step_Edit(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idS = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Step_Edit(idU, idS, name);

            object sql = SQL.getData("UPDATE Steps SET Name=\'" + name + "\' WHERE DateAdd=\'" + idS.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось изменить шаг: " + idS.ToString(), true);
                SendMessage(tcp, data);
                return;
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }
        private void Step_Do(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idS = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object sql = SQL.getData("UPDATE Steps SET DateFinish=\'" + DateTime.Now.Ticks.ToString() + "\' WHERE DateAdd=\'" + idS.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось установить отметку выполнения шага: " + idS.ToString(), true);
                SendMessage(tcp, data);
                return;
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }

        private void Step_Add(int user, long task, string newV)
        {
            History_Add(user, task, 201, "", newV, false);
        }
        private void Step_Edit(int user, long step, string newV)
        {
            string oldV = ""; long task = -1;
            object[] sql = SQL.getData("SELECT [Name], [Task] FROM [Steps] WHERE [DateAdd]=\'" + step.ToString() + "\'");
            if (sql == null || sql.Length == 0)
                return;

            sql = (object[])sql[0];
            newV = (string)sql[0];
            task = (long)sql[1];

            History_Add(user, task, 202, oldV, newV, false);
        }
        private void Step_Do(int user, long step)
        {
            long task = -1;
            object[] sql = SQL.getData("SELECT [Task] FROM [Steps] WHERE [DateAdd]=\'" + step.ToString() + "\'");
            if (sql == null || sql.Length == 0)
                return;

            sql = (object[])sql[0];
            task = (long)sql[0];

            History_Add(user, task, 203, "", "", false);
        }
        private void Step_UnDo(int user, long task)
        {
            History_Add(user, task, 204, "", "", false);
        }
        private void Step_Delete(int user, long task)
        {
            History_Add(user, task, 203, "", "", false);
        }
        private void Step_UnDelete(int user, long task)
        {
            History_Add(user, task, 204, "", "", false);
        }
        #endregion

        #region Messages
        private void Message_List(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT Users.Name, Messages.DateTime, Messages.Message FROM Messages INNER JOIN Users ON Users.Id=Messages.UserName WHERE Messages.Task=\'" + idT.ToString() + "\' ORDER BY DateTime");

            if (sql == null)
            {
                data = StringToByte("300");
                SendMessage(tcp, data);
                Program.log.WriteLine("Не удалось получить список сообщений задачи: " + idT.ToString(), true);
                return;
            }

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for(int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte(line[0].ToString()));
                data = ByteAdd(data, StringToByte(((long)line[1]).ToString()));
                data = ByteAdd(data, StringToByte(line[2].ToString()));
            }

            SendMessage(tcp, data);
        }
        private void Message_Add(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string message = ByteToString(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Message_Add(idU, idT, message);

            object sql = SQL.getData("INSERT INTO Messages VALUES (\'" + idT.ToString() + "\', \'" + idU.ToString() + "\', \'" + DateTime.Now.Ticks.ToString() + "\', \'" + message + "\')");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось добавить сообщение к задаче: " + idT.ToString(), true);
                SendMessage(tcp, data);
                return;
            }
            else
            {
                data = StringToByte("100");
            }

            SendMessage(tcp, data);
        }

        private void Message_Add(int user, long task, string newV)
        {
            History_Add(user, task, 401, "", newV, false);
        }
        #endregion

        #region Cooperation
        private void Coop_List(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT Users.Id, Users.Name FROM Users INNER JOIN Tasks_Users ON Users.Id=Tasks_Users.UserName WHERE Tasks_Users.Task=\'" + idT.ToString() + "\' ORDER BY Users.Name");
            if (sql == null)
            {
                SendMessage(tcp, StringToByte("300"));
                Program.log.WriteLine("Не удалось получить список сотрудников для задачи: " + idT.ToString(), true);
                return;
            }

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for (int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte(((int)line[0]).ToString()));
                data = ByteAdd(data, StringToByte(line[1].ToString()));
            }

            SendMessage(tcp, data);
        }
        private void Coop_Filter(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string filter = ByteToString(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT Id, Name From Users WHERE Name LIKE \'%" + filter + "%\' ORDER BY Name");
            if (sql == null)
            {
                SendMessage(tcp, StringToByte("300"));
                Program.log.WriteLine("Не удалось получить список сотрудников для задачи: " + idT.ToString() + " с фильтром \"" + filter + "\"", true);
                return;
            }

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for (int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte(((int)line[0]).ToString()));
                data = ByteAdd(data, StringToByte(line[1].ToString()));
            }

            SendMessage(tcp, data);
        }
        private void Coop_Add(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int idA = Convert.ToInt32(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Coop_Add(idU, idT, idA);

            object sql = SQL.getData("INSERT INTO Tasks_Users VALUES(\'" + idT.ToString() + "\', \'" + idA.ToString() + "\', \'0\', \'\')");

            if (sql == null)
            {
                SendMessage(tcp, StringToByte("300"));
                Program.log.WriteLine("Не удалось связать сотрудника с задачей: " + idT.ToString() + " - " + idA.ToString(), true);
            }
            else
            {
                SendMessage(tcp, StringToByte("100"));
            }
        }
        private void Coop_Delete(TcpClient tcp, byte[] data)
        {
            long idC= Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int idD = Convert.ToInt32(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            Coop_Delete(idU, idT, idD);

            object sql = SQL.getData("DELETE FROM Tasks_Users WHERE Task=\'" + idT.ToString() + "\' AND UserName=\'" + idD.ToString() + "\'");

            if (sql == null)
            {
                SendMessage(tcp, StringToByte("300"));
                Program.log.WriteLine("Не удалось отвязать сотрудника от задачи: " + idT.ToString() + " - " + idD.ToString(), true);
            }
            else
            {
                SendMessage(tcp, StringToByte("100"));
            }
        }

        private void Coop_Add(int user, long task, int idA)
        {
            object[] sql = SQL.getData("SELECT [FIO] FROM [Users] WHERE [ID]=\'" + idA.ToString() + "\'");
            if (sql == null || sql.Length == 0)
                return;

            sql = (object[])sql[0];
            string userName = (string)sql[0];

            History_Add(user, task, 401, "", userName, false);
        }
        private void Coop_Delete(int user, long task, int idD)
        {
            object[] sql = SQL.getData("SELECT [FIO] FROM [Users] WHERE [ID]=\'" + idD.ToString() + "\'");
            if (sql == null || sql.Length == 0)
                return;

            sql = (object[])sql[0];
            string userName = (string)sql[0];

            History_Add(user, task, 401, "", userName, false);
        }
        #endregion

        #region History & Events
        private void History_Add(int user, long task, int type, string valueOld, string valueNew, bool isNew)
        {
            if (valueOld == valueNew && valueNew != "" && valueOld != "")
                return;

            if (isNew == true)
                type = -type;

            SQL.getData("INSERT INTO [History] VALUES(\'" + task.ToString() + "\',\'" + user.ToString() + "\',\'" + DateTime.Now.Ticks.ToString() + "\',\'" + type.ToString() + "\',\'" + valueOld + "\',\'" + valueNew + "\')");

            object[] sql = SQL.getData("SELECT [UserName] FROM [Tasks_Users] WHERE [Task]=\'" + task.ToString() + "\' AND [UserName]<>\'" + user.ToString() + "\'");
            if (sql == null || sql.Length == 0)
                return;
            int userID = -1;
            for (int i = 0; i < sql.Length; i++)
            {
                userID = (int)((object[])sql[i])[0];
                SQL.getData("INSERT INTO [Events] VALUES(\'" + task.ToString() + "\',\'" + userID.ToString() + "\',\'" + user.ToString() + "\',\'" + DateTime.Now.Ticks.ToString() + "\',\'" + type.ToString() + "\',\'" + valueOld + "\',\'" + valueNew + "\',\'0\')");
            }
        }
        private void Event_List(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("SELECT [Tasks].[Name], [Events].[Type], [Events].[DateTime], [Events].[OldV], [Events].[NewV] FROM [Events] INNER JOIN [Tasks] ON [Events].[TaskId]=[Tasks].[DateAdd] WHERE [Events].[UserId]=\'" + idU.ToString() + "\' AND [Events].[Showed]=\'0\'");

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for (int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte((string)line[0]));
                data = ByteAdd(data, StringToByte((int)line[1]));
                data = ByteAdd(data, StringToByte((long)line[2]));
                data = ByteAdd(data, StringToByte((string)line[3]));
                data = ByteAdd(data, StringToByte((string)line[4]));
            }

            SendMessage(tcp, data);

            SQL.getData("UPDATE [Events] SET [Showed]='1' WHERE [UserId]=\'" + idU.ToString() + "\' AND [Showed]=\'0\'");
        }
        private void Event_Delete(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object[] sql = SQL.getData("DELETE FROM [Events] WHERE [TaskId]=\'" + idT.ToString() + "\' AND [UserId]=\'" + idU.ToString() + "\'");

            SendMessage(tcp, StringToByte("100"));
        }

        /* Event types:
         * Tasks
         * 101 - Name, 102 - Description, 103 - DateStart, 104 - DateEnd, 105 - Direction, 106 - Type
         * 111 - Add, 112  - Do, 113 - UnDo, 114 - Delete, 115 - UnDelete, 116 - Copy)
         * 
         * Steps
         * 201 - Add, 202 - Edit, 203 - Do, 204 - UnDo, 205 - Delete, 206 - UnDelete
         * 
         * Cooperation
         * 301 - Add, 302 - Remove
         * 
         * Messages
         * 401 - New
        */
        #endregion

        #region Administration
        private void Admin_Users(TcpClient tcp, byte[] data)
        {
            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int type = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (idU == -1)
            {
                SendMessage(tcp, StringToByte("300"));
                return;
            }

            string command = "SELECT [FIO],[Sbe],[Post],[Version] FROM [Users]";
            if (type == 0)
                command += " ORDER BY [FIO]";
            else if (type == 1)
                command += " ORDER BY [Version]";
            else if (type == 2)
                command += " ORDER BY [Version] DESC";
            else if (type == 3)
                command += " WHERE [Password] IS NULL";

            object[] sql = SQL.getData(command);

            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));
            for (int i = 0; i < sql.Length; i++)
            {
                object[] line = (object[])sql[i];
                data = ByteAdd(data, StringToByte((string)line[0]));
                data = ByteAdd(data, StringToByte((string)line[1]));
                data = ByteAdd(data, StringToByte((string)line[2]));
                data = ByteAdd(data, StringToByte(line[3] == DBNull.Value ? "" : (string)line[3]));
            }

            SendMessage(tcp, data);
        }
        #endregion
    }
}