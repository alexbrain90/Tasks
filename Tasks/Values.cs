using System;

namespace Tasks
{
    static partial class Program
    {
        static public string AppExecutable;
        static public string CurrentVersion;
        static public string ServerVersion;
        static public string ServerVersionInfo;

        static public string ServerName = "194.154.82.6";
        static public int ServerPort = 1434;
        static public Ini ConfigFile = new Ini("config.ini");

        static public int user_ID, user_IDMain;
        static public long user_ConID;
        static public string user_Name, user_Pass, user_NameMain, user_Fio, user_Sbe, user_Post;
        static public bool user_Manager;
        static public bool flag_ReadOnly = false;
    }
}
