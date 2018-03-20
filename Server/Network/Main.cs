using System;
using System.Net.Sockets;
using System.Threading;

namespace Tasks_Server
{
    partial class Network
    {
        private Thread Main = null;
        private TcpListener listener;

        public Network()
        {
            Main = new Thread(ThreadFunc);
            t_AuthTimer = new Thread(Auth_Timer);
        }
        public void Start()
        {
            Main.Start();
            t_AuthTimer.Start();
        }
        public void Abort()
        {
            try
            {
                listener.Stop();
                Main.Abort();
                t_AuthTimer.Abort();
            }
            catch { }
            while (Main.ThreadState != ThreadState.Aborted && t_AuthTimer.ThreadState != ThreadState.Aborted)
                Thread.Sleep(100);
        }

        private void ThreadFunc()
        {
            while(true)
            {
                try
                {
                    listener = new TcpListener(Program.Connect_Port);
                    listener.Start();
                    while(true)
                        new Thread(ConnectionFunc).Start((object)listener.AcceptTcpClient());
                }
                catch (Exception ex)
                {
                    Program.log.WriteLine(ex.Message, true);
                    Thread.Sleep(10000);
                }
            }
        }
        private void ConnectionFunc(object obj)
        {
            try
            {
                TcpClient tcp = (TcpClient)obj;

                byte[] buf = new byte[4];
                tcp.Client.Receive(buf, 4, SocketFlags.None);

                byte[] com = new byte[2];
                tcp.Client.Receive(com, 2, SocketFlags.None);

                int len = BitConverter.ToInt32(buf, 0);
                buf = new byte[len];
                int n = 0;
                while (n != len)
                    n += tcp.Client.Receive(buf, n, len - n, SocketFlags.None);

                switch (com[0])
                {
                    case 0:
                        #region Authentification
                        switch (com[1])
                        {
                            case 0:
                                User_Auth(tcp, buf);
                                break;
                            case 1:
                                User_Password(tcp, buf);
                                break;
                            case 2:
                                User_Info(tcp, buf);
                                break;
                            case 3:
                                User_Exit(tcp, buf);
                                break;
                        }
                        #endregion
                        break;
                    case 1:
                        #region Manager
                        switch(com[1])
                        {
                            case 0:
                                Manager_List(tcp, buf);
                                break;
                            case 1:
                                Manager_Change(tcp, buf);
                                break;
                        }
                        #endregion
                        break;
                    case 2:
                        #region Tasks
                        switch (com[1])
                        {
                            case 0:
                                Task_List(tcp, buf);
                                break;
                            case 1:
                                Task_Info(tcp, buf);
                                break;
                            case 2:
                                Task_Add(tcp, buf);
                                break;
                            case 3:
                                Task_Edit(tcp, buf);
                                break;
                            case 4:
                                Task_Do(tcp, buf);
                                break;
                            case 5:
                                Task_List_Plan(tcp, buf);
                                break;
                            case 6:
                                Task_List_Report(tcp, buf);
                                break;
                        }
                        #endregion
                        break;
                    case 3:
                        #region Steps
                        switch (com[1])
                        {
                            case 0:
                                Step_List(tcp, buf);
                                break;
                            case 1:
                                Step_Info(tcp, buf);
                                break;
                            case 2:
                                Step_Add(tcp, buf);
                                break;
                            case 3:
                                Step_Edit(tcp, buf);
                                break;
                            case 4:
                                Step_Do(tcp, buf);
                                break;
                        }
                        #endregion
                        break;
                    case 4:
                        #region Messages
                        switch (com[1])
                        {
                            case 0:
                                Message_List(tcp, buf);
                                break;
                            case 1:
                                Message_Add(tcp, buf);
                                break;
                        }
                        #endregion
                        break;
                    case 5:
                        #region Cooperation
                        switch (com[1])
                        {
                            case 0:
                                Coop_List(tcp, buf);
                                break;
                            case 1:
                                Coop_Filter(tcp, buf);
                                break;
                            case 2:
                                Coop_Add(tcp, buf);
                                break;
                            case 3:
                                Coop_Delete(tcp, buf);
                                break;
                        }
                        #endregion
                        break;
                }


                tcp.Close();
            }
            catch (Exception ex)
            {
                Program.log.WriteLine(ex.Message + "\r\n" + ex.Source + "\r\n" + ex.StackTrace, true);
            }
        }

        static private bool SendMessage(TcpClient tcp, byte[] data)
        {
            try
            {
                tcp.SendTimeout = 10000;
                byte[] len = BitConverter.GetBytes(data.Length);
                byte[] result = new byte[len.Length + data.Length];
                len.CopyTo(result, 0);
                data.CopyTo(result, 4);
                tcp.Client.Send(result, result.Length, SocketFlags.None);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}