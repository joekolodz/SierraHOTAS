using System;
using System.Text;

namespace SierraJSON
{

    public class Serializer
    {
        public static string ToJSON(object obj, ISierraJsonConverter converter)
        {
            if (obj == null) return "{}";

            Write.SetConverter(converter);

            var resultJSON = new StringBuilder();
            Write.Reset(resultJSON);

            Write.WriteValue(obj);

            return resultJSON.ToString();
        }

        public static T ToObject<T>(string json, ISierraJsonConverter converter)
        {
            Read.SetConverter(converter);
            return Read.ToObject<T>(json);
        }

        public static string GetToken(string name)
        {
            return Read.GetToken(name);
        }

        public static string GetToken(string name, string json)
        {
            return Read.GetToken(name, json);
        }

        public static void Populate(object target)
        {
           Read.Populate(target);
        }

        public static void WriteObjectStart()
        {
            Write.WriteObjectStart();
        }

        public static void WriteObjectEnd()
        {
            Write.WriteObjectEnd();
        }

        public static void WriteKeyValue(string key, object value)
        {
            Write.WriteKeyValue(key, value);
        }

        public class SierraJSONException : Exception
        {

        }

    }
}
