using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushbulletVR.Extensions
{
    static class DoubleExtensions
    {
        // IsNanOrInfinity
        public static bool HasValue(this double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }
    }
}
