using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime date)
        {
            var sunday = date;
            if (date.DayOfWeek != DayOfWeek.Sunday)
                sunday = date.AddDays((-1 * (int)date.DayOfWeek));

            return sunday;
        }

        public static DateTime EndOfWeek(this DateTime date )
        {
            var saturday = date;
            if (date.DayOfWeek != DayOfWeek.Saturday)
                saturday = date.AddDays((int)DayOfWeek.Saturday - (int)date.DayOfWeek);

            return saturday;
        }
    }
}
