using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UnwindTicket.Entity;

namespace UnwindTicket.DAL
{
    public static class DataFetching
    {

        public  class IdeaData
        {
            public Int64 No {get; set;}
            public Int64 EventId { get; set; }
            public Int64 OTICSIngestId { get; set; }
            public Int64 IdeaID { get; set; }
            public string Company { get; set; }
            public string Author { get; set; }
            public DateTime TransDate { get; set; }
            public string Ticker { get; set; }
            public string Direction { get; set; }
            public int ClosingPeriod { get; set; }
            public double OpenPrice  {get; set;}
            public double ClosePrice {get; set;}
            public double MktOpenPrice  {get; set;}
            public double MktClosePrice {get; set;}
            public string UnwindType {get; set;}
            public double Value {get; set;}
            public double Unwind { get; set; }
            public string PortwareStrategyId { get; set; }
           
        }


        public static List<BlotterLiveIdea> getOpenIdeaList()
        {
            try
            {

                List<BlotterLiveIdea> ideas =   APIUtility.GetBlotterLiveIdeas();
                return ideas;
                //SqlConnection cn = new SqlConnection("Data Source=144.76.57.47;Initial Catalog=OTASBASE_1_9_4;Persist Security Info=False;User ID=OTAS;pwd=0livetree870Tas;Connection Timeout=10000; MultipleActiveResultSets=True");
                //SqlCommand cmd = new SqlCommand("getOpenIdeaByClient", cn);
                //cmd.CommandType = CommandType.StoredProcedure;
                //SqlParameter param1 = new SqlParameter();
                //param1.ParameterName = "@ClientId";
                //param1.Value = "239";
                //cmd.Parameters.Add(param1);
                //SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                //DataSet ds = new DataSet();
                //adpt.Fill(ds);

                //var list = ds.Tables[0].AsEnumerable()
                //        .Select(dr =>
                //        new IdeaData
                //        {
                //             No = dr.Field<Int64>("RowNo"),
                //             IdeaID= dr.Field<Int64>("IdeaId"),
                //             OTICSIngestId = dr.Field<Int64>("OTICSIngestID"),
                //             Company = dr.Field<string>("Company"),
                //             Author = dr.Field<string>("Author"),
                //             Ticker = dr.Field<string>("Ticker"),
                //             Direction = dr.Field<string>("Direction"),
                //             TransDate = dr.Field<DateTime>("TransDate"),
                //             ClosingPeriod = dr.Field<Int32>("ClosingPeriod"),
                //             OpenPrice = dr.Field<double>("OpenPrice"),
                //             ClosePrice = dr.Field<double>("ClosePrice"),
                //             MktOpenPrice = dr.Field<double>("MktOpenPrice"),
                //             MktClosePrice = dr.Field<double>("MktClosePrice"),
                //             PortwareStrategyId = dr.Field<string>("PortwareStrategyId"),
                //             UnwindType = dr.Field<string>("UnwindType"),
                //             Value = dr.Field<double>("Value")
                //        }
                //        ).ToList();


                //return list;
            }
            catch (Exception ex)
            {
                Logger.LogEntry("Error", "getOpenIdeaList: " + ex.Message + "\t" + ex.StackTrace);
                return new List<BlotterLiveIdea>();
            }

        }


    }
}
