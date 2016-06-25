using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushbulletVR.PushbulletAPI.Utils
{
    public static class EnumUtils
    {
        public static T ParseNonCaseSensitiveOrDefault<T>(string ToParse, T DefaultValue)
        {
            string[] names = Enum.GetNames(typeof(T));
            if (String.IsNullOrEmpty(ToParse) || !names.Any(x => x.ToLower().Equals(ToParse.ToLower())))
                    return DefaultValue;
            return (T)Enum.Parse(typeof(T), names.First(x => x.ToLower().Equals(ToParse.ToLower())));
        }
    }
}
