using System;
using System.Net;
using System.Net.Sockets;

namespace Tasks
{
    static class Network
    {
        static public bool flag_Reconnect = false;

        static private bool SendMessage(TcpClient tcp, byte[] type, byte[] data)
        {
            try
            {
                tcp.SendTimeout = 15000;
                byte[] len = BitConverter.GetBytes(data.Length);
                byte[] result = new byte[len.Length + type.Length + data.Length];
                len.CopyTo(result, 0);
                type.CopyTo(result, 4);
                data.CopyTo(result, 6);
                tcp.Client.Send(result, result.Length, SocketFlags.None);
                return true;
            }
            catch
            {
                return false;
            }
        }
        static private byte[] RecieveMessage(TcpClient tcp)
        {
            try
            {
                tcp.ReceiveTimeout = 15000;
                byte[] data = new byte[4];
                tcp.Client.Receive(data, 4, SocketFlags.None);
                data = new byte[BitConverter.ToInt32(data, 0)];
                int n = 0;
                while(n != data.Length)
                    n += tcp.Client.Receive(data, n, data.Length - n, SocketFlags.None);
                return data;
            }
            catch
            {
                return new byte[0];
            }
        }
        static public bool SendFile(string fileName)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.UploadFile("http://" + Config.ServerName + ":8080/Tasks/", fileName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Tools
        static private byte[] StringToByte(string text)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] result = new byte[data.Length + 4];
            BitConverter.GetBytes(data.Length).CopyTo(result, 0);
            data.CopyTo(result, 4);

            return result;
        }
        static private string ByteToString(byte[] data)
        {
            int len = BitConverter.ToInt32(data, 0);
            return System.Text.Encoding.UTF8.GetString(data, 4, len);
        }

        static private byte[] ByteCut(byte[] data, int start)
        {
            byte[] result = new byte[data.Length - start];
            for (int i = start; i < data.Length; i++)
                result[i - start] = data[i];
            return result;
        }
        static private byte[] ByteCut(byte[] data)
        {
            data = ByteCut(data, BitConverter.ToInt32(data, 0) + 4);
            return data;
        }
        static private byte[] ByteAdd(byte[] list1, byte[] list2)
        {
            byte[] result = new byte[list1.Length + list2.Length];
            list1.CopyTo(result, 0);
            list2.CopyTo(result, list1.Length);
            return result;
        }

        static private string HashToString(byte[] data)
        {
            string result = "";
            for (int i = 0; i < data.Length; i++)
                result += data[i].ToString("X2");
            return result;
        }
        static private byte[] HashToByte(string text)
        {
            return StringToByte(HashToString(new System.Security.Cryptography.SHA1CryptoServiceProvider().ComputeHash(StringToByte(text))));
        }
        #endregion

        #region Authentification
        /// <summary>
        /// Authentification on server with selected username and password.
        /// Returns: -1 - error; 0 - wrong username or password; 1 - succesfull; 2 - must change password; 3 - must update application.
        /// </summary>
        /// <param name="Name">Username</param>
        /// <param name="Password">Password</param>
        /// <returns>Message code</returns>
        static public int User_Auth(string Name, string Password)
        {
            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Name);
                data = ByteAdd(data, HashToByte(Password));
                data = ByteAdd(data, StringToByte(Config.CurrentVersion));

                SendMessage(tcp, new byte[] { 0, 0 }, data);

                data = RecieveMessage(tcp);

                string result = ByteToString(data); data = ByteCut(data);
                if (result == "200" || result == "202")
                {
                    Tray.SetStatusNormal();
                    Config.UserConID = Convert.ToInt64(ByteToString(data)); data = ByteCut(data);
                    Config.user_ID = Convert.ToInt32(ByteToString(data));
                    Config.user_IDMain = Config.user_ID;
                    if (result == "200")
                        return 1;
                    else
                        return 2;
                }
                else if (result == "204")
                    return 0;
                else if (result == "Update" || result == "203")
                    return 3;

                return -1;
            }
            catch
            {
                return -1;
            }
        }
        static public bool User_Info()
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                SendMessage(tcp, new byte[] { 0, 2 }, StringToByte(Config.UserConID.ToString()));

                byte[] data = RecieveMessage(tcp);

                if (ByteToString(data) == "204")
                    return result;
                else if (ByteToString(data) == "201")
                {
                    Config.UserConID = -1;
                    return result;
                }
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    Config.user_Fio = ByteToString(data); data = ByteCut(data);
                    Config.user_Sbe = ByteToString(data); data = ByteCut(data);
                    Config.user_Post = ByteToString(data); data = ByteCut(data);
                    string tmp = ByteToString(data);
                    if (tmp == "true")
                        Config.user_Manager = true;
                    else
                        Config.user_Manager = false;
                    result = true;
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool User_Password(string oldP, string newP)
        {
            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, HashToByte(oldP));
                data = ByteAdd(data, HashToByte(newP));

                SendMessage(tcp, new byte[] { 0, 1 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "100")
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        static public void User_Exit()
        {
            if (Threads.Connection.Connected == false)
                return;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                SendMessage(tcp, new byte[] { 0, 3 }, StringToByte(Config.UserConID.ToString()));

                byte[] data = RecieveMessage(tcp);
            }
            catch { }
        }
        #endregion

        #region Tasks
        /// <summary>
        /// Получение списка задач
        /// </summary>
        /// <param name="status">Фильтр задач по статусу</param>
        /// <param name="filterB">Начало интервала фильтра по дате</param>
        /// <param name="filterE">Конец интервала фильтра по дате</param>
        /// <param name="sortId">Сортировка</param>
        /// <returns></returns>
        static public string[,] Task_List(int status, long filterB, long filterE, int sortId)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(status.ToString()));
                data = ByteAdd(data, StringToByte(filterB.ToString()));
                data = ByteAdd(data, StringToByte(filterE.ToString()));
                data = ByteAdd(data, StringToByte(sortId.ToString()));

                SendMessage(tcp, new byte[] { 2, 0 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);

                    int n = Convert.ToInt32(ByteToString(data));
                    data = ByteCut(data);

                    result = GetList(n, 10, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public string[] Task_Info(long id)
        {
            string[] result = new string[0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));

                SendMessage(tcp, new byte[] { 2, 1 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    result = GetList(13, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public long Task_Add(string name, string desc, long ds, long df, int type, string dire)
        {
            long result = -1;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(name));
                data = ByteAdd(data, StringToByte(desc));
                data = ByteAdd(data, StringToByte(ds.ToString()));
                data = ByteAdd(data, StringToByte(df.ToString()));
                data = ByteAdd(data, StringToByte(type.ToString()));
                data = ByteAdd(data, StringToByte(dire.ToString()));

                SendMessage(tcp, new byte[] { 2, 2 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    result = Convert.ToInt64(ByteToString(data));
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Task_Edit(long taskID, string name, string desc, long ds, long df, int type, string dire)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(taskID.ToString()));
                data = ByteAdd(data, StringToByte(name));
                data = ByteAdd(data, StringToByte(desc));
                data = ByteAdd(data, StringToByte(ds.ToString()));
                data = ByteAdd(data, StringToByte(df.ToString()));
                data = ByteAdd(data, StringToByte(type.ToString()));
                data = ByteAdd(data, StringToByte(dire.ToString()));

                SendMessage(tcp, new byte[] { 2, 3 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        /// <summary>
        /// Выполнение задачи
        /// </summary>
        /// <param name="id">ИД задачи</param>
        /// <returns>true - задача успешно выполнена, в противном случае - false</returns>
        static public bool Task_Do(long id)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                SendMessage(tcp, new byte[] { 2, 4 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Task_UnDo(long id)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                SendMessage(tcp, new byte[] { 2, 7 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Task_Delete(long id)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                SendMessage(tcp, new byte[] { 2, 8 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Task_UnDelete(long id)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                SendMessage(tcp, new byte[] { 2, 9 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public string[,] Task_List_Plan(long date1, long date2)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(date1.ToString()));
                data = ByteAdd(data, StringToByte(date2.ToString()));

                SendMessage(tcp, new byte[] { 2, 5 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data));
                    data = ByteCut(data);

                    result = GetList(n, 4, data);
                }
                else if (ByteToString(data) == "201")
                    Config.UserConID = -1;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public string[,] Task_List_Report(long date1, long date2)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(date1.ToString()));
                data = ByteAdd(data, StringToByte(date2.ToString()));

                SendMessage(tcp, new byte[] { 2, 6 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data));
                    data = ByteCut(data);

                    result = GetList(n, 10, data);
                }
                if (ByteToString(data) == "210")
                    Config.UserConID = -1;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        #endregion

        #region Manager
        static public string[,] Manager_List()
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);

                SendMessage(tcp, new byte[] { 1, 0 }, StringToByte(Config.UserConID.ToString()));
                byte[] data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);

                    int n = Convert.ToInt32(ByteToString(data));
                    data = ByteCut(data);

                    result = GetList(n, 3, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Manager_Change(int UserID)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);

                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(Config.user_ID.ToString()));
                data = ByteAdd(data, StringToByte(UserID.ToString()));
                SendMessage(tcp, new byte[] { 1, 1 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        #endregion

        #region Steps
        static public string[,] Step_List(long id)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));

                SendMessage(tcp, new byte[] { 3, 0 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data));
                    data = ByteCut(data);

                    result = GetList(n, 3, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public string Step_Info(long id)
        {
            string result = "";

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                SendMessage(tcp, new byte[] { 3, 1 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    result = ByteToString(data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        /// <summary>
        /// Добавление шага
        /// </summary>
        /// <param name="id">ИД задачи</param>
        /// <param name="name">Наименование шага</param>
        /// <returns>true - шаг успешно добавлена, в противном случае - false</returns>
        static public bool Step_Add(long id, string name)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                data = ByteAdd(data, StringToByte(name));
                SendMessage(tcp, new byte[] { 3, 2 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        /// <summary>
        /// Изменение шага
        /// </summary>
        /// <param name="id">ИД шага</param>
        /// <param name="name">Наименование шага</param>
        /// <returns>true - шаг успешно обновлена, в противном случае - false</returns>
        static public bool Step_Edit(long id, string name)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                data = ByteAdd(data, StringToByte(name));
                SendMessage(tcp, new byte[] { 3, 3 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        /// <summary>
        /// Выполнение шага
        /// </summary>
        /// <param name="id">ИД шага</param>
        /// <returns>true - шаг успешно выполнен, в противном случае - false</returns>
        static public bool Step_Do(long id)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                SendMessage(tcp, new byte[] { 3, 4 }, data);

                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        #endregion

        #region Messages
        static public string[,] Message_List(long id)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));

                SendMessage(tcp, new byte[] { 4, 0 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
                    result = GetList(n, 3, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Message_Add(long id, string message)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                data = ByteAdd(data, StringToByte(message));
                SendMessage(tcp, new byte[] { 4, 1 }, data);

                data = RecieveMessage(tcp);
                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        #endregion

        #region Cooperation
        static public string[,] Coop_List(long id)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                SendMessage(tcp, new byte[] { 5, 0 }, data);

                data = RecieveMessage(tcp);
                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
                    result = GetList(n, 2, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public string[,] Coop_Filter(long id, string filter)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(id.ToString()));
                data = ByteAdd(data, StringToByte(filter));
                SendMessage(tcp, new byte[] { 5, 1 }, data);

                data = RecieveMessage(tcp);
                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
                    result = GetList(n, 2, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Coop_Add(long idT, int idU)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(idT.ToString()));
                data = ByteAdd(data, StringToByte(idU.ToString()));
                SendMessage(tcp, new byte[] { 5, 2 }, data);

                data = RecieveMessage(tcp);
                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Coop_Delete(long idT, int idU)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(idT.ToString()));
                data = ByteAdd(data, StringToByte(idU.ToString()));
                SendMessage(tcp, new byte[] { 5, 3 }, data);

                data = RecieveMessage(tcp);
                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        #endregion

        #region Events
        static public string[,] Event_List()
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());

                SendMessage(tcp, new byte[] { 6, 0 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
                    result = GetList(n, 5, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        static public bool Event_Delete(long idT)
        {
            bool result = false;

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(idT.ToString()));
                SendMessage(tcp, new byte[] { 6, 2 }, data);

                data = RecieveMessage(tcp);
                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                    result = true;
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        #endregion

        #region Administration
        static public string[,] Admin_User(int type)
        {
            string[,] result = new string[0, 0];

            if (Threads.Connection.Connected == false)
                return result;

            try
            {
                TcpClient tcp = new TcpClient(Config.ServerName, Config.ServerPort);
                byte[] data = StringToByte(Config.UserConID.ToString());
                data = ByteAdd(data, StringToByte(type.ToString()));

                SendMessage(tcp, new byte[] { 255, 0 }, data);
                data = RecieveMessage(tcp);

                if (ByteToString(data) == "201")
                    Config.UserConID = -1;
                else if (ByteToString(data) == "100")
                {
                    data = ByteCut(data);
                    int n = Convert.ToInt32(ByteToString(data)); data = ByteCut(data);
                    result = GetList(n, 4, data);
                }
            }
            catch
            {
                Config.UserConID = -1;
            }

            return result;
        }
        #endregion

        static private string[] GetList(int x, byte[] data)
        {
            string[] result = new string[x];
            for (int i = 0; i < x; i++)
            {
                result[i] = ByteToString(data);
                data = ByteCut(data);
            }
            return result;
        }
        static private string[,] GetList(int x, int y, byte[] data)
        {
            string[,] result = new string[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    result[i, j] = ByteToString(data);
                    data = ByteCut(data);
                }
            }
            return result;
        }
    }
}