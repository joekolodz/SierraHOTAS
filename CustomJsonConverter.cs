using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SierraHOTAS.Models;
using System;
using System.Collections.ObjectModel;

namespace SierraHOTAS
{
    public class CustomJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HOTASButtonMap) ||
                   objectType == typeof(HOTASAxisMap);
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var list = new ObservableCollection<IHotasBaseMap>();
            var jsonArray = JArray.Load(reader);

            foreach (var jsonObject in jsonArray)
            {
                var map = default(IHotasBaseMap);
                var testValue = jsonObject.Value<string>("Type");
                HOTASButtonMap.ButtonType testType;
                Enum.TryParse(testValue, out testType);
                try
                {
                    switch (testType)
                    {
                        case HOTASButtonMap.ButtonType.AxisLinear:
                        case HOTASButtonMap.ButtonType.AxisRadial:
                            map = new HOTASAxisMap();
                            serializer.Populate(jsonObject.CreateReader(), map);
                            break;
                        default:
                            map = new HOTASButtonMap();
                            serializer.Populate(jsonObject.CreateReader(), map);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                list.Add(map);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }
    }
}
