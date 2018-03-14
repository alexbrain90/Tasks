using System;

namespace Tasks_Server
{
    static partial class Program
    {
        static public string AppExecutable;
        static public string CurrentVersion;
        static public string ServerVersion;
        static public string ServerVersionInfo;

        static public int Connect_Port;

        static public string MainConnectionString = "";
        static public Ini ConfigFile = new Ini("config.ini");

        static public int user_ID, user_Manager, user_Main;
        static public string user_Name, user_Fio, user_Sbe, user_Post;

        static public bool ReadValues()
        {
            // Get database server info
            string srvName = ConfigFile.Read("Database", "Name");
            string srvPort = ConfigFile.Read("Database", "Port");
            string dbName = ConfigFile.Read("Database", "DataBase");
            string dbUser = ConfigFile.Read("Database", "UserName");
            string dbPass = ConfigFile.Read("Database", "Password");

            // Check database server info
            if (srvName == "" || srvPort == "" || dbName == "" || dbUser == "" || dbPass == "")
                return false;
            // Create connection string
            MainConnectionString = "Server=" + srvName + "," + srvPort + ";Database=" + dbName + ";User ID=" + dbUser + ";Password=" + dbPass + ";";

            string tmp = ConfigFile.Read("Server", "Port");
            if (tmp == "")
                Connect_Port = 1434;
            else
                Connect_Port = Convert.ToInt32(tmp);

            return true;
        }
    }
}
