using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;

namespace PushbulletVR.Extensions
{
    public static class StringExtensions
    {
        /*
         * CODE GATHERED FROM: https://github.com/adamyeager/PushbulletSharp
         * PushSharp API
         * 
         * */

        /// <summary>
        /// To the json.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static string ToJson(this object data)
        {
            var serializer = new DataContractJsonSerializer(data.GetType());

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, data);
                return Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
            }
        }

        /// <summary>
        /// Jsons to ojbect.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static T JsonToOjbect<T>(this string json)
        {
            var bytes = Encoding.Unicode.GetBytes(json);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                var output = (T)serializer.ReadObject(stream);
                return output;
            }
        }
    }
}
