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
                   objectType == typeof(ButtonAction) ||
                   objectType == typeof(ActionCatalogItem) ||
                   objectType == typeof(IHOTASDevice);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(ActionCatalogItem))
            {
                var item = new ActionCatalogItem();
                try
                {
                    serializer.Populate(reader, item);
                }
                catch (Exception e)
                {
                    Logging.Log.Error(e, "Failed to deserialize an ActionCatalogItem");
                    throw;
                }
                return item;
            }

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

            if (objectType == typeof(ButtonAction))
            {
                var action = new ButtonAction();
                try
                {
                    serializer.Populate(reader, action);
                }
                catch (Exception e)
                {
                    Logging.Log.Error(e, "Failed to deserialize a ButtonAction");
                    throw;
                }
                return action;
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

        //Don't serialize buttons that don't have any maps or custom changes
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ButtonAction action)
            {
                SerializeButtonAction(writer, serializer, action);
                return;
            }

            if (value is HOTASButton button)
            {
                if (button.Type == HOTASButton.ButtonType.Button && string.CompareOrdinal(button.MapName, JoystickOffsetValues.GetName(button.MapId)) != 0 ||
                    button.Type == HOTASButton.ButtonType.POV && string.CompareOrdinal(button.MapName, JoystickOffsetValues.GetPOVName(button.MapId)) != 0 ||
                    button.ActionCatalogItem?.Actions?.Count > 0 ||
                    button.ShiftModePage > 0 ||
                    button.IsShift)
                {
                    SerializeButton(writer, serializer, button);
                }
                return;
            }

            if (value is HOTASAxis axis)
            {
                if ((axis.ButtonMap == null || axis.ButtonMap.Count == 0) &&
                    (axis.ReverseButtonMap == null || axis.ReverseButtonMap.Count == 0)) return;

                SerializeAxis(writer, serializer, axis);
                return;
            }

            if (value is ActionCatalogItem item)
            {
                if (item.Id == Guid.Empty) return;
                SerializeCatalogActionItem(writer, serializer, item);
            }

            if (!(value is Dictionary<int, ObservableCollection<IHotasBaseMap>>)) return;

            serializer.Serialize(writer, value);
        }

        private static void SerializeButton(JsonWriter writer, JsonSerializer serializer, HOTASButton button)
        {
            var propList = typeof(HOTASButton).GetProperties();
            writer.WriteStartObject();
            foreach (var prop in propList)
            {
                if (prop.GetCustomAttributes(true).Any(x => x is JsonIgnoreAttribute)) continue;

                var value = prop.GetValue(button);

                if (prop.Name == nameof(button.ShiftModePage) && (int)prop.GetValue(button) == 0) continue;
                if (prop.Name == nameof(button.IsShift) && (bool)prop.GetValue(button) == false) continue;

                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, value);
            }
            writer.WriteEndObject();
        }

        private static void SerializeAxis(JsonWriter writer, JsonSerializer serializer, HOTASAxis axis)
        {
            var propList = typeof(HOTASAxis).GetProperties();
            writer.WriteStartObject();
            foreach (var prop in propList)
            {
                if (prop.GetCustomAttributes(true).Any(x => x is JsonIgnoreAttribute)) continue;

                var value = prop.GetValue(axis);

                if (prop.Name == nameof(axis.IsDirectional) && (bool)prop.GetValue(axis) == true) continue;
                if (prop.Name == nameof(axis.IsMultiAction) && (bool)prop.GetValue(axis) == false) continue;
                if (prop.Name == nameof(axis.SoundFileName) && string.IsNullOrEmpty((string)prop.GetValue(axis))) continue;
                if (prop.Name == nameof(axis.SoundVolume) && Math.Abs((double)prop.GetValue(axis) - 1.0d) < 0.01) continue;

                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, value);
            }
            writer.WriteEndObject();
        }

        private static void SerializeButtonAction(JsonWriter writer, JsonSerializer serializer, ButtonAction action)
        {
            var propList = typeof(ButtonAction).GetProperties();
            writer.WriteStartObject();
            foreach (var prop in propList)
            {
                if (prop.GetCustomAttributes(true).Any(x => x is JsonIgnoreAttribute)) continue;

                var value = prop.GetValue(action);

                if (prop.Name == nameof(action.IsKeyUp) && (bool)prop.GetValue(action) == false) continue;
                if (prop.Name == nameof(action.IsExtended) && (bool)prop.GetValue(action) == false) continue;
                if (prop.Name == nameof(action.TimeInMilliseconds) && (int)prop.GetValue(action) == 0) continue;

                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, value);
            }
            writer.WriteEndObject();
        }

        private static void SerializeCatalogActionItem(JsonWriter writer, JsonSerializer serializer, ActionCatalogItem item)
        {
            var propList = typeof(ActionCatalogItem).GetProperties();
            writer.WriteStartObject();
            foreach (var prop in propList)
            {
                if (prop.GetCustomAttributes(true).Any(x => x is JsonIgnoreAttribute)) continue;

                var value = prop.GetValue(item);

                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, value);
            }
            writer.WriteEndObject();
        }
    }
}

