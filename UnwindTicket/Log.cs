using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace UnwindTicket
{
    public partial class Log : Form
    {
        public Log()
        {
            InitializeComponent();
        }

        private void Log_Load(object sender, EventArgs e)
        {
            try
            {
               // string LocalHistoryFilePath = ConfigurationManager.AppSettings["LogPath"];
                string LocalHistoryFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\Log";
                if (!System.IO.Directory.Exists(LocalHistoryFilePath))
                    System.IO.Directory.CreateDirectory(LocalHistoryFilePath);

                System.IO.StreamReader sr = new System.IO.StreamReader(LocalHistoryFilePath + @"\" +  DateTime.Today.Date.ToString("ddMMyyyy") + ".log", true);
               
               string log = sr.ReadToEnd();
                txtLog.Text = log;
                sr.Close();
                sr = null;

                try
                {
                    Logger.SendMail(log, "Unwind Blotter Ideas Log", "");
                }
                catch { }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
