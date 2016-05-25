using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnwindTicket.BAL
{
    public static class IdeaExitCalc
    {
        public static double GetTakeProfitsVsMarketExit(double entry, double exit, double mentry, double mexit,string direction, double exitPercentvalue)
        {
            try
            {
                if (entry == 0 || exit == 0 || mentry == 0 || mexit == 0) return 0;
                double ret = exit / entry - 1.0;
                double retMrkt = mexit / mentry - 1.0;
                double retvsMrkt = (ret - retMrkt) * (direction.ToUpper() == "SHORT" ? -1 : 1);

                if ((retvsMrkt ) >= exitPercentvalue)
                    return retvsMrkt;
                else
                    return 0;
            }
            catch
            {
                return 0;
            } 
        }

        public static double GetStopLossVsMarketExit(double entry, double exit, double mentry, double mexit, string direction, double exitPercentvalue)
        {
            try
            {
                if (entry == 0 || exit == 0 || mentry == 0 || mexit == 0) return 0;

                double ret = exit / entry - 1.0;
                double retMrkt = mexit / mentry - 1.0;
                double retvsMrkt = (ret - retMrkt) * (direction.ToUpper() == "SHORT" ? -1 : 1);

                if ((retvsMrkt) <= -exitPercentvalue)
                    return retvsMrkt;
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
