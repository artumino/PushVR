using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushbulletVR.PushbulletAPI
{
    public class PushUtils
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static double DateTimeToUnixTimeStamp(DateTime date)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return (date.ToUniversalTime() - dtDateTime).TotalSeconds;
        }

        public static double ParseFloatTime(string floatTime)
        {
            return Double.Parse(floatTime, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ConvertToFloatTime(double doubleTime)
        {
            return doubleTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
