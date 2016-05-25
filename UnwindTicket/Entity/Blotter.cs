using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnwindTicket.Entity
{
    public class BlotterLiveIdea
    {
        public long RowNo { get; set; }
        public long EventId { get; set; }
        public long OTICSIngestId { get; set; }
        public long IdeaId { get; set; }
        public string Company { get; set; }
        public string Author { get; set; }
        public string Ticker { get; set; }
        public string Direction { get; set; }
        public DateTime TransDate { get; set; }
        public int ClosingPeriod { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
        public double MktOpenPrice { get; set; }
        public double MktClosePrice { get; set; }
        public string PortwareStrategyId { get; set; }
        public string UnwindType { get; set; }
        public double UnwindValue { get; set; }
        public double Unwind { get; set; }
        public string Response { get; set; }
    }

    public class BlotterUnwindIdea
    {
        public long IdeaId { get; set; }
        public string PortwareStrategyId { get; set; }
        public string UnwindType { get; set; }
        public double UnwindValue { get; set; }
        public string Comment { get; set; }
    }
}
