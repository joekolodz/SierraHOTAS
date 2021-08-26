﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SierraHOTAS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SierraHOTAS
{
    public class CustomJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;
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
                    Logging.Log.Error(e,"Failed to deserialize a HOTASDevice");
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


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }
    }
}
