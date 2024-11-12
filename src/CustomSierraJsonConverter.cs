using SierraHOTAS.Models;
using SierraJSON;
using System;
using System.Linq;
using System.Reflection;

namespace SierraHOTAS
{
    public class CustomSierraJsonConverter : ISierraJsonConverter
    {
        public bool CanConvertOnRead(Type objectType)
        {
            return objectType == typeof(IHotasBaseMap) ||
                   objectType == typeof(IHOTASDevice);
        }

        public bool CanConvertOnWrite(Type objectType)
        {
            return objectType == typeof(HOTASButton) ||
                   objectType == typeof(HOTASAxis) ||
                   objectType == typeof(ButtonAction) ||
                   objectType == typeof(ActionCatalogItem) ||
                   objectType == typeof(IHotasBaseMap) ||
                   objectType == typeof(IHOTASDevice);
        }

        public object ReadJson(Type objectType, object existingValue)
        {
            if (objectType == typeof(IHOTASDevice))
            {
                var device = new HOTASDevice();
                try
                {
                    Serializer.Populate(device);
                }
                catch (Exception e)
                {
                    Logging.Log.Error(e, "Failed to deserialize a HOTASDevice");
                    throw;
                }
                return device;
            }

            if (objectType == typeof(IHotasBaseMap))
            {
                var testValue = Serializer.GetToken("Type");
                Enum.TryParse(testValue, out HOTASButton.ButtonType testType);

                IHotasBaseMap map;
                switch (testType)
                {
                    case HOTASButton.ButtonType.AxisLinear:
                    case HOTASButton.ButtonType.AxisRadial:
                        map = new HOTASAxis();
                        break;
                    default:
                        map = new HOTASButton();
                        break;
                }
                Serializer.Populate(map);
                return map;
            }
            return existingValue;
        }

        public void WriteJson(object value)
        {
            if (value is ButtonAction action)
            {
                SerializeButtonAction(action);
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
                    SerializeButton(button);
                }
                return;
            }

            if (value is HOTASAxis axis)
            {
                if ((axis.ButtonMap == null || axis.ButtonMap.Count == 0) &&
                    (axis.ReverseButtonMap == null || axis.ReverseButtonMap.Count == 0)) return;

                SerializeAxis(axis);
                return;
            }

            if (value is ActionCatalogItem item)
            {
                if (item.Id == Guid.Empty) return;
                SerializeCatalogActionItem(item);
            }

            //if (!(value is Dictionary<int, ObservableCollection<IHotasBaseMap>>)) return;

            //writer needs to support object start and end, and object properties
            //Writer should be able to figure out that a comma is needed before writing the next property
            //writer should be able to figure out that a comma is needed between objects in a list/dictionary
        }

        private static void SerializeButtonAction(ButtonAction action)
        {
             var propList = SierraJsonAttributes.GetSerializableProperties(action);
            Serializer.WriteObjectStart();
            foreach (var prop in propList)
            {
                var value = prop.GetValue(action);

                if (prop.Name == nameof(action.IsKeyUp) && (bool)prop.GetValue(action) == false) continue;
                if (prop.Name == nameof(action.IsExtended) && (bool)prop.GetValue(action) == false) continue;
//                if (prop.Name == nameof(action.TimeInMilliseconds) && (int)prop.GetValue(action) == 0) continue;

                Serializer.WriteKeyValue(prop.Name, value);
            }
            Serializer.WriteObjectEnd();
        }

        private static void SerializeButton(HOTASButton button)
        {
            var propList = SierraJsonAttributes.GetSerializableProperties(button);
            Serializer.WriteObjectStart();
            foreach (var prop in propList)
            {
                var value = prop.GetValue(button);

//                if (prop.Name == nameof(button.ShiftModePage) && (int)prop.GetValue(button) == 0) continue;
//                if (prop.Name == nameof(button.RepeatCount) && (int)prop.GetValue(button) == 0) continue;
                if (prop.Name == nameof(button.IsShift) && (bool)prop.GetValue(button) == false) continue;
                if (prop.Name == nameof(button.IsOneShot) && (bool)prop.GetValue(button) == false) continue;
                if (prop.Name == nameof(button.ActionId) && !button.ActionCatalogItem.Actions.Any()) continue;

                Serializer.WriteKeyValue(prop.Name, value);
            }
            Serializer.WriteObjectEnd();
        }

        private static void SerializeAxis(HOTASAxis axis)
        {
            var propList = SierraJsonAttributes.GetSerializableProperties(axis);
            Serializer.WriteObjectStart();
            foreach (var prop in propList)
            {
                var value = prop.GetValue(axis);

                if (prop.Name == nameof(axis.IsDirectional) && (bool)prop.GetValue(axis) == true) continue;
                if (prop.Name == nameof(axis.IsMultiAction) && (bool)prop.GetValue(axis) == false) continue;
                if (prop.Name == nameof(axis.SoundFileName) && string.IsNullOrEmpty((string)prop.GetValue(axis))) continue;
                if (prop.Name == nameof(axis.SoundVolume) && Math.Abs((float)prop.GetValue(axis) - 1.0d) < 0.01) continue;

                var isNoHide = prop.GetCustomAttribute(typeof(SierraJsonNoHide));

                Serializer.WriteKeyValue(prop.Name, value, isNoHide != null);
            }
            Serializer.WriteObjectEnd();
        }

        private static void SerializeCatalogActionItem(ActionCatalogItem item)
        {
            var propList = SierraJsonAttributes.GetSerializableProperties(item);
            Serializer.WriteObjectStart();
            foreach (var prop in propList)
            {
                var value = prop.GetValue(item);

                Serializer.WriteKeyValue(prop.Name, value);
            }
            Serializer.WriteObjectEnd();
        }

    }
}
