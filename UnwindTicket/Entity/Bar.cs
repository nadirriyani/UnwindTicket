using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnwindTicket.Entity
{
    public class Bar
    {
        public String Ric { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public double Volume { get; set; }
        public double Amount { get; set; }
    }
}
