using System;
using Microsoft.Win32;

namespace Tasks
{
    static class Config
    {
        static public string AppExecutable;
        static public string CurrentVersion;
        static public string ServerVersion;
        static public string ServerVersionInfo;

        static public string ServerName = "194.154.82.6";
        static public int ServerPort = 1434;
        static public Ini ConfigFile = new Ini("config.ini");

        static public int user_ID, user_IDMain;
        static private long _userConID; static public long UserConID
        {
            get
            {
                return _userConID;
            }
            set
            {
                if (value == -1)
                {
                    try
                    {
                        Tray.SetStatusError();
                    }
                    catch { }
                }
                _userConID = value;
            }
        }
        static public string user_Name, user_Pass, user_NameMain, user_Fio, user_Sbe, user_Post;
        static public bool user_Manager;
        static public bool flag_ReadOnly = false;

        static public bool global_AutoStart = true;
        static public bool login_Auto = false;
        
        static public System.Drawing.Font fort_Main = new System.Drawing.Font("Arial", 10);

        static public int form_Main_LocationX, form_Main_LocationY;
        static public int form_Main_SizeX, form_Main_SizeY;
        static public bool form_Main_Maximized;
        static public int form_Main_FilterDate, form_Main_FilderStatus, form_Main_Sort;

        #region Read and Write
        static public void ReadConfig()
        {
            Config.UserConID = -1;

            user_Name = ConfigFile.Read("User", "Name"); user_NameMain = user_Name;
            user_Pass = ConfigFile.Read("User", "Password");
            login_Auto = ConfigFile.Read("Global", "AutoLogin") == "true" ? true : false;
        }
        static public void ApplyConfig()
        {
            if (global_AutoStart == true)
            {
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true).SetValue("Tasks", "\"" + Config.AppExecutable + "\"", RegistryValueKind.String);
            }
        }
        static public void WriteConfig()
        {
            ConfigFile.Write("Global", "AutoLogin", login_Auto.ToString().ToLower());
            ConfigFile.Write("User", "Name", user_NameMain);
            ConfigFile.Write("User", "Password", user_Pass);

            ConfigFile.Save();
        }
        #endregion
    }
}