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

    public partial class Form1 : Form
    {
        string mrktSymbol = "VGA INDEX";
        List<DAL.DataFetching.IdeaData> lstLiveIdeas;
        class Idea
        {
            public int ClientId = 239;
            public Int64 IdeaId = 0;
            public string PortwareStretegyId = "";
            public string BBTicker = "";
            public double Price = 0;
            public string UnwindType = "";
            public float UnwindValue = 0;

        }

        private decimal bbPrice; private bool isBloombergConnected = true;//by default, true  
        List<Idea> lstPrice = new List<Idea>();

        public Form1()
        {
            InitializeComponent();
        }

        private void tmrProcess_Tick(object sender, EventArgs e)
        {
            try
            {
                UpdateDataset();
            }
            catch(Exception ex) {}
        }

      

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            lstLiveIdeas = new List<DAL.DataFetching.IdeaData>();
            lstLiveIdeas = DAL.DataFetching.getOpenIdeaList(); // Read recent ideas
            
            BBRequestResponse obj = new BBRequestResponse();
            string[] tickers= lstLiveIdeas.Select(a => a.Ticker + "  EQUITY").Distinct().ToArray();
           // string[] tickers = { "VOD LN EQUITY" };
            Array.Resize(ref tickers, tickers.Length + 1);
            tickers[tickers.Length-1] = mrktSymbol;
            string[] Fields = { "LAST_PRICE" }; // for recent price
            List<List<string>> livePrice =obj.GetBbFields(tickers, Fields);
            var bbPrice = livePrice.ToArray();

            foreach (var idea in lstLiveIdeas)
            {   
                var bbprice = livePrice.Where(x => x.Contains(idea.Ticker + "  EQUITY")).FirstOrDefault();
                var mrktprice = livePrice.Where(x => x.Contains(mrktSymbol)).FirstOrDefault();
                idea.ClosePrice = Convert.ToDouble(bbprice[1]);
                idea.MktClosePrice = Convert.ToDouble(mrktprice[1]);
            }

            UpdateDataset();  //Update to grid
            double result = BAL.IdeaExitCalc.GetStopLossVsMarketExit(10, 5, 10, 5, "Long", 2.5); // SL and TP scenario
        }


        private void UpdateDataset()
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
        # region "request response data"
        Element ReferenceDataResponse;
        string strcol;
        Element securityDataArray;
        Element securityData;
        string security;
        int numItems;
        int sequenceNumber;
        bool bolRequestResponseRunning;
        Element fieldData;
        string strBBStockTicker;
        decimal decLAST_PRICEValue;
        Session BBSession;
        SessionOptions sessionOptions;
        private void SendBBRequest()
        {
            try
            {

                BBSession = new Session(sessionOptions,new Bloomberglp.Blpapi.EventHandler(RequestProcessEvent));
                BBSession.StartAsync(); //start session with async
            }
            catch { }

        }

        private void RequestProcessEvent(Bloomberglp.Blpapi.Event eventObj, Session session)
        {
            try {
                handleRequestResponseData(eventObj);
        }
            catch {}

        }

        
        private void handleRequestResponseData(Event eventObj)
        {
            foreach(Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
            {

                ReferenceDataResponse = message.AsElement;
                if(ReferenceDataResponse.HasElement("responseError")) // check response error
                    return;
                
                strcol = message.CorrelationID.ToString();
                 securityDataArray = ReferenceDataResponse.GetElement("securityData");
                numItems = securityDataArray.NumValues;
                for(int i = 0; i<= numItems-1; i++)
                {
                      securityData = securityDataArray.GetValueAsElement(i);
                    security = securityData.GetElementAsString("security");
                    sequenceNumber = securityData.GetElementAsInt32("sequenceNumber");
                      if(securityData.HasElement("securityError") ) //check response error
                      { 
                          bolRequestResponseRunning = false;
                            return;
                      }
                    else
                      {
                          //get field data and assign to current row
                           if(securityData.HasElement("fieldData") == true)
                           {
                               fieldData = securityData.GetElement("fieldData"); //''get all fielddata from security data
                               if(lstLiveIdeas.Count() >0)
                               {
                                   strBBStockTicker = security.Replace(" Equity", "");
                                   if(fieldData["LAST_PRICE"].NumValues == 1)
                                   {
                                        object decLAST_PRICEValue = fieldData["LAST_PRICE"].GetValue();

                                        //''added logic for ISR;ZAF country stocks as price coming diff. TR and Bloomberg
                                        //    If gblBloombergPriceforCurrency.Replace(";", " ").Contains(drStock.CurrencyCode.ToString.Trim) AndAlso drStock.CurrencyCode.ToString.Trim.Length > 0 Then
                                        //        decLAST_PRICEValue = (decLAST_PRICEValue / 100)
                                        //        decPREV_SES_LAST_PRICEValue = (decPREV_SES_LAST_PRICEValue / 100)
                                        //    End If


                                   }
                               }

                           }


                      }

                }


            }

        }

        # endregion

        private void button2_Click(object sender, EventArgs e)
        {
            BBRequestResponse obj = new BBRequestResponse();
            string[] tickers = { "VOD LN EQUITY", "MKS LN EQUITY" };
            string[] Fields = { "LAST_PRICE" };
           List<List<string>> result = obj.GetBbFields(tickers, Fields);
        }


        #region Subscription


        //#region Declaration

        //Session session = default(Session);
        //SessionOptions sessionOptions = new SessionOptions();
        //List<Subscription> subscriptions = new List<Subscription>();
        //List<CorrelationID> correlationIds = new List<CorrelationID>();
        //Element referenceDataResponse = default(Element);
        //string fieldsName = "BEST_BID,BEST_ASK,BID_SIZE,ASK_SIZE,LAST_PRICE,SIZE_LAST_TRADE,TRADE_UPDATE_STAMP_RT,LAST_DIR,EVT_TRADE_SIZE_RT,EVT_TRADE_PRICE_RT,EVT_TRADE_DATE_RT,PX_OFFICIAL_AUCTION_RT,OFFICIAL_AUCTION_VOLUME_RT,TIME_AUCTION_CALL_CONCLUSION_RT,VOLUME_TDY,NUM_TRADES_RT,ON_BOOK_VOLUME_TODAY_RT,OFF_BOOK_VOLUME_TODAY_RT,MKTDATA_EVENT_TYPE,MKTDATA_EVENT_SUBTYPE";
        //string errFilePath = "errLog.txt";

        //#endregion

        ////Start subscription 
        //public void StartSubscription(string tickers)
        //{
        //    try
        //    {
        //        if (tickers.Trim().Length == 0)
        //            return;

        //        this.correlationIds.Clear();
        //        this.subscriptions.Clear();
        //        subscriptions = new List<Subscription>();

        //        foreach (string ticker in tickers.Split(','))
        //        {
        //            //to-do ... 
        //            //string fname = Application.StartupPath.ToString() + "\\" + ticker + "_" + System.DateTime.Now.ToShortDateString().Replace(", "") + ".txt";
        //            //if (System.IO.File.Exists(fname) == false)
        //            //    System.IO.File.Create(fname);

        //            CorrelationID correlationId = new CorrelationID(ticker + " Equity");
        //            if (correlationIds.Contains(correlationId) == false)
        //            {
        //                subscriptions.Add(new Subscription(ticker + " Equity", fieldsName, "interval=5", correlationId));
        //                this.correlationIds.Add(correlationId);
        //            }
        //        }

        //        session = new Session(sessionOptions, new Bloomberglp.Blpapi.EventHandler(SubscriptionEvent));
        //        session.StartAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.GetType().Equals(typeof(OutOfMemoryException))) { }
        //        else if (ex.Message.ToUpper().Contains("SYSTEM.OUTOFMEMORYEXCEPTION")) { }
        //        else
        //        {
        //            System.IO.File.AppendAllText(errFilePath, "Error :: " + ex.Message.ToString() + Environment.NewLine);
        //        }
        //    }
        //}
        ////Subscription process event
        //private void SubscriptionEvent(Event eventObj, Session session)
        //{
        //    try
        //    {
        //        switch ((eventObj.Type))
        //        {
        //            case Event.EventType.SESSION_STATUS:
        //                foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
        //                {
        //                    if (message.MessageType.Equals("SessionStarted"))
        //                    {
        //                        session.OpenServiceAsync("//blp/mktdata", new CorrelationID(99));
        //                    }
        //                    else if (message.MessageType.Equals("SessionConnectionDown")) { }
        //                }
        //                break;
        //            case Event.EventType.SERVICE_STATUS:
        //                foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
        //                {
        //                    if (message.CorrelationID.Value == 99 && message.MessageType.Equals("ServiceOpened"))
        //                    {
        //                        if (subscriptions.Count > 0)
        //                        {
        //                            session.Subscribe(subscriptions);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                }
        //                break;
        //            case Event.EventType.SUBSCRIPTION_DATA:
        //                SubscriptionResponse(eventObj);
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.GetType().Equals(typeof(ArgumentOutOfRangeException))) { }
        //        else if (ex.Message.ToUpper().Contains("SYSTEM.ARGUMENTOUTOFRANGEEXCEPTION")) { }
        //        else if (ex.GetType().Equals(typeof(OutOfMemoryException))) { }
        //        else if (ex.Message.ToUpper().Contains("SYSTEM.OUTOFMEMORYEXCEPTION")) { }
        //        else { System.IO.File.AppendAllText(errFilePath, "Error :: " + ex.Message.ToString() + Environment.NewLine); }
        //    }
        //}
        ////subscription response data
        //private void SubscriptionResponse(Event eventObj)
        //{
        //    try
        //    {
        //        foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
        //        {
        //            referenceDataResponse = message.AsElement;

        //            if ((referenceDataResponse.HasElement("responseError")))
        //            {
        //                break;
        //            }
        //            if (message.MessageType.Equals("MarketDataEvents") == true)
        //            {
        //                //to-do ... pass message to bar generation lib and get response 
        //                string strTicker = message.CorrelationID.ToString().ToUpper().Replace("EQUITY", "").ToString().Replace("USER:", "").ToString().Trim();
        //                //string filename = Application.StartupPath.ToString() + "\\" + strTicker + "_" + System.DateTime.Now.ToShortDateString().Replace(", "") + ".txt";
        //                //System.IO.File.AppendAllText(filename, System.DateTime.Now.ToString().Replace(", "").Replace(":", "").Replace(" ", "") + Environment.NewLine + message.ToString() + Environment.NewLine);
        //                strTicker = "";
        //                //filename = "";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.GetType().Equals(typeof(ArgumentOutOfRangeException))) { }
        //        else if (ex.Message.ToUpper().Contains("SYSTEM.ARGUMENTOUTOFRANGEEXCEPTION")) { }
        //        else if (ex.GetType().Equals(typeof(OutOfMemoryException))) { }
        //        else if (ex.Message.ToUpper().Contains("SYSTEM.OUTOFMEMORYEXCEPTION")) { }
        //        else { System.IO.File.AppendAllText(errFilePath, "Error :: " + ex.Message.ToString() + Environment.NewLine); }
        //    }
        //}

        #endregion
         



        //#region "Bloomberg Data"


        

        //private void tmrBloomberg_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        tmrBloomberg.Enabled = false; tmrBloomberg.Stop();
        //        System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(new Action(getBloombergPrice));
        //        task.Start();
        //    }
        //    catch
        //    {

        //    }
        //}
        //private void getBloombergPrice() //Bloomberg integration 
        //{
        //    try
        //    {
        //        var unqTicker = lstLiveIdeas.Select(a => a.Ticker).Distinct().ToArray();
                
        //        BloombergLive objBB = new BloombergLive();
        //        bbIdentifiers identifiers = new bbIdentifiers();

        //        foreach (var stock in unqTicker)
        //        {
        //            //bbIdentifier identifier = new bbIdentifier(stock, 0.00, 0.00);
        //            identifiers.Add(new bbIdentifier(stock, 0.00, 0.00));
        //        }
        //        identifiers.Add(new bbIdentifier(mrktSymbol, 0.00, 0.00));
        //        identifiers = objBB.GetStockPrice(identifiers);
        //        if (identifiers.Count > 0)
        //        {
        //            isBloombergConnected = true;
        //            var mktValue = identifiers.FirstOrDefault(a => a.Ticker == mrktSymbol);
        //            foreach (var idea in lstLiveIdeas)
        //            {   
        //                var bbValue = identifiers.FirstOrDefault(a => a.Ticker == idea.Ticker);
        //                if (bbValue.Price > 0 && mktValue.Price > 0) 
        //                {
        //                    idea.ClosePrice = bbValue.Price;
        //                    idea.MktClosePrice = mktValue.Price;
        //                }
        //            }
                            
        //        }
        //        else
        //        {
        //            isBloombergConnected = false;
        //        }

        //    }
        //    catch (Exception ex) { throw ex; }

        //}
        //#endregion




    }
}
