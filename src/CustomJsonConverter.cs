using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SierraHOTAS
{
    public class CustomJsonConverter : JsonConverter
    {
        public override bool CanWrite => true;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HOTASButton) ||
                   objectType == typeof(HOTASAxis) ||
                   objectType == typeof(IHOTASDevice);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(IHOTASDevice))
            {
                var device = new HOTASDevice();
                try
                {
                    serializer.Populate(reader, device);
                }
                catch (Exception e)
                {
                    Logging.Log.Error(e, "Failed to deserialize a HOTASDevice");
                    throw;
                }
                return device;
            }

            if (objectType == typeof(HOTASButton))
            {
                var buttonMap = new HOTASButton();
                try
                {
                    serializer.Populate(reader, buttonMap);
                }
                catch (Exception e)
                {
                    Logging.Log.Error(e, "Failed to deserialize a HOTASButton");
                    throw;
                }
                return buttonMap;
            }

            var dic = new Dictionary<int, ObservableCollection<IHotasBaseMap>>();

            var jsonDictionary = JObject.Load(reader);

            foreach (var kv in jsonDictionary)
            {
                var key = int.Parse(kv.Key);
                var jsonArray = kv.Value;
                var list = new ObservableCollection<IHotasBaseMap>();

                foreach (var jsonObject in jsonArray)
                {
                    IHotasBaseMap map;
                    var testValue = jsonObject.Value<string>("Type");
                    Enum.TryParse(testValue, out HOTASButton.ButtonType testType);

                    try
                    {
                        switch (testType)
                        {
                            case HOTASButton.ButtonType.AxisLinear:
                            case HOTASButton.ButtonType.AxisRadial:
                                map = new HOTASAxis();
                                serializer.Populate(jsonObject.CreateReader(), map);
                                break;
                            default:
                                map = new HOTASButton();
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
                dic.Add(key, list);
            }

            return dic;
        }


        //Don't serialize buttons that don't have any maps
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            if (value is HOTASButton button)
            {
                if ((string.Compare(button.MapName, JoystickOffsetValues.GetName(button.MapId)) == 0 ||
                    string.Compare(button.MapName, JoystickOffsetValues.GetPOVName(button.MapId)) == 0) &&
                    (button.ActionCatalogItem == null || button.ActionCatalogItem.Actions == null || button.ActionCatalogItem.Actions.Count == 0)) return;

                SerializeButton(writer, serializer, typeof(HOTASButton), button);
            }

            if (value is HOTASAxis axis)
            {
                if ((axis.ButtonMap == null || axis.ButtonMap.Count == 0) &&
                    (axis.ReverseButtonMap == null || axis.ReverseButtonMap.Count == 0)) return;

                SerializeButton(writer, serializer, typeof(HOTASAxis), axis);
            }

            if ((!(value is Dictionary<int, ObservableCollection<IHotasBaseMap>> profiles))) return;


            serializer.Serialize(writer, value);
        }

        private static void SerializeButton(JsonWriter writer, JsonSerializer serializer, Type t, IHotasBaseMap axis)
        {
            var propList = t.GetProperties();
            writer.WriteStartObject();
            foreach (var prop in propList)
            {
                if (prop.GetCustomAttributes(true).Any(x => x is JsonIgnoreAttribute))
                {
                    continue;
                }

                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, prop.GetValue(axis));
            }
            writer.WriteEndObject();
        }
    }
}
