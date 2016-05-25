using System;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Bloomberglp.Blpapi;

namespace UnwindTicket
{
    internal class BloombergLive
    {

        private const string APIMKTDATA_SVC = "//blp/refdata";
        private Bloomberglp.Blpapi.SessionOptions sessionOptions;// = new Bloomberglp.Blpapi.SessionOptions();

        private Bloomberglp.Blpapi.Session session;// = new Bloomberglp.Blpapi.Session(sessionOptions);

        public BloombergLive()
        {
            sessionOptions = new Bloomberglp.Blpapi.SessionOptions();
            session = new Bloomberglp.Blpapi.Session(sessionOptions);
        }

        public bool isBloomergInstalled()
        {
            try
            {
                if (!session.Start())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool isBloomergUserLogged()
        {
            try
            {
                if (!session.OpenService("//blp/refdata"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bbIdentifiers GetStockPrice(bbIdentifiers identifiers)
        {
            try
            {

                if (isBloomergInstalled() == true & isBloomergUserLogged() == true)
                {
                    Service refDataSvc = session.GetService(APIMKTDATA_SVC);
                    Bloomberglp.Blpapi.Request request = refDataSvc.CreateRequest("HistoricalDataRequest");

                    for (int iRow = 0; iRow <= identifiers.Count - 1; iRow++)
                    {
                        //request.GetElement("securities").AppendValue(identifiers[iRow].Ticker.ToString() + " Equity");
                        request.GetElement("securities").AppendValue(identifiers[iRow].Ticker.ToString());
                    }

                    request.GetElement("fields").AppendValue("LAST_PRICE");

                    request.Set("currency", "USD");
                    request.Set("startDate", System.DateTime.UtcNow.ToString("yyyyMMdd"));
                    request.Set("endDate", System.DateTime.UtcNow.ToString("yyyyMMdd"));

                    session.SendRequest(request, new CorrelationID(1));

                    bool continueToLoop = true;
                    while (continueToLoop == true)
                    {
                        Event eventObj = session.NextEvent();
                        //' final response
                        if (eventObj.Type == Event.EventType.RESPONSE)
                        {
                            continueToLoop = false;
                            handleResponseData(eventObj, identifiers);
                            break; // TODO: might not be correct. Was : Exit While
                        }
                        else if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                        {
                            handleResponseData(eventObj, identifiers);
                        }
                    }

                    return identifiers;
                }
                else
                {
                    return new bbIdentifiers();
                }
            }
            catch (Exception ex)
            {
                return new bbIdentifiers();
                throw ex;
            }
        }

        private static readonly Name EXCEPTIONS = new Name("exceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name REASON = new Name("reason");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name DESCRIPTION = new Name("description");
        private static readonly Name ERROR_CODE = new Name("errorCode");
        private static readonly Name SOURCE = new Name("source");
        private static readonly Name SECURITY_ERROR = new Name("securityError");
        private static readonly Name MESSAGE = new Name("message");
        private static readonly Name RESPONSE_ERROR = new Name("responseError");
        private static readonly Name SECURITY_DATA = new Name("securityData");
        private static readonly Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
        private static readonly Name ERROR_INFO = new Name("errorInfo");

        private void handleResponseData(Event eventObj, bbIdentifiers identifiers)
        {
            try
            {

                string securityName = string.Empty;
                Boolean hasFieldError = false;
                // process message
                foreach (Message msg in eventObj)
                {
                    if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("HistoricalDataResponse")))
                    {
                        // process errors
                        if (msg.HasElement(RESPONSE_ERROR))
                        {
                            Element error = msg.GetElement(RESPONSE_ERROR);
                            identifiers = new bbIdentifiers();
                            return;
                        }
                        else
                        {
                            Element secDataArray = msg.GetElement(SECURITY_DATA);
                            int numberOfSecurities = secDataArray.NumValues;
                            if (secDataArray.HasElement(SECURITY_ERROR))
                            {
                                // security error
                                Element secError = secDataArray.GetElement(SECURITY_ERROR);
                                identifiers = new bbIdentifiers();
                                return;
                            }
                            if (secDataArray.HasElement(FIELD_EXCEPTIONS))
                            {
                                // field error
                                Element error = secDataArray.GetElement(FIELD_EXCEPTIONS);
                                for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++)
                                {
                                    Element errorException = error.GetValueAsElement(errorIndex);
                                    string field = errorException.GetElementAsString(FIELD_ID);
                                    Element errorInfo = errorException.GetElement(ERROR_INFO);
                                    string message = errorInfo.GetElementAsString(MESSAGE);
                                    hasFieldError = true;

                                    identifiers = new bbIdentifiers();
                                    return;

                                } // end for 
                            } // end if
                            // process securities data
                            for (int index = 0; index < numberOfSecurities; index++)
                            {
                                foreach (Element secData in secDataArray.Elements)
                                {
                                    switch (secData.Name.ToString())
                                    {
                                        case "fieldData":
                                            if (hasFieldError && secData.NumValues == 0)
                                            {
                                                identifiers = new bbIdentifiers();
                                                return;
                                            }
                                            else
                                            {
                                                // get field data
                                                for (int pointIndex = 0; pointIndex < secData.NumValues; pointIndex++)
                                                {
                                                    Element fields = secData.GetValueAsElement(pointIndex);
                                                    identifiers[0].Price = fields.GetElementAsFloat64("LAST_PRICE");
                                                    break;
                                                } // end for
                                            }
                                            break;
                                    } // end switch
                                } // end foreach
                            } // end for
                        } // end else 
                    } // end if
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public decimal getIntradayChange(decimal OldPrice, decimal NewPrice)
        {
            try
            {
                //' to avoid 
                if (OldPrice != 0)
                {
                    return ((NewPrice - OldPrice) / OldPrice) * 100;
                }
                else return (decimal)0.0;
            }
            catch (Exception ex)
            {
                return (decimal)0.0;
            }
        }

    }

    #region " Data Object Class "

    public class bbIdentifiers : List<bbIdentifier>
    {
    }

    public class bbIdentifier
    {

        #region "Declaration"
        private string _ticker = string.Empty;
        private double _price = 0.0;
        private double _priceChange = 0.0;
        #endregion
        private string _QUOTE = string.Empty;

        #region "Properties"
        public string Ticker
        {
            get { return _ticker; }
            set { _ticker = value; }
        }

        public double Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public double PriceChange
        {
            get { return _priceChange; }
            set { _priceChange = value; }
        }

        public string QUOTE
        {
            get { return _QUOTE; }
            set { _QUOTE = value; }
        }

        #endregion

        #region "Constructor"
        public bbIdentifier(string Ticker, double Price, double PriceChange)
        {
            this._ticker = Ticker;
            this.Price = Price;
            this.PriceChange = PriceChange;
        }
        #endregion

    }

    #endregion

}
