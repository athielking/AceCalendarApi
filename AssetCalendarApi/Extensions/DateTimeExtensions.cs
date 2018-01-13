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
            var end = new DateTime(date.Year, date.Month + 1, 1);

            return GetDatesBetween(start, end);
        }

        public static IEnumerable<DateTime> GetDatesInWeek(this DateTime date )
        {
            return GetDatesTo(date, date.EndOfWeek());
        }

        public static IEnumerable<DateTime> GetDatesBetween(this DateTime date, DateTime? end)
        {
            return GetDates(date, end, inclusive: false);
        }

        public static IEnumerable<DateTime> GetDatesTo(this DateTime date, DateTime? end)
        {
            return GetDates(date, end, inclusive: true);
        }

        private static IEnumerable<DateTime> GetDates(this DateTime date, DateTime? end, bool inclusive = false)
        {
            if (end < date)
                throw new InvalidOperationException("End date must be larger than start date");

            if (end == null || end == date)
                yield return date;
            else
            {
                for (DateTime d = date; d <= end; d = d.AddDays(1))
                {
                    if (d == end && !inclusive)
                        continue;

                    yield return d;
                }
            }

        }
    }
}
