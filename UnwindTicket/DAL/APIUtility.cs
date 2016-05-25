using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnwindTicket.Entity;
using Newtonsoft.Json;


namespace UnwindTicket.DAL
{
    class APIUtility
    {
        private static string strException = "Data is not available, please contact to otassupport.";

        private static Tuple<HttpStatusCode, string> GetResponseFromApi(string endpoint, string queryString)
        {
            string strURL = ConfigurationManager.AppSettings["BaseAPIURL"] + "/";
            strURL += endpoint + (string.IsNullOrEmpty(queryString) ? "" : "?" + queryString);

            System.Net.WebRequest objReq = System.Net.WebRequest.Create(strURL);
            string strResponse = string.Empty;
            try
            {
                string apiKey = ConfigurationManager.AppSettings["BaseAPIKey"];
                string userName = ConfigurationManager.AppSettings["userName"];
                string password = ConfigurationManager.AppSettings["password"];

                ((HttpWebRequest)objReq).KeepAlive = false;
                ((HttpWebRequest)objReq).Headers.Add("HTTP_ACCEPT_ENCODING", "gzip,deflate,sdch");
                ((HttpWebRequest)objReq).Headers.Add("Authorization", apiKey);
                ((HttpWebRequest)objReq).Headers.Add("userName", userName);
                ((HttpWebRequest)objReq).Headers.Add("password", password);

                objReq.Method = "GET";
                objReq.Timeout = 150000;
                objReq.ContentLength = 0;
                using (WebResponse objResponse = ((HttpWebRequest)objReq).GetResponse())
                {
                    using (StreamReader objStreamReader = new StreamReader(objResponse.GetResponseStream()))
                    {
                        strResponse = objStreamReader.ReadToEnd();
                    }
                }
                if (strResponse.ToString().Contains("Inifinity") == true)
                {
                    strResponse = strResponse.Replace("Inifinity", "0").ToString();
                }
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.OK, strResponse);
            }
            catch (WebException webExcp)
            {
                if (webExcp.Response != null)
                {
                    using (WebResponse objResponse = webExcp.Response)
                    {
                        WebExceptionStatus status = webExcp.Status;
                        int wResp = ((HttpWebResponse)objResponse) != null ? (int)((HttpWebResponse)objResponse).StatusCode : 0;
                        if (status == WebExceptionStatus.ProtocolError)
                        {
                            using (Stream objStreamData = objResponse.GetResponseStream())
                            {
                                using (var objReader = new StreamReader(objStreamData))
                                {
                                    strException = objReader.ReadToEnd();
                                    if (!strException.Trim().StartsWith("{"))
                                    {
                                        Regex _removeComment = new Regex("(<.*?>\\s*)+", RegexOptions.Singleline);
                                        strException = _removeComment.Replace(strException, string.Empty);
                                    }
                                    try
                                    {
                                        JObject objJSON = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(strException);
                                        if (objJSON != null && objJSON["message"] != null)
                                            strException = Convert.ToString(objJSON["message"]);
                                        objJSON = null;
                                    }
                                    catch (Exception)
                                    { }
                                }
                            }
                            return new Tuple<HttpStatusCode, string>(((HttpWebResponse)objResponse).StatusCode, strException);
                        }
                        return new Tuple<HttpStatusCode, string>(((HttpWebResponse)objResponse).StatusCode, wResp + "-" + strException);
                    }
                }
                else
                {
                    throw webExcp;
                }
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "GetResponseFromApi: " + ex.Message + "\t" + ex.StackTrace);
                throw ex;
            }
            finally { objReq = null; }
        }

        private static string GetResponseFromApiPost(string endpoint, string postData, string type="form")
        {
            string strURL = ConfigurationManager.AppSettings["BaseAPIURL"] + "/";
            strURL += endpoint;

            System.Net.WebRequest objReq = System.Net.WebRequest.Create(strURL);
            string strResponse = string.Empty;
            try
            {
                string apiKey = ConfigurationManager.AppSettings["BaseAPIKey"];
                string userName = ConfigurationManager.AppSettings["userName"];
                string password = ConfigurationManager.AppSettings["password"];

                ((HttpWebRequest)objReq).KeepAlive = false;
                ((HttpWebRequest)objReq).Headers.Add("HTTP_ACCEPT_ENCODING", "gzip,deflate,sdch");
                ((HttpWebRequest)objReq).Headers.Add("Authorization", apiKey);
                ((HttpWebRequest)objReq).Headers.Add("userName", userName);
                ((HttpWebRequest)objReq).Headers.Add("password", password);

                var data = Encoding.ASCII.GetBytes(postData);
                objReq.Method = "POST";
                objReq.Timeout = 150000;
                if (type.ToLower() == "form")
                    objReq.ContentType = "application/x-www-form-urlencoded";
                else
                    objReq.ContentType = "application/json";
                objReq.ContentLength = data.Length;
                using (var stream = objReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                using (WebResponse objResponse = ((HttpWebRequest)objReq).GetResponse())
                {
                    using (StreamReader objStreamReader = new StreamReader(objResponse.GetResponseStream()))
                    {
                        strResponse = objStreamReader.ReadToEnd();
                    }
                }
                if (strResponse.ToString().Contains("Inifinity") == true)
                {
                    strResponse = strResponse.Replace("Inifinity", "0").ToString();
                }
                return strResponse;
            }
            catch (WebException exc)
            {
                if (exc.Response != null)
                {
                    using (WebResponse objResponse = exc.Response)
                    {
                        using (Stream objStreamData = objResponse.GetResponseStream())
                        {
                            using (var objReader = new StreamReader(objStreamData))
                            {
                                return objReader.ReadToEnd();
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogEntry("Error", "GetResponseFromApiPost: " + exc.Message + "\t" + exc.StackTrace);
                    throw exc;
                }
            }
            finally { objReq = null; }
        }

        internal static List<BlotterLiveIdea> GetBlotterLiveIdeas()
        {
            try
            {
                string strEndPoint = string.Empty, strParameters = string.Empty;
                strEndPoint = "/blotterliveideas";

                var tplResponse = GetResponseFromApi(strEndPoint, strParameters);
                var lstBlotterLiveIdeas = new List<BlotterLiveIdea>();
                if (tplResponse.Item1 == HttpStatusCode.OK)
                {
                    lstBlotterLiveIdeas = JsonConvert.DeserializeObject<List<BlotterLiveIdea>>(tplResponse.Item2);
                }
                else
                {
                    Logger.LogEntry("Information", "GetBlotterLiveIdeas: " + tplResponse.Item1 + "\n" + tplResponse.Item2); 
                    // no data found write log
                    // tplResponse.Item2
                }
                return lstBlotterLiveIdeas;
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "GetBlotterLiveIdeas: " + ex.Message + "\t" + ex.StackTrace);
                return null;
            }
        }


        internal static string BlotterUnwindIdeaAdd(Int64 IdeaId, string PortwareStrategyId, string UnwindType, double UnwindValue, string Comment)
        {
            try
            {
                string strEndPoint = string.Empty, strParameters = string.Empty;
                strEndPoint = "/blotterunwindidea/add";

                var objBlotterUnwindIdea = new BlotterUnwindIdea();
                objBlotterUnwindIdea.IdeaId = IdeaId;
                objBlotterUnwindIdea.PortwareStrategyId = PortwareStrategyId;
                objBlotterUnwindIdea.Comment = Comment;
                objBlotterUnwindIdea.UnwindType = UnwindType;
                objBlotterUnwindIdea.UnwindValue = UnwindValue;
                
                string postData = JsonConvert.SerializeObject(objBlotterUnwindIdea);
                return GetResponseFromApiPost(strEndPoint, postData, "json");
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "BlotterUnwindIdeaAdd: " + ex.Message + "\t" + ex.StackTrace);
                throw;
            }
        }

    }
}
