using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UnwindTicket
{
    public partial class IntradayDataSend : Form
    {
        DateTime DownloadDate;

        public IntradayDataSend()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Download in progress";
                DownloadDate = dtDownloadDate.Value;
                UnwindTicket.DAL.BloombergHistoryData obj = new DAL.BloombergHistoryData();
                obj.StartRequestDateTime = Convert.ToDateTime(DownloadDate.ToString("dd-MMM-yyyy 00:00:00"));
                obj.EndRequestDateTime = Convert.ToDateTime(DownloadDate.ToString("dd-MMM-yyyy 23:59:59"));
                obj.ProcessHistoryRequest();
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "btnIntradayData_Click " + ex.Message + "\t" + ex.StackTrace);
            }
            
         
        }

        private void IntradayDataSend_Load(object sender, EventArgs e)
        {

            switch (dtDownloadDate.Value.ToString("dddd").ToUpper())
            {
                case "SUNDAY":
                    dtDownloadDate.Value = DateTime.Now.AddDays(-2);
                    break;
                case "MONDAY":
                    dtDownloadDate.Value = DateTime.Now.AddDays(-3);
                    break;
                 default:
                    dtDownloadDate.Value = DateTime.Now.AddDays(-1);
                    break;
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}



//List<string> BBRequest = new List<string>();
//string[] SectorIndex = ConfigurationManager.AppSettings["SectorIndex"].ToString().Split(',');
//string[] Tickers = ConfigurationManager.AppSettings["Tickers"].ToString().Split(',');
//foreach (string sector in SectorIndex)
//    BBRequest.Add(sector.ToUpper() + " INDEX");

//foreach (string ticker in Tickers)
//    BBRequest.Add(ticker.ToUpper() + " EQUITY");