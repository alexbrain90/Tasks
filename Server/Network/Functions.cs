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
            string[] list = new string[] { "0.9.2.40"};
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == version)
                    return true;
            }
            return false;
        }

        #region Authentification
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
                    data = ByteAdd(data, StringToByte(Auth_New((int)sql[0]).ToString()));
                    data = ByteAdd(data, StringToByte(((int)sql[0]).ToString()));
                    SendMessage(tcp, data);
                    return;
                }
                string tmp = sql[1].ToString();
                if (Hash == tmp)
                {
                    data = StringToByte("200");
                    data = ByteAdd(data, StringToByte(Auth_New((int)sql[0]).ToString()));
                    data = ByteAdd(data, StringToByte(sql[0].ToString()));
                    SendMessage(tcp, data);

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
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Сведения о сотруднике: " + id.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void User_Password(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Смена пароля: " + id.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
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
        private long Auth_New(int user)
        {
            m_Auth.WaitOne(1000);
            long result = DateTime.Now.Ticks;

            c_List.Add(new Connection { Id = result, LastConnect = result, UserId = user });

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
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Список подчиненных: " + idU.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
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
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Смена сотрудника: " + idNU.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        #endregion

        #region Tasks
        private void Task_List(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int status = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
            long filterB = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long filterE = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int sortId = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);

            int id = Auth_Check(idC);
            if (Auth_SendError(tcp, id))
                return;

            string request = "SELECT Tasks.Name, Tasks.DateAdd, Tasks.DateFinish, Tasks.DateStart, Tasks.DateEnd FROM Tasks_Users INNER JOIN Tasks ON Tasks_Users.Task=Tasks.DateAdd WHERE Tasks_Users.UserName=\'" + id.ToString() + "\'";

            if (status == 1)
                request += " AND Tasks.DateFinish IS NULL";
            else if (status == 2)
                request += " AND Tasks.DateFinish IS NOT NULL";

            if (filterB != 0 && filterE != 0)
                request += " AND ((Tasks.DateStart>=\'" + filterB.ToString() + "\' AND Tasks.DateStart<=\'" + filterE.ToString() + "\') OR (Tasks.DateEnd >= \'" + filterB.ToString() + "\' AND Tasks.DateEnd <= \'" + filterE.ToString() + "\') OR (Tasks.DateStart <= \'" + filterB.ToString() + "\' AND Tasks.DateEnd >= \'" + filterE.ToString() + "\'))";

            if (sortId == 0)
                request += " ORDER BY Tasks.Name";
            else if (sortId == 1)
                request += " ORDER BY Tasks.DateAdd";
            else if (sortId == 2)
                request += " ORDER BY Tasks.DateStart";
            else if (sortId == 3)
                request += " ORDER BY Tasks.DateEnd";
            else if (sortId == 4)
                request += " ORDER BY Tasks.DateFinish";

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
            }

            SendMessage(tcp, data);

            sw.Stop();
            Program.log.WriteLine("Список задач пользователя: " + id.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Task_Info(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data));
            data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            // Проверка на соответсвие задачи пользователю

            object[] sql = SQL.getData("SELECT Name, Description, DateStart, DateEnd, DateFinish FROM Tasks WHERE DateAdd=\'" + idT.ToString() + "\'");

            if (sql.Length != 1)
            {
                data = StringToByte("302");
                SendMessage(tcp, data);
                Program.log.WriteLine("Задача не найдена: " + idT.ToString(), true);
                return;
            }

            string tmp;
            sql = (object[])sql[0]; tmp = sql[0].ToString();
            data = StringToByte("100");
            data = ByteAdd(data, StringToByte(sql[0].ToString()));
            data = ByteAdd(data, StringToByte(sql[1].ToString()));
            data = ByteAdd(data, StringToByte(((long)sql[2]).ToString()));
            data = ByteAdd(data, StringToByte(((long)sql[3]).ToString()));
            byte[] tmpB = sql[4] == DBNull.Value ? StringToByte("0") : StringToByte(((long)sql[3]).ToString());

            sql = SQL.getData("SELECT Id FROM Files WHERE Task=\'" + idT.ToString() + "\'");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));

            sql = SQL.getData("SELECT UserName FROM Tasks_Users WHERE Task=\'" + idT.ToString() + "\'");
            data = ByteAdd(data, StringToByte(sql.Length.ToString()));

            data = ByteAdd(data, tmpB);

            SendMessage(tcp, data);

            sw.Stop();
            Program.log.WriteLine("Сведения о задаче: " + idT.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Task_Add(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data); data = ByteCut(data);
            string desc = ByteToString(data); data = ByteCut(data);
            long ds = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long df = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long id = DateTime.Now.Ticks;

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object sql = SQL.getData("INSERT INTO Tasks VALUES(\'" + id.ToString() + "\', NULL, \'" + name + "\', \'" + desc + "\', \'" + ds.ToString() + "\', \'" + df.ToString() + "\')");
            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось добавить задачу", true);
            }
            else
            {
                sql = SQL.getData("INSERT INTO Tasks_Users VALUES(\'" + id.ToString() + "\', \'" + idU.ToString() + "\')");
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

            sw.Stop();
            Program.log.WriteLine("Новая задача: " + id.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Task_Edit(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long taskID = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data); data = ByteCut(data);
            string desc = ByteToString(data); data = ByteCut(data);
            long ds = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long df = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object sql = SQL.getData("UPDATE Tasks SET Name=\'" + name + "\', Description=\'" + desc + "\', DateStart=\'" + ds.ToString() + "\', DateEnd=\'" + df.ToString() + "\' WHERE DateAdd=\'" + taskID.ToString() + "\'");

            if (sql == null)
            {
                data = StringToByte("300");
                Program.log.WriteLine("Не удалось обновить задачу: " + taskID.ToString(), true);
            }
            else
            {
                data = StringToByte("100");
            }
            SendMessage(tcp, data);

            sw.Stop();
            Program.log.WriteLine("Изменение задачи: " + taskID.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Task_Do(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

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

            sw.Stop();
            Program.log.WriteLine("Задача выполнена: " + idT.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Task_List_Plan(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks1 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks2 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            DateTime dt1, dt2;
            dt1 = new DateTime(ticks1);
            dt2 = new DateTime(ticks2);

            object[] sql = SQL.getData("SELECT Tasks.DateAdd, Tasks.Description, Tasks.DateStart, Tasks.DateEnd FROM Tasks_Users INNER JOIN Tasks ON Tasks_Users.Task=Tasks.DateAdd WHERE Tasks_Users.UserName=\'" + idU.ToString() + "\' AND ((Tasks.DateStart>=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateStart<=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateEnd >=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd <=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateStart <=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd >=\'" + dt2.Ticks.ToString() + "\')) AND Tasks.DateAdd<=\'" + dt1.Ticks.ToString() + "\' ORDER BY Tasks.DateStart");

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
            }

            SendMessage(tcp, data);

            sw.Stop();
            Program.log.WriteLine("Формирование плана: " + idU.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Task_List_Report(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks1 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long ticks2 = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            DateTime dt1, dt2;
            dt1 = new DateTime(ticks1);
            dt2 = new DateTime(ticks2);

            object[] sql = SQL.getData("SELECT Tasks.DateAdd, Tasks.DateFinish, Tasks.Description, Tasks.DateStart, Tasks.DateEnd FROM Tasks_Users INNER JOIN Tasks ON Tasks_Users.Task=Tasks.DateAdd WHERE Tasks_Users.UserName=\'" + idU.ToString() + "\' AND ((Tasks.DateStart>=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateStart<=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateEnd >=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd <=\'" + dt2.Ticks.ToString() + "\') OR (Tasks.DateStart <=\'" + dt1.Ticks.ToString() + "\' AND Tasks.DateEnd >=\'" + dt2.Ticks.ToString() + "\')) ORDER BY Tasks.DateStart");

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
            }

            SendMessage(tcp, data);

            sw.Stop();
            Program.log.WriteLine("Формирование отчета: " + idU.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        #endregion

        #region Steps
        private void Step_List(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Список шагов: " + idT.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Step_Info(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Сведения о шаге: " + idS.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Step_Add(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

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

            sw.Stop();
            Program.log.WriteLine("Добавление шага к задаче: " + idT.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Step_Edit(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idS = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string name = ByteToString(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

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

            sw.Stop();
            Program.log.WriteLine("Изменение шага: " + idS.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Step_Do(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Выполнение шага: " + idS.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        #endregion

        #region Messages
        private void Message_List(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Список сообщений: " + idT.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Message_Add(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            string message = ByteToString(data);

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

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

            sw.Stop();
            Program.log.WriteLine("Добавление сообщения к задаче: " + idT.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        #endregion

        #region Cooperation
        private void Coop_List(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Список сорудников: " + idT.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Coop_Filter(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

            sw.Stop();
            Program.log.WriteLine("Список сотрудников: " + idT.ToString() + " с фильтром \"" + filter + "\" - " + sql.Length.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Coop_Add(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int idA = Convert.ToInt32(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

            object sql = SQL.getData("INSERT INTO Tasks_Users VALUES(\'" + idT.ToString() + "\', \'" + idA.ToString() + "\')");

            if (sql == null)
            {
                SendMessage(tcp, StringToByte("300"));
                Program.log.WriteLine("Не удалось связать сотрудника с задачей: " + idT.ToString() + " - " + idA.ToString(), true);
            }
            else
            {
                SendMessage(tcp, StringToByte("100"));
            }

            sw.Stop();
            Program.log.WriteLine("Установлена связь: " + idT.ToString() + " - " + idA.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        private void Coop_Delete(TcpClient tcp, byte[] data)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            long idC= Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            long idT = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
            int idD = Convert.ToInt32(ByteToString(data));

            int idU = Auth_Check(idC);
            if (Auth_SendError(tcp, idU))
                return;

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

            sw.Stop();
            Program.log.WriteLine("Удаление связи: " + idT.ToString() + " - " + idD.ToString() + "   [" + sw.ElapsedMilliseconds.ToString() + "]", false);
        }
        #endregion
    }
}