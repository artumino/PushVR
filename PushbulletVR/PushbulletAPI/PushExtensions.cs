using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushbulletVR.PushbulletAPI
{
    public static class PushExtensions
    {
        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static void AddRange<TK, TV>(this IDictionary<TK, TV> dict, IDictionary<TK,TV> ValuesToAdd)
        {
            foreach(TK key in ValuesToAdd.Keys)
                dict.Add(key, ValuesToAdd[key]);
        }
    }
}
