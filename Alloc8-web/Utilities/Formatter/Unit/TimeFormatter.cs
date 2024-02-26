using System;

namespace Alloc8_web.Utilities.Formatter
{
        
    public class TimeFormatter
    {
        protected TimeZoneInfo? _timeZone;
        public TimeFormatter()
        {
            this._timeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault();
        }
        public void byTimeZone(string? timeZoneName)
        {
            if(string.IsNullOrEmpty(timeZoneName))
            {
                return;
            }
            this._timeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x=>x.Id == timeZoneName); 
        }
        public  DateTime convert(DateTime? dateTime)
        {
            if(dateTime == null)
            {
                return DateTime.UtcNow;
            }
            if(_timeZone == null)
            {
                return (DateTime)dateTime;
            }
            return (DateTime)dateTime + _timeZone.BaseUtcOffset;
        }
        public string? human(DateTime? dateTime)
        {

            if(dateTime == null)
            {
                return DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss");
            }
            if(_timeZone == null)
            {
                return dateTime.ToString();
            }
            DateTime? userTime = dateTime + _timeZone.BaseUtcOffset;
            return userTime?.ToString("dd-MMM-yyyy HH:mm:ss");
        }
        public DateTime reverse(DateTime dateTime)
        {
            if(_timeZone == null)
            {
                return dateTime;
            }
            return dateTime - _timeZone.BaseUtcOffset;
        }
    }
}
