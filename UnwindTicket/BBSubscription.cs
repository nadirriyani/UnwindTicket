using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloomberglp.Blpapi;

namespace UnwindTicket
{
    class BBSubscription
    {

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
                        subscriptions.Add(new Subscription(ticker + " Equity", fieldsName, "interval=20", correlationId));
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
