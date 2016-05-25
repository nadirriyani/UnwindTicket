using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bloomberglp.Blpapi;


namespace UnwindTicket
{
    public partial class OpenIdeasSubscription : Form
    {
        public OpenIdeasSubscription()
        {
            InitializeComponent();
        }

        private void OpenIdeasSubscription_Load(object sender, EventArgs e)
        {
            try
            {
                tmrBloomberg.Start();
            }
            catch (Exception ex)
            { Logger.LogEntry("Error", "OpenIdeas_Load " + ex.Message + "\t" + ex.StackTrace); }
        }

    

        string mrktSymbol = "VGA INDEX";
        List<DAL.DataFetching.IdeaData> lstLiveIdeas;
        
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                RawData();
                ReadLivePrice();
                UpdateGrid();
            }
            catch (Exception ex)
            { Logger.LogEntry("Error", ex.Message + "\t" + ex.StackTrace); }
        }

        private void RawData()
        {
            try
            {
                lstLiveIdeas = new List<DAL.DataFetching.IdeaData>();
                lstLiveIdeas = DAL.DataFetching.getOpenIdeaList(); // Read recent ideas
            }
            catch (Exception ex)
            { Logger.LogEntry("Error", "RawData " + ex.Message + "\t" + ex.StackTrace); }
        }

        private void ReadLivePrice()
        {
            try
            {
                BBRequestResponse obj = new BBRequestResponse();
                string[] tickers = lstLiveIdeas.Select(a => a.Ticker + "  EQUITY").Distinct().ToArray();
                Array.Resize(ref tickers, tickers.Length + 1);
                tickers[tickers.Length - 1] = mrktSymbol;
                string[] Fields = { "LAST_PRICE" }; // for recent tick price
                List<List<string>> livePrice = obj.GetBbFields(tickers, Fields);
                var bbPrice = livePrice.ToArray();
                foreach (var idea in lstLiveIdeas)
                {
                    var bbprice = livePrice.Where(x => x.Contains(idea.Ticker + "  EQUITY")).FirstOrDefault();
                    var mrktprice = livePrice.Where(x => x.Contains(mrktSymbol)).FirstOrDefault();
                    idea.ClosePrice = Convert.ToDouble(bbprice[1]);
                    idea.MktClosePrice = Convert.ToDouble(mrktprice[1]);
                }
                grvLiveIdea.Refresh();
                lblSystemMessage.Text = "Bloom berg Live price : Connected";
            }
             catch (Exception ex)
            {
                lblSystemMessage.Text = "Bloom berg Live price : Disconnected";
                Logger.LogEntry("Error", "ReadLivePrice " + ex.Message + "\t" + ex.StackTrace);
            }
        }

        private void UpdateGrid()
        {
            try
            {
                grvLiveIdea.DataSource = lstLiveIdeas;
                grvLiveIdea.AutoSize = true;
                grvLiveIdea.ScrollBars = ScrollBars.Both;
                for (int j = 0; j < grvLiveIdea.Columns.Count; j++)
                {
                    if ("TIMID,EVENTID,OTICSINGESTID,IDEAID,CLOSINGPERIOD,PORTWARESTRATEGYID".Contains(grvLiveIdea.Columns[j].Name.ToUpper()) == true)
                    {
                        grvLiveIdea.Columns[j].Visible = false;
                        continue;
                    }

                    grvLiveIdea.Columns[j].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    grvLiveIdea.Columns[j].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grvLiveIdea.Columns[j].ReadOnly = true;
                    grvLiveIdea.Columns[j].FillWeight = 1;
                }
                grvLiveIdea.Columns["OpenPrice"].DefaultCellStyle.Format = "#,0.00";
                grvLiveIdea.Columns["ClosePrice"].DefaultCellStyle.Format = "#,0.00";
                grvLiveIdea.Columns["MktOpenPrice"].DefaultCellStyle.Format = "#,0.00";
                grvLiveIdea.Columns["MktClosePrice"].DefaultCellStyle.Format = "#,0.00";
                grvLiveIdea.Columns["Value"].DefaultCellStyle.Format = "#,0.00";
                grvLiveIdea.Refresh();
            }
            catch (Exception ex)
            { Logger.LogEntry("Error", "UpdateGrid" +  ex.Message + "\t" + ex.StackTrace); }
        }

        private void tmrBloomberg_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrBloomberg.Stop();
                ReadLivePrice();
                CheckSL_TP();
                tmrBloomberg.Start();
            }
           catch (Exception ex)
            { Logger.LogEntry("Error", "tmrBloomberg_Tick " + ex.Message + "\t" + ex.StackTrace); }

        }

        private void CheckSL_TP()
        {
            try {

                foreach (var idea in lstLiveIdeas)
                {
                    if(idea.UnwindType == "SL")
                        idea.Unwind= BAL.IdeaExitCalc.GetStopLossVsMarketExit(idea.OpenPrice,idea.ClosePrice, idea.MktOpenPrice, idea.MktClosePrice, idea.Direction, idea.Value); // SL  scenario
                    else if(idea.UnwindType == "TP")
                        idea.Unwind = BAL.IdeaExitCalc.GetTakeProfitsVsMarketExit(idea.OpenPrice, idea.ClosePrice, idea.MktOpenPrice, idea.MktClosePrice, idea.Direction, idea.Value); //  TP scenario
                }
            }
            catch (Exception ex)
            { Logger.LogEntry("Error", "CheckSL_TP " + ex.Message + "\t" + ex.StackTrace); }

        }

              #region Subscription

        Session session = default(Session);
        SessionOptions sessionOptions = new SessionOptions();
        List<Subscription> subscriptions = new List<Subscription>();
        List<CorrelationID> correlationIds = new List<CorrelationID>();
        Element referenceDataResponse = default(Element);
        string fieldsName = "LAST_PRICE";
        string errFilePath = "errLog.txt";

        #endregion

        #region Subscription
        //Start subscription 
        public void StartSubscription(string tickers)
        {
            try
            {
                if (tickers.Trim().Length == 0)
                    return;

                this.correlationIds.Clear();
                this.subscriptions.Clear();
                subscriptions = new List<Subscription>();

                foreach (string ticker in tickers.Split(','))
                {
                    //to-do ... 
                    //string fname = Application.StartupPath.ToString() + "\\" + ticker + "_" + System.DateTime.Now.ToShortDateString().Replace(", "") + ".txt";
                    //if (System.IO.File.Exists(fname) == false)
                    //    System.IO.File.Create(fname);

                    CorrelationID correlationId = new CorrelationID(ticker + " Equity");
                    if (correlationIds.Contains(correlationId) == false)
                    {
                        subscriptions.Add(new Subscription(ticker + " Equity", fieldsName, "interval=5", correlationId));
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
                else
                {
                    System.IO.File.AppendAllText(errFilePath, "Error :: " + ex.Message.ToString() + Environment.NewLine);
                }
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
                                break;
                            }
                        }
                        break;
                    case Event.EventType.SUBSCRIPTION_DATA:
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
                else { System.IO.File.AppendAllText(errFilePath, "Error :: " + ex.Message.ToString() + Environment.NewLine); }
            }
        }
        //subscription response data
        private void SubscriptionResponse(Event eventObj)
        {
            try
            {
                foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
                {
                    referenceDataResponse = message.AsElement;

                    if ((referenceDataResponse.HasElement("responseError")))
                    {
                        break;
                    }
                    if (message.MessageType.Equals("MarketDataEvents") == true)
                    {
                        //to-do ... pass message to bar generation lib and get response 
                        string strTicker = message.CorrelationID.ToString().ToUpper().Replace("EQUITY", "").ToString().Replace("USER:", "").ToString().Trim();
                        //string filename = Application.StartupPath.ToString() + "\\" + strTicker + "_" + System.DateTime.Now.ToShortDateString().Replace(", "") + ".txt";
                        //System.IO.File.AppendAllText(filename, System.DateTime.Now.ToString().Replace(", "").Replace(":", "").Replace(" ", "") + Environment.NewLine + message.ToString() + Environment.NewLine);
                        strTicker = "";
                        //filename = "";
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Equals(typeof(ArgumentOutOfRangeException))) { }
                else if (ex.Message.ToUpper().Contains("SYSTEM.ARGUMENTOUTOFRANGEEXCEPTION")) { }
                else if (ex.GetType().Equals(typeof(OutOfMemoryException))) { }
                else if (ex.Message.ToUpper().Contains("SYSTEM.OUTOFMEMORYEXCEPTION")) { }
                else { System.IO.File.AppendAllText(errFilePath, "Error :: " + ex.Message.ToString() + Environment.NewLine); }
            }
        }

        #endregion

    }
}
