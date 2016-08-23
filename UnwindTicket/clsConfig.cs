using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;



namespace UnwindTicket
{
    public static class clsConfig
    {
        public static string URL;
        public static string APIKey;
        public static string UserName;
        public static string Password;

        public static void LoadConfig()
        {
         try
            {
                clsConfig.URL = ConfigurationManager.AppSettings["BaseAPIURL"];
                clsConfig.APIKey = ConfigurationManager.AppSettings["BaseAPIKey"];
                clsConfig.UserName = ConfigurationManager.AppSettings["userName"];
                clsConfig.Password = ConfigurationManager.AppSettings["password"];

                string path = GetSaveFolderPath();
                if (path != "")
                {
                    path += "\\UTconfig.txt";
                    if (System.IO.File.Exists(path))
                    {
                        string info = System.IO.File.ReadAllText(path);
                        string[] para = info.Split(';');
                        clsConfig.URL = para[0].Split('|')[1];
                        clsConfig.UserName = para[1].Split('|')[1];
                        clsConfig.Password = para[2].Split('|')[1];
                        clsConfig.APIKey = para[3].Split('|')[1];
                    }
                }
                else
                {
                    Logger.LogEntry("Error", "LoadConfig - path does not found , config value is in process");
                }

            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "Configuration_Load " + ex.Message + "\t" + ex.StackTrace);
            }
        }
        public static string GetSaveFolderPath()
        {
            try
            {
                string strUserProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (Directory.Exists(strUserProfilePath + "\\UnwindTicket") == false)
                {
                    Directory.CreateDirectory(strUserProfilePath + "\\UnwindTicket");
                }
                return strUserProfilePath + "\\UnwindTicket";
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "GetSaveFolderPath " + ex.Message + "\t" + ex.StackTrace);
                return "";
            }
        }

    }
}
