using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Bloomberglp.Blpapi;
using System.ComponentModel;
using UnwindTicket.Entity;
using System.IO;
using DataClassLibrary;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace UnwindTicket.DAL
{
    public class BloombergHistoryData
    {
        #region Declaration
        Session session = new Session(new SessionOptions());
       
        public DateTime StartRequestDateTime = new DateTime(2017, 01, 31, 00, 00, 0, 0);
        public DateTime EndRequestDateTime = new DateTime(2017, 01, 31, 23, 59, 59, 0);
        public string[] BBTickers;
        Int32 ClientId = 76;
        string WatchlistName = "Spain Equity";
        
        public List<Bar> IntradayBars = new List<Bar>();
        private BackgroundWorker bwProcess;

        string FTPLocation = ConfigurationManager.AppSettings["FTPLocation"];
        string FTPUserName = ConfigurationManager.AppSettings["FTPUserName"];
        string FTPPassword = ConfigurationManager.AppSettings["FTPPassword"];
        string FTPFolderName = ConfigurationManager.AppSettings["FTPFolderName"];

        public string UploadDataLink = ConfigurationManager.AppSettings["UploadDataLink"];

        #endregion

        #region Methods

        /// <summary>
        /// Start history request 
        /// </summary>
        /// <param name="tickers"></param>
        /// <param name="_reSubscribe"></param>
        public void ProcessHistoryRequest()
        {
            try
            {
                bwProcess = new System.ComponentModel.BackgroundWorker();
                bwProcess.DoWork += bwProcess_DoWork;
                bwProcess.RunWorkerCompleted += bwProcess_RunWorkerCompleted;
             
                bwProcess.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "ProcessHistoryRequest: " + ex.Message.ToString() + "\n" + ex.StackTrace.ToString());
            }
        }

        /// <summary>
        /// send request for each symbol
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwProcess_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                string result = CallApiMethod(UploadDataLink + "GetIdentifier?" + "ClientId=" + ClientId + "&WatchlistName=" + WatchlistName, string.Empty , "GET");
                BBTickers = result.Split(',').ToArray();

                session = new Session(new SessionOptions());
                bool sessionStarted = session.Start();
                if (!sessionStarted)
                {
                    Logger.LogEntry("Information", "bwProcess_DoWork: " + "Failed to start session.");
                    return;
                }
                if (!session.OpenService("//blp/refdata"))
                {
                    Logger.LogEntry("Information", "bwProcess_DoWork: " + "Failed to open API");
                    return;
                }

                foreach (string symbol in BBTickers)
                {

                    // Assign exchange start / end UTC time for Bloomberg request 
                    Service refDataService = session.GetService("//blp/refdata");
                    Request requestAuth = refDataService.CreateRequest("IntradayBarRequest");

                    //Assign fields
                    requestAuth.Set("eventType", "TRADE");
                    // Assign ticker
                    requestAuth.Set("security", symbol); //+ " EQUITY"
                    // Assign interval and fill bars
                    requestAuth.Set("interval", 1); // in minutes
                    requestAuth.Set("gapFillInitialBar", false); // to fill data from previous tick
                    string s = EndRequestDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    requestAuth.Set("startDateTime", StartRequestDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
                    requestAuth.Set("endDateTime", EndRequestDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));

                    CorrelationID correlationId = new CorrelationID(symbol); //+ " Equity"
                    session.SendRequest(requestAuth, correlationId);

                    bool done = false;
                    while (!done)
                    {
                        Event eventObj = session.NextEvent();
                        if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                        {
                            requestProcessResponse(eventObj);
                        }
                        else if (eventObj.Type == Event.EventType.RESPONSE)
                        {
                            requestProcessResponse(eventObj);
                            done = true;
                        }
                        else
                        {
                            foreach (Message msg in eventObj)
                            {
                                if (eventObj.Type == Event.EventType.SESSION_STATUS)
                                {
                                    if (msg.MessageType.Equals("SessionTerminated"))
                                    {
                                        done = true;
                                    }
                                }
                            }
                        }
                    }
                }
                StopSession();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("SESSION NOT STARTED") == false)
                    throw ex;
            }
        }

        /// <summary>
        /// Stop session
        /// </summary>
        public void StopSession()
        {
            try
            {
                session.Stop();
                session.Dispose();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("SESSION NOT STARTED") == false)
                    throw ex;
            }
        }

        /// <summary>
        /// proceed to history request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwProcess_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    this.GenerateBarCSV(IntradayBars);
                    Logger.LogEntry("Information", "BB Process finish");
                    
                }
                else
                {
                    Logger.LogEntry("Error", "bwProcess_RunWorkerCompleted: " + e.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "bwProcess_RunWorkerCompleted: " + ex.Message.ToString() + "\n" + ex.StackTrace.ToString());
            }
        }
        /// <summary>
        /// request process response 
        /// </summary>
        /// <param name="eventObj"></param>
        public void requestProcessResponse(Event eventObj)
        {
            try
            {
                foreach (Bloomberglp.Blpapi.Message message in eventObj.GetMessages())
                {
                    string responseTicker = message.CorrelationID.Object.ToString().ToUpper().Replace("EQUITY", "").Trim();

                    Element referenceDataResponse = message.AsElement;
                    if ((referenceDataResponse.HasElement("responseError")))
                    {
                        if ((referenceDataResponse.HasElement("category") == true) && (referenceDataResponse.GetElementAsString("category") == "LIMIT"))
                        {
                            if ((referenceDataResponse.HasElement("subcategory") == true))
                            {
                                if (referenceDataResponse.GetElementAsString("subcategory").ToString().Contains("DAILY"))
                                {
                                    Logger.LogEntry("Information", "requestProcessResponse: Daily Limit reached ");
                                }
                                else if (referenceDataResponse.GetElementAsString("subcategory").ToString().Contains("MONTHLY"))
                                {
                                    Logger.LogEntry("Information", "requestProcessResponse: Monthly Limit reached ");
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            // pass to bar gen
                            if (message.HasElement("barData"))
                            {
                                Element data = message.GetElement("barData").GetElement("barTickData");
                                int numBars = data.NumValues;
                                for (int i = 0; i < numBars; ++i)
                                {
                                    Element bar = data.GetValueAsElement(i);
                                    IntradayBars.Add(
                                        new Bar
                                        { 
                                            Ric = responseTicker,
                                            Date = StartRequestDateTime.Date,
                                            Time = Convert.ToDateTime(bar.GetElementAsDate("time").ToString()),
                                            Volume = bar.GetElementAsInt64("volume"),
                                            Amount = bar.GetElementAsFloat64("value"),
                                        }
                                    );
                                
                                }
                                Logger.LogEntry("Information", "requestProcessResponse: Response received for  [" + responseTicker + "]");
                            }
                            else
                            {
                                Logger.LogEntry("Information", "requestProcessResponse: No response received for [" + responseTicker + "]");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogEntry("Error", "requestProcessResponse: " + ex.Message.ToString() + "\n" + ex.StackTrace.ToString());
                        }

                        //if (Convert.ToBoolean(ConfigurationManager.AppSettings["WriteBloombergMessage"]) == true)
                        //    Logger.LogEntry("Information", "requestProcessResponse: " + message.CorrelationID.Object.ToString().ToUpper() + message);

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "requestProcessResponse: " + ex.Message.ToString() + "\n" + ex.StackTrace.ToString());
            }
        }


        private void GenerateBarCSV(List<Bar> Bars)
        {
            try
            {
                string LocalHistoryFilePath = AppDomain.CurrentDomain.BaseDirectory;
                foreach (string ticker in Bars.Select(a => a.Ric).Distinct())
                {
                    try
                    {
                        List<Bar> bar = Bars.Where(a => a.Ric == ticker).ToList();
                        if (bar.Count() == 0)
                        {
                            Logger.LogEntry("Information", "No Bar Data found found for Ticker: " + ticker);
                            continue;
                        }
                        DateTime TradeDate = bar.Select(a => a.Date).FirstOrDefault();
                        if (TradeDate == null)
                        {
                            Logger.LogEntry("Information", "TradeDate found Null for Ticker: " + ticker);
                            continue;
                        }
                        CreateDirectory(TradeDate, LocalHistoryFilePath);
                        string BarFileName = LocalHistoryFilePath + @"\Log\" + TradeDate.ToString("dd.MMM.yyyy") + @"\" + ticker + ".csv";

                        StreamWriter sw = new StreamWriter(BarFileName, true);
                        StringBuilder Data = new StringBuilder();
                        Data.Append("Time,Volume,Amount");
                        foreach (Bar b in bar)
                        {
                            Data.Append(b.Time + "," + b.Volume + "," + b.Amount + "\n");
                        }
                        sw.Write(Data.ToString());

                        try
                        {
                            string result = CallApiMethod(UploadDataLink + "UploadBBData/", "Ticker=" + ticker + "&TradeDate=" + TradeDate.ToString("dd-MMM-yyyy") + "&Data=" + Data.ToString(), "POST");
                            Logger.LogEntry("Information", "Data uploaded for Ticker: " + ticker + " Result=" + result);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogEntry("Error", "btnIntradayData_Click - Upload Data Ticker " + ticker + "\n" + ex.Message + "\t" + ex.StackTrace);
                        }

                       // this.UploadFileToServer(TradeDate.ToString("dd.MMM.yyyy"), ticker + ".csv", BarFileName); // FTP Upload
                    }
                    catch (Exception ex)
                    {
                        Logger.LogEntry("Error", "btnIntradayData_Click Ticker " + ticker  + "\n" + ex.Message + "\t" + ex.StackTrace);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "btnIntradayData_Click " + ex.Message + "\t" + ex.StackTrace);
            }

        }


        public string CallApiMethod(string url, string argument, string method)
        {
            try
            {
                string accessToken = "";
                string responseResult = string.Empty;
                HttpWebRequest webRequest = null;
                WebResponse webResponse = null;
                Stream responseStream = null;
                StreamReader responseStreamReader = null;

                SetCertificatePolicy();
                webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.ProtocolVersion = HttpVersion.Version10;
                webRequest.KeepAlive = false;
                webRequest.ReadWriteTimeout = 1000000000;
                webRequest.Headers.Add("HTTP_ACCEPT_ENCODING", "deflate");
                if (!string.IsNullOrEmpty(accessToken)) webRequest.Headers.Add("X-Auth-Token", accessToken);

                webRequest.Method = method;
                webRequest.Timeout = 240000;
                if (method == "POST")
                {
                    webRequest.ContentType = "application/x-www-form-urlencoded";

                    if (!string.IsNullOrEmpty(argument))
                    {
                        byte[] arrByteData = Encoding.UTF8.GetBytes(argument);
                        webRequest.ContentLength = arrByteData.Length;

                        using (Stream reqStream = webRequest.GetRequestStream())
                        {
                            reqStream.Write(arrByteData, 0, arrByteData.Length);
                            arrByteData = null;
                        }
                    }
                    else
                    {
                        webRequest.ContentLength = 0;
                    }
                }

                using (webResponse = webRequest.GetResponse())
                {
                    using (responseStream = webResponse.GetResponseStream())
                    {
                        using (responseStreamReader = new StreamReader(responseStream))
                        {
                            responseResult = responseStreamReader.ReadToEnd();
                        }
                    }
                }

                return responseResult;

            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "UploadFile " + ex.Message + "\t" + ex.StackTrace);
                return "";
            }

        }
       

        private void CreateDirectory(DateTime TradeDate, string LocalHistoryFilePath)
        {
            try
            {
               
                if (Directory.Exists(LocalHistoryFilePath + @"\Log\") == false)
                {
                    Directory.CreateDirectory(LocalHistoryFilePath + @"Log\");
                }

                if (Directory.Exists(LocalHistoryFilePath + @"\Log\" + TradeDate.ToString("dd.MMM.yyyy")) == false)
                {
                    Directory.CreateDirectory(LocalHistoryFilePath + @"Log\" + TradeDate.ToString("dd.MMM.yyyy"));
                }

            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "CreateDirectory " + ex.Message + "\t" + ex.StackTrace);
            }

        }

        public static void SetCertificatePolicy()
        {
            if (ServicePointManager.ServerCertificateValidationCallback == null)
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(CertificateCallBack);
            }
        }

        private static bool CertificateCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        #endregion


        //private void UploadFileToServer(String ServerFolderName, string LocalFileName, string LocalBarFilePath)
        //{

        //    try
        //    {
        //        FTPclient objFTP = new FTPclient(FTPLocation, FTPUserName, FTPPassword);
        //        string FTPPath = FTPLocation + @"\" + ServerFolderName;
        //        objFTP.FtpCreateDirectory(FTPPath);
        //        objFTP.Upload(LocalBarFilePath + @"\" + LocalFileName, FTPPath + @"\" + LocalFileName);

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogEntry("Error", "UploadFildToServer Error while upload file to server File Name:" + LocalFileName + "\n" + ex.Message + "\t" + ex.StackTrace);
        //    }

        //}
    


    }
}
