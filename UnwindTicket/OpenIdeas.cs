using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bloomberglp.Blpapi;
using System.Threading;
using UnwindTicket.Entity;
using UnwindTicket.DAL;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;

namespace UnwindTicket
{
    public partial class OpenIdeas : Form
    {
        public OpenIdeas()
        {
            InitializeComponent();
        }

        string mrktSymbol = "VGA Index";
        //bool BBDataAvailable = false; // confirms that fully data available
        bool BBSubscriptionConnected = false;
        DateTime LastRefresh = DateTime.Now;
        private delegate void dlgRefreshUI(bool IsOtasConnected);
        List<BlotterLiveIdea> lstLiveIdeas;

        #region  Form Event

        private void OpenIdeas_Load(object sender, EventArgs e)
        {
            try
            {
                clsConfig.LoadConfig();
                tmrRefresh.Interval = (60 * 2000); //2 Min.
                tmrRefresh.Enabled = true;
                tmrRefresh.Start();
                Refresh();
            }
            catch (Exception ex)
            { Logger.LogEntry("OpenIdeas_Load", ex.Message + "\t" + ex.StackTrace); }
        }

        private void OpenIdeas_Resize(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    notifySLTP.Visible = true;
                    notifySLTP.ShowBalloonTip(1000);
                    this.Hide();
                }
                else if (this.WindowState == FormWindowState.Normal || this.WindowState == FormWindowState.Maximized)
                {
                    notifySLTP.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "OpenIdeas_Resize " + ex.Message + "\t" + ex.StackTrace);
            }
        }

        #endregion

        # region Control Event
            private void notifySLTP_Click(object sender, EventArgs e)
            {
                try
                {
                    notifySLTP.Visible = false;
                    this.Show();
                    this.WindowState = FormWindowState.Maximized;
                }
                catch (Exception ex)
                {
                    Logger.LogEntry("Error", "OpenIdeas_Resize " + ex.Message + "\t" + ex.StackTrace);
                }

            }

            private void btnRefresh_Click(object sender, EventArgs e)
            {
                try
                {
                    Refresh();
                }
                catch (Exception ex)
                {
                    Logger.LogEntry("btnRefresh_Click", ex.Message + "\t" + ex.StackTrace);
                }
            }

            private void btnLog_Click(object sender, EventArgs e)
            {
                try
                {
                    Log obj = new Log();
                    obj.ShowDialog();
                    obj = null;
                }
                catch (Exception ex)
                {
                    Logger.LogEntry("Information", "btnLog_Click: " + ex.Message + "\t" + ex.StackTrace);
                }
            }

            private void tmrRefresh_Tick(object sender, EventArgs e)
            {
                try
                {
                     if (LastRefresh.Date != DateTime.Now.Date)  // overnight refresh call
                    {
                        Refresh();
                        Logger.LogEntry("Information", "Refreshed Overnight - " + LastRefresh);
                        LastRefresh = DateTime.Now.Date;

                        /**************************Send BB data to Intraday server****************************************************/
                        DateTime DownloadDate = DateTime.Now.Date.AddDays(-1);
                        Logger.LogEntry("Information", "Spain and Sector data download start - " + DownloadDate.ToString());
                        UnwindTicket.DAL.BloombergHistoryData obj = new DAL.BloombergHistoryData();
                        if (DownloadDate.ToString("dddd").ToUpper() != "SUNDAY" && DownloadDate.ToString("dddd").ToUpper() != "SATURDAY") // not required for non tradding days
                        {
                            obj.StartRequestDateTime = Convert.ToDateTime(DownloadDate.ToString("dd-MMM-yyyy 00:00:00"));
                            obj.EndRequestDateTime = Convert.ToDateTime(DownloadDate.ToString("dd-MMM-yyyy 23:59:59"));
                            obj.ProcessHistoryRequest();
                        }
                    }

                    if (!BBSubscriptionConnected) //if disconnected then refresh all
                    {
                        Refresh();
                        Logger.LogEntry("Information", "Refreshed - " + BBSubscriptionConnected.ToString());
                    }

                    tmrRefresh.Interval = (60 * 2000); //2 Min.
                    tmrRefresh.Enabled = true;
                    tmrRefresh.Start();
                    Logger.LogEntry("Information", "Tick identified");

                }
                catch (Exception ex)
                {
                    Logger.LogEntry("Error", "tmrRefresh_Tick " + ex.Message + "\t" + ex.StackTrace);
                }

            }

            private void mnuVersion_Click(object sender, EventArgs e)
            {
                try
                {
                    MessageBox.Show("Unwind Ticket Version: " + Application.ProductVersion.ToString());
                }
                catch (Exception ex)
                {
                    Logger.LogEntry("Error", "mnuVersion_Click " + ex.Message + "\t" + ex.StackTrace);
                }

            }
        #endregion

        #region Methods
            public override void Refresh()
            {
                try
                {
                    RawData();
                    FormatGrid();
                    bolSubscriptionRunning = false;
                    string[] tickers = lstLiveIdeas.Select(a => a.Ticker + " Equity").Distinct().ToArray();
                    Array.Resize(ref tickers, tickers.Length + 1);
                    tickers[tickers.Length - 1] = mrktSymbol; //future ticket add
                    StartSubscription(tickers);
                    Logger.LogEntry("Information", "Ideas Refreshed");
                }
                catch (Exception ex)
                { Logger.LogEntry("Information",   "Refresh: " + ex.Message + "\t" + ex.StackTrace); }
            }

                private void RawData()
        {
            try
            {
                lstLiveIdeas = new List<BlotterLiveIdea>();
                lstLiveIdeas = DAL.DataFetching.getOpenIdeaList(); // Read recent ideas
            }
            catch (Exception ex)
            { Logger.LogEntry("Error", "RawData " + ex.Message + "\t" + ex.StackTrace); }
        }

                private void FormatGrid()
                {
                    try
                    {
                        grvLiveIdea.DataSource = lstLiveIdeas;
                        grvLiveIdea.ReadOnly = true;
                        grvLiveIdea.AutoSize = true;
                        grvLiveIdea.ScrollBars = ScrollBars.Both;

                        grvLiveIdea.Columns["EVENTID"].Visible = false;
                        grvLiveIdea.Columns["IDEAID"].Visible = false;
                        grvLiveIdea.Columns["OTICSINGESTID"].Visible = false;
                        grvLiveIdea.Columns["CLOSINGPERIOD"].Visible = false;
                        grvLiveIdea.Columns["PORTWARESTRATEGYID"].Visible = false;
                        

                        grvLiveIdea.Columns["RowNo"].Width = 30;
                        grvLiveIdea.Columns["RowNo"].HeaderText = "#";
               
                        grvLiveIdea.Columns["Company"].Width = 130;
                        grvLiveIdea.Columns["Author"].Width = 140;
                        grvLiveIdea.Columns["TransDate"].Width = 120;
                        grvLiveIdea.Columns["Ticker"].Width = 80;


                        grvLiveIdea.Columns["Direction"].HeaderText = "Dir";
                        grvLiveIdea.Columns["Direction"].Width = 40;

                        grvLiveIdea.Columns["OpenPrice"].Width = 90;
                        grvLiveIdea.Columns["OpenPrice"].HeaderText = "Open";

                        grvLiveIdea.Columns["ClosePrice"].Width = 90;
                        grvLiveIdea.Columns["ClosePrice"].HeaderText = "Live Price";

                        grvLiveIdea.Columns["MktOpenPrice"].Width = 100;
                        grvLiveIdea.Columns["MktOpenPrice"].HeaderText = "Market Open";

                        grvLiveIdea.Columns["MktClosePrice"].Width = 100;
                        grvLiveIdea.Columns["MktClosePrice"].HeaderText = "Market Live";

                        grvLiveIdea.Columns["UnwindType"].Width = 60;
                        grvLiveIdea.Columns["UnwindType"].HeaderText = "SL/TP";

                        grvLiveIdea.Columns["UnwindValue"].Width = 80;
                        grvLiveIdea.Columns["UnwindValue"].HeaderText = "Rule (%)";
                        grvLiveIdea.Columns["Response"].Width = 56;
                        grvLiveIdea.Columns["Response"].HeaderText = "Result";
                        
                        grvLiveIdea.Columns["Unwind"].HeaderText = "Unwind(%)";
                        grvLiveIdea.Columns["OpenPrice"].DefaultCellStyle.Format = "#,0.00";
                        grvLiveIdea.Columns["ClosePrice"].DefaultCellStyle.Format = "#,0.00";
                        grvLiveIdea.Columns["MktOpenPrice"].DefaultCellStyle.Format = "#,0.00";
                        grvLiveIdea.Columns["MktClosePrice"].DefaultCellStyle.Format = "#,0.00";
                        grvLiveIdea.Columns["UnwindValue"].DefaultCellStyle.Format = "#,0.00%";
                        grvLiveIdea.Columns["Unwind"].DefaultCellStyle.Format = "#,0.00%";
                        grvLiveIdea.Refresh();
            
                    }
                    catch (Exception ex)
                    { Logger.LogEntry("Error", "FormatGrid:" + ex.Message + "\t" + ex.StackTrace); }
        }

               
            # endregion

        #region SL and TP
        bool SL_TP_Running = false;
        private string CheckSL_TP(string ticker = "")
        {
           if (SL_TP_Running) return "";
            SL_TP_Running = true;
          string result = "";
            try
            {
                
                if (ticker == mrktSymbol) ticker = "";
                lock (grvLiveIdea)
                {
                    if (ticker != "")
                    {
                    for (int i = 0; i < grvLiveIdea.RowCount; i++)
                    {
                       
                        if (grvLiveIdea.Rows[i].Cells["Ticker"].Value.ToString().ToUpper() != ticker.ToUpper()) continue;

                          if (grvLiveIdea.Rows[i].Cells["UnwindType"].Value.ToString() == "SL")
                          {

                              grvLiveIdea.Rows[i].Cells["Unwind"].Value = BAL.IdeaExitCalc.GetStopLossVsMarketExit(Convert.ToDouble(grvLiveIdea.Rows[i].Cells["OpenPrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["ClosePrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktOpenPrice"].Value.ToString()),
                                        Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktClosePrice"].Value.ToString()),
                                        grvLiveIdea.Rows[i].Cells["Direction"].Value.ToString(),
                                        Convert.ToDouble(grvLiveIdea.Rows[i].Cells["UnwindValue"].Value.ToString())); // SL  scenario
                            }
                            else if (grvLiveIdea.Rows[i].Cells["UnwindType"].Value.ToString() == "TP")
                            {
                                grvLiveIdea.Rows[i].Cells["Unwind"].Value = BAL.IdeaExitCalc.GetTakeProfitsVsMarketExit(
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["OpenPrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["ClosePrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktOpenPrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktClosePrice"].Value.ToString()),
                                        grvLiveIdea.Rows[i].Cells["Direction"].Value.ToString(),
                                        Convert.ToDouble(grvLiveIdea.Rows[i].Cells["UnwindValue"].Value.ToString())
                                    ); //  TP scenario
                                
                            }
                        }
                       
                        }
                    else
                    {
                        for (int i = 0; i < grvLiveIdea.RowCount; i++)
                        {
                            if (grvLiveIdea.Rows[i].Cells["UnwindType"].Value.ToString() == "SL")
                            {
                                 grvLiveIdea.Rows[i].Cells["Unwind"].Value = BAL.IdeaExitCalc.GetStopLossVsMarketExit(Convert.ToDouble(grvLiveIdea.Rows[i].Cells["OpenPrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["ClosePrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktOpenPrice"].Value.ToString()),
                                        Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktClosePrice"].Value.ToString()),
                                        grvLiveIdea.Rows[i].Cells["Direction"].Value.ToString(),
                                        Convert.ToDouble(grvLiveIdea.Rows[i].Cells["UnwindValue"].Value.ToString())); // SL  scenario
                                
                            }
                            else if ( grvLiveIdea.Rows[i].Cells["UnwindType"].Value.ToString() == "TP")
                            {
                                grvLiveIdea.Rows[i].Cells["Unwind"].Value = BAL.IdeaExitCalc.GetTakeProfitsVsMarketExit(
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["OpenPrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["ClosePrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktOpenPrice"].Value.ToString()),
                                       Convert.ToDouble(grvLiveIdea.Rows[i].Cells["MktClosePrice"].Value.ToString()),
                                        grvLiveIdea.Rows[i].Cells["Direction"].Value.ToString(),
                                        Convert.ToDouble(grvLiveIdea.Rows[i].Cells["UnwindValue"].Value.ToString())
                                    ); //  TP scenario
                            }
                        }

                    }

                }
             

            }
            catch (Exception ex)
            { Logger.LogEntry("Error", "CheckSL_TP " + ex.Message + "\t" + ex.StackTrace); SL_TP_Running = false; }
            finally
            {
                lock (grvLiveIdea)
                {
                    for (int i = 0; i < grvLiveIdea.RowCount; i++)
                    {
                        if (grvLiveIdea.Rows[i].Cells["Response"].Value == null && Convert.ToDouble(grvLiveIdea.Rows[i].Cells["Unwind"].Value) != 0)
                        {
                            result = "";
                            Int64 IdeaId = Convert.ToInt64(grvLiveIdea.Rows[i].Cells["IdeaId"].Value);
                            string StretegyId = grvLiveIdea.Rows[i].Cells["PortwareStrategyId"].Value.ToString();
                            double UnwindValue = Convert.ToDouble(grvLiveIdea.Rows[i].Cells["UnwindValue"].Value);
                            string UnwindType = grvLiveIdea.Rows[i].Cells["UnwindType"].Value.ToString();
                            string Comment = grvLiveIdea.Rows[i].Cells["OpenPrice"].Value.ToString() + "," +
                                             grvLiveIdea.Rows[i].Cells["ClosePrice"].Value.ToString() + "," +
                                             grvLiveIdea.Rows[i].Cells["MktOpenPrice"].Value.ToString() + "," +
                                             grvLiveIdea.Rows[i].Cells["MktClosePrice"].Value.ToString() + "," +
                                             grvLiveIdea.Rows[i].Cells["Unwind"].Value.ToString() + "," +
                                             grvLiveIdea.Rows[i].Cells["OTICSIngestId"].Value.ToString();
                            result = APIUtility.BlotterUnwindIdeaAdd(IdeaId, StretegyId, UnwindType, UnwindValue, Comment);
                            if (result.ToString().Contains("Unsuccessful"))
                                grvLiveIdea.Rows[i].Cells["Response"].Value = "Fail";
                            else
                                grvLiveIdea.Rows[i].Cells["Response"].Value = "Pass";

                            Logger.LogEntry("Information", ticker + " unwinded IdeaId:" + IdeaId + " result:" + result + "\n" + Comment, true);
                           break;
                        }
                    }
                }
                SL_TP_Running = false;
            }
            SL_TP_Running = false;
            return result.ToUpper();
        }

        //private string UnwindIdea(string ticker, Int64 IdeaId, string StretegyId, string UnwindType, double UnwindValue, string Comment)
        //{
        //    try
        //    {

        //        string result = APIUtility.BlotterUnwindIdeaAdd(IdeaId, StretegyId, UnwindType, UnwindValue, Comment);
        //        Logger.LogEntry("Information", ticker + " unwinded IdeaId:" + IdeaId + " result:" + result + "\n" + Comment, true);

        //        if (result.ToString().Contains("Unsuccessful"))
        //             return  "Fail";
        //        else
        //             return  "Pass";
                
        //    }
        //    catch(Exception ex)
        //    { Logger.LogEntry("Error", "UnwindIdea " + ex.Message + "\t" + ex.StackTrace); return ""; }
        //}

        #endregion

        #region Subscription

        #region Declaration

        Session session = default(Session);
        SessionOptions sessionOptions = new SessionOptions();
        List<Subscription> subscriptions = new List<Subscription>();
        List<CorrelationID> correlationIds = new List<CorrelationID>();
        Element referenceDataResponse = default(Element);
        //string fieldsName = "BEST_BID,BEST_ASK,BID_SIZE,ASK_SIZE,LAST_PRICE,SIZE_LAST_TRADE,TRADE_UPDATE_STAMP_RT,LAST_DIR,EVT_TRADE_SIZE_RT,EVT_TRADE_PRICE_RT,EVT_TRADE_DATE_RT,PX_OFFICIAL_AUCTION_RT,OFFICIAL_AUCTION_VOLUME_RT,TIME_AUCTION_CALL_CONCLUSION_RT,VOLUME_TDY,NUM_TRADES_RT,ON_BOOK_VOLUME_TODAY_RT,OFF_BOOK_VOLUME_TODAY_RT,MKTDATA_EVENT_TYPE,MKTDATA_EVENT_SUBTYPE";
        string fieldsName = "LAST_PRICE";
        string errFilePath = "errLog.txt";

        #endregion

        //Start subscription 
        private void StartSubscription(string[] tickers)
        {
            try
            {
                if (tickers.Length == 0)
                    return;
              
                this.correlationIds.Clear();
                this.subscriptions.Clear();
                subscriptions = new List<Subscription>();

                foreach (string ticker in tickers)
                {
                    CorrelationID correlationId = new CorrelationID(ticker);
                    if (correlationIds.Contains(correlationId) == false)
                    {
                        subscriptions.Add(new Subscription(ticker , fieldsName, "interval=3", correlationId));
                        this.correlationIds.Add(correlationId);
                    }
                }
                session = new Session(sessionOptions, new Bloomberglp.Blpapi.EventHandler(SubscriptionEvent));
                session.StartAsync();
            }
            catch (Exception ex)
            {
                if (ex.GetType().Equals(typeof(OutOfMemoryException))) { }
                else if (ex.Message.ToUpper().Contains("SYSTEM.OUTOFMEMORYEXCEPTION")) { }
                else { Logger.LogEntry("Error", "StartSubscription " + ex.Message + "\t" + ex.StackTrace); }
            }
        }
        //Subscription process event
        private void SubscriptionEvent(Event eventObj, Session session)
        {
            try
            {
                switch ((eventObj.Type))
                {
                    case Event.EventType.SESSION_STATUS:
                        foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
                        {
                            if (message.MessageType.Equals("SessionStarted"))
                            {
                                session.OpenServiceAsync("//blp/mktdata", new CorrelationID(99));
                            }
                            else if (message.MessageType.Equals("SessionConnectionDown")) { }
                            else if (message.MessageType.Equals("SessionStartupFailure"))
                            { 
                                if (this.InvokeRequired)
                                    this.Invoke(new dlgRefreshUI(RefreshUI), false);
                            }
                        }
                        break;
                    case Event.EventType.SERVICE_STATUS:
                        foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
                        {
                            if (message.CorrelationID.Value == 99 && message.MessageType.Equals("ServiceOpened"))
                            {
                                if (subscriptions.Count > 0)
                                {
                                    session.Subscribe(subscriptions);
                                }
                            }
                            else
                            {
                                if (this.InvokeRequired)
                                    this.Invoke(new dlgRefreshUI(RefreshUI), false);
                                
                                BBSubscriptionConnected = false; // disconnected
                                Logger.LogEntry("Error", "BB Disconncted");
                                break;
                            }
                        }
                        break;
                    case Event.EventType.SUBSCRIPTION_DATA:
                        BBSubscriptionConnected = true;
                        if (this.InvokeRequired)
                            this.Invoke(new dlgRefreshUI(RefreshUI), true);
                        SubscriptionResponse(eventObj);
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Equals(typeof(ArgumentOutOfRangeException))) { }
                else if (ex.Message.ToUpper().Contains("SYSTEM.ARGUMENTOUTOFRANGEEXCEPTION")) { }
                else if (ex.GetType().Equals(typeof(OutOfMemoryException))) { }
                else if (ex.Message.ToUpper().Contains("SYSTEM.OUTOFMEMORYEXCEPTION")) { }
                else { Logger.LogEntry("Error", "SubscriptionEvent " + ex.Message + "\t" + ex.StackTrace); }
            }
        }
        //subscription response data
        bool bolSubscriptionRunning = false;
        private void SubscriptionResponse(Event eventObj)
        {
            string strTicker = "";
            try
            {
                foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
                {
                    referenceDataResponse = message.AsElement;

                    if ((referenceDataResponse.HasElement("responseError")))
                    {
                        Logger.LogEntry("Error", "SubscriptionResponse:responseError ");
                        return;
                    }
                    if (bolSubscriptionRunning) break;

                    bolSubscriptionRunning = true;

                    if (message.MessageType.Equals("MarketDataEvents") == true && message.HasElement("LAST_PRICE"))
                    {
                        double recentPrice = Convert.ToDouble(message.GetElement("LAST_PRICE").GetValue());
                        strTicker = message.CorrelationID.ToString().ToUpper().Replace("EQUITY", "").ToString().Replace("USER:", "").ToString().Trim();
                        lock (grvLiveIdea)
                        {
                            for(int i=0;i < grvLiveIdea.RowCount; i++)
                            {
                                if (strTicker.ToUpper() == mrktSymbol.ToUpper())
                                {
                                    grvLiveIdea.Rows[i].Cells["MktClosePrice"].Value = recentPrice;
                                    continue;
                                }

                                if (grvLiveIdea.Rows[i].Cells["Ticker"].Value.ToString().ToUpper() == strTicker.ToUpper())
                                { grvLiveIdea.Rows[i].Cells["ClosePrice"].Value = recentPrice; }

                            }
                           
                        }
                       
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Equals(typeof(ArgumentOutOfRangeException))) { }
                else if (ex.Message.ToUpper().Contains("SYSTEM.ARGUMENTOUTOFRANGEEXCEPTION")) { }
                else if (ex.GetType().Equals(typeof(OutOfMemoryException))) { }
                else if (ex.Message.ToUpper().Contains("SYSTEM.OUTOFMEMORYEXCEPTION")) { }
                else
                { Logger.LogEntry("Error", "SubscriptionResponse " + ex.Message + "\t" + ex.StackTrace); }

            }
            finally
            {

              string result =   CheckSL_TP(strTicker); // check SL and TP
              //if (result.ToUpper().Contains("SUCCESSFUL"))
              //    Refresh();
 
               bolSubscriptionRunning = false;
            }
            
        }

        private void RefreshUI(bool IsBBConnected = false)
        {
            try
            {
                if (IsBBConnected)
                {
                    pictBBconnection.Image = Properties.Resources.MidGreen16;
                  //  Logger.LogEntry("Information", "BB Connected");
                }
                else
                {
                    pictBBconnection.Image = Properties.Resources.HighRed16;
                    Logger.LogEntry("Information", "BB DisConnected");
                }
            }
            catch { }
        }

        #endregion


        # region Config
        private void btnConfig_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.LogEntry("Information", "Config open");
                Configuration objConfig = new Configuration();
                objConfig.ShowDialog();
                if (objConfig.ConfigUpdated)
                    Refresh();

                objConfig = null;
            }
            catch (Exception ex) { Logger.LogEntry("Error", "btnUpdate_Click " + ex.Message + "\t" + ex.StackTrace); }

        }

       # endregion


        # region "Intraday Data"
        private void btnIntradayData_Click(object sender, EventArgs e)
        {
            try
            {
                IntradayDataSend objIntradayDataSend = new IntradayDataSend();
                objIntradayDataSend.Show();
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "btnIntradayData_Click " + ex.Message + "\t" + ex.StackTrace); 
            }
        }

     


        # endregion





    }
}



  //# region " Request/Response BB Live Price"

  //      #region "variable"
  //      int numItems;
  //      Element securityData;
  //      String security;
  //      int sequenceNumber;
  //      Session BBSession;
  //      SessionOptions sessionOptionsRR = new SessionOptions();
  //      Element ReferenceDataResponse;
  //      string strcol;
  //      Element securityDataArray;
  //      Element fieldData;
  //      string strBBStockTicker;
  //      List<CorrelationID> lstCorrIds = new List<CorrelationID>();
  //      double decLAST_PRICEValue = 0;
  //      bool BBConnectionStatus = false;
  //      #endregion

  //      //''Sending Bloomberg request and changing Bloomberg button status to 'Connecting to Bloomberg..'
  //      private void SendBBRequest()
  //      {
  //          try
  //          {
  //              BBSession = new Session(sessionOptionsRR, new Bloomberglp.Blpapi.EventHandler(RequestProcessEvent));
  //              BBSession.StartAsync(); //''start session with async
  //          }
  //          catch (Exception ex)
  //          {
  //              Logger.LogEntry("Error", "SendBBRequest " + ex.Message + "\t" + ex.StackTrace);
  //          }
  //      }

  //      private void RequestProcessEvent(Bloomberglp.Blpapi.Event eventObj, Session session)
  //      {
  //          try
  //          {
  //              switch ((eventObj.Type))
  //              {
  //                  case Event.EventType.SESSION_STATUS: //'' check session status 
  //                      foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
  //                      {
  //                          if (message.MessageType.Equals("SessionStarted")) //connection check
  //                          {
  //                              BBConnectionStatus = true;
  //                              session.OpenServiceAsync("//blp/refdata", new CorrelationID(99));
  //                          }
  //                          else
  //                          {
  //                              Logger.LogEntry("Error", "RequestProcessEvent-SESSION_STATUS: BB disconnected ");
  //                              BBConnectionStatus = false;
  //                              break;
  //                          }
  //                      }
  //                      break;
  //                  case Event.EventType.SERVICE_STATUS: // check service status

  //                      foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
  //                      {
  //                          if (message.CorrelationID.Value == 99 && message.MessageType.Equals("ServiceOpened"))
  //                          {
  //                              Service service = session.GetService("//blp/refdata");
  //                              Request request = service.CreateRequest("ReferenceDataRequest");
  //                              //string[] tickers = { "vod ln Equity" }; //lstLiveIdeas.Select(a => a.Ticker + "  EQUITY").Distinct().ToArray();
  //                              string[] tickers = lstLiveIdeas.Select(a => a.Ticker + "  EQUITY").Distinct().ToArray();
  //                              Array.Resize(ref tickers, tickers.Length + 1);
  //                              tickers[tickers.Length - 1] = mrktSymbol;
  //                              foreach (string security in tickers)
  //                                  request.GetElement("securities").AppendValue(security);

  //                              //''field to need to get from response
  //                              request.GetElement("fields").AppendValue("LAST_PRICE");

  //                              //''send request with correlationid
  //                              session.SendRequest(request, new CorrelationID(86));
  //                          }
  //                          else
  //                          {
  //                              Logger.LogEntry("Error", "RequestProcessEvent-SERVICE_STATUS: BB disconnected ");
  //                              BBConnectionStatus = false;

  //                          }

  //                      }
  //                      break;
  //                  case Event.EventType.PARTIAL_RESPONSE: // response
  //                      BBConnectionStatus = true;
  //                      handleRequestResponseData(eventObj);
  //                      break;
  //                  case Event.EventType.RESPONSE:
  //                      BBConnectionStatus = true;
  //                      BBDataAvailable = true; // confirms that fully data available
  //                      handleRequestResponseData(eventObj);
  //                      break;

  //              }
  //          }
  //          catch (Exception ex)
  //          {
  //              Logger.LogEntry("Error", "RequestProcessEvent " + ex.Message + "\t" + ex.StackTrace);
  //          }
  //      }

  //      //''Handle Bloomberg request response. Show data to dashboard price and intraday columns 
  //      private void handleRequestResponseData(Bloomberglp.Blpapi.Event eventObj) //''for BB Live Price
  //      {
  //          try
  //          { // get response data message

  //              foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
  //              {
  //                  ReferenceDataResponse = message.AsElement;
  //                  if (ReferenceDataResponse.HasElement("responseError"))
  //                      break;

  //                  strcol = message.CorrelationID.ToString();
  //                  securityDataArray = ReferenceDataResponse.GetElement("securityData");
  //                  numItems = securityDataArray.NumValues;
  //                  for (int i = 0; i < numItems; i++)
  //                  {
  //                      securityData = securityDataArray.GetValueAsElement(i);
  //                      security = securityData.GetElementAsString("security");
  //                      sequenceNumber = securityData.GetElementAsInt32("sequenceNumber");
  //                      if (securityData.HasElement("securityError")) //check response error
  //                      { break; }
  //                      else
  //                      {
  //                          //get field  data and assign to current row
  //                          if (securityData.HasElement("fieldData"))
  //                          {
  //                              fieldData = securityData.GetElement("fieldData"); //''get all fielddata from security data
  //                              strBBStockTicker = security.Replace(" EQUITY", "").Trim();

  //                              if (fieldData["LAST_PRICE"].NumValues == 1)
  //                              {
  //                                  //assign value to variables 
  //                                  decLAST_PRICEValue = Convert.ToDouble(fieldData["LAST_PRICE"].GetValue());

  //                                  for (int j = 0; j < grvLiveIdea.RowCount; j++)
  //                                  {
  //                                      if (strBBStockTicker == mrktSymbol)
  //                                      {
  //                                          grvLiveIdea.Rows[i].Cells["MktClosePrice"].Value = decLAST_PRICEValue;
  //                                          continue;
  //                                      }

  //                                      if (grvLiveIdea.Rows[i].Cells["Ticker"].Value.ToString() == strBBStockTicker)
  //                                      { grvLiveIdea.Rows[i].Cells["ClosePrice"].Value = decLAST_PRICEValue; }
  //                                  }
  //                                  //foreach (var idea in lstLiveIdeas)
  //                                  //{
  //                                  //    if (strBBStockTicker == mrktSymbol)
  //                                  //    {
  //                                  //        idea.MktClosePrice = decLAST_PRICEValue;
  //                                  //    }
  //                                  //    else
  //                                  //    {
  //                                  //        if (idea.Ticker != strBBStockTicker) continue;
  //                                  //        idea.ClosePrice = decLAST_PRICEValue;
  //                                  //    }
  //                                  //}
  //                              }
  //                          }

  //                      }
  //                  }
  //              }

  //          }
  //          catch (Exception ex)
  //          {
  //              Logger.LogEntry("Error", "handleRequestResponseData " + ex.Message + "\t" + ex.StackTrace);
  //          }
  //          finally
  //          {
  //              if (BBDataAvailable) //to check all tickers data available not partial
  //              {

  //                  CheckSL_TP();
  //                  //System.Threading.Thread thrd = new System.Threading.Thread(new ThreadStart(this.RefreshGrid) );
  //                  //thrd.Start();
  //                  Logger.LogEntry("Information", "Request/Response finish");
  //                  string[] tickers = lstLiveIdeas.Select(a => a.Ticker + " Equity").Distinct().ToArray();
  //                  Array.Resize(ref tickers, tickers.Length + 1);
  //                  tickers[tickers.Length - 1] = mrktSymbol;
  //                  BBDataAvailable = false;
  //                  StartSubscription(tickers);
  //              }
  //          }
  //      }

  //      # endregion

//private void ReadLivePrice()
//{
//    try
//    {
//        BBRequestResponse obj = new BBRequestResponse();
//        string[] tickers = lstLiveIdeas.Select(a => a.Ticker + "  EQUITY").Distinct().ToArray();
//        Array.Resize(ref tickers, tickers.Length + 1);
//        tickers[tickers.Length - 1] = mrktSymbol;
//        string[] Fields = { "LAST_PRICE" }; // for recent tick price
//        List<List<string>> livePrice = obj.GetBbFields(tickers, Fields);
//        var bbPrice = livePrice.ToArray();
//        foreach (var idea in lstLiveIdeas)
//        {
//            var bbprice = livePrice.Where(x => x.Contains(idea.Ticker + "  EQUITY")).FirstOrDefault();
//            var mrktprice = livePrice.Where(x => x.Contains(mrktSymbol)).FirstOrDefault();
//            idea.ClosePrice = Convert.ToDouble(bbprice[1]);
//            idea.MktClosePrice = Convert.ToDouble(mrktprice[1]);
//        }
//        grvLiveIdea.Refresh();
//        lblSystemMessage.Text = "Bloom berg Live price : Connected";
//    }
//    catch (Exception ex)
//    {
//        lblSystemMessage.Text = "Bloom berg Live price : Disconnected";
//        Logger.LogEntry("Error", "ReadLivePrice " + ex.Message + "\t" + ex.StackTrace);
//    }
//}


 //private void StopBBSession(bool bolIsBBsessionStoped = false, bool bolIsAsyncCalled = false,bool bolRollback = false) //''for BB Live Price
 //       {
 //           try {
 //               if (BBSession != null)
 //               {
 //                   BBSession.Stop(AbstractSession.StopOption.SYNC);
 //                    BBSession.Dispose();
 //               }
 //           }
 //           catch (Exception ex)
 //           { Logger.LogEntry("Error", "StopBBSession " + ex.Message + "\t" + ex.StackTrace); }
 //       }


 //private void RefreshGrid()
 //       {
 //           try
 //           {
 //               for (int i = 0; i < grvLiveIdea.Rows.Count; i++)
 //               {
 //                   grvLiveIdea.InvalidateRow(i);

 //               }
 //               //  grvLiveIdea.BeginEdit(true);
 //               //  grvLiveIdea.DataSource = lstLiveIdeas;
 //               //  grvLiveIdea.EndEdit();
 //               ////  grvLiveIdea.Update();
 //               //  ////grvLiveIdea.Parent.Refresh();
 //               //  grvLiveIdea.Refresh();
 //               //this.Text = "SL & TP Utility - Bloomberg connected";

 //           }
 //           catch (Exception ex)
 //           { Logger.LogEntry("Error", "RefreshGrid " + ex.Message + "\t" + ex.StackTrace); }
 //       }