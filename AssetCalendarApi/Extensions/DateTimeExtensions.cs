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

        public static IEnumerable<DateTime> GetDatesInMonth( this DateTime date )
        {
            var start = new DateTime(date.Year, date.Month, 1);
            var end = new DateTime(date.Year, date.Month + 1, 1).AddDays(-1);

            for(DateTime d = start; d <= end; d = d.AddDays(1))
            {
                yield return d;
            }
        }

        public static IEnumerable<DateTime> GetDatesInWeek(this DateTime date )
        {
            for (DateTime d = date.StartOfWeek(); d <= date.EndOfWeek(); d = d.AddDays(1))
            {
                yield return d;
            }
        }
    }
}
