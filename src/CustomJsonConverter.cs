﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(HOTASButtonMap))
            {
                var buttonMap = new HOTASButtonMap();
                serializer.Populate(reader, buttonMap);
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
                    Enum.TryParse(testValue, out HOTASButtonMap.ButtonType testType);

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
                dic.Add(key, list);
            }

            return dic;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }
    }
}
