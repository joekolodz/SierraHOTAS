using System;
using System.Collections.Generic;
using System.Linq;

namespace SierraHOTAS.Models
{
    static public class JoystickOffsetValues
    {
        public enum ButtonState
        {
            ButtonReleased = 0,
            ButtonPressed = 128
        }

        public enum PointOfViewPositionValues
        {
            Released = -1,
            POVNorth = 0,
            POVNorthEast = 4500,
            POVEast = 9000,
            POVSouthEast = 13500,
            POVSouth = 18000,
            POVSouthWest = 22500,
            POVWest = 27000,
            POVNorthWest = 31500
        }

        private const int ButtonStartOffset = (int)JoystickOffset.Button1;

        //DirectX enum for joystick offset names restructured into a dictionary to use as a lookup
        private static Dictionary<int, JoystickOffset> OffsetLookup { get; set; }
        private static Dictionary<string, int> IndexLookup { get; set; }
        public static JoystickOffset[] Offsets { get; set; }

        /// <summary>
        /// Get the SharpDX enum value list as an array
        /// </summary>
        static JoystickOffsetValues()
        {
            Offsets = (JoystickOffset[])Enum.GetValues(typeof(JoystickOffset));
            PopulateOffsetIndexLookup();
            PopulateOffsetNameLookup();
        }

        private static void PopulateOffsetNameLookup()
        {
            //build map of offset enumerated names and a cardinal(numerical sequence) index to get them by. IE Button0 has a value of 45, but we want to return a value of 1 for display since that is effectively the first button
            var index = 0;
            OffsetLookup = Offsets.ToDictionary(name => index++);
        }

        private static void PopulateOffsetIndexLookup()
        {
            //build map of offset indexes and the offset's string name to get them by
            IndexLookup = new Dictionary<string, int>();
            var names = Enum.GetNames(typeof(JoystickOffset));

            for (var i = IndexLookup.Count; i < names.Length; i++)
            {
                IndexLookup.Add(names[i], i);
            }
        }

        /// <summary>
        /// get cardinal index from offset string name (ie: 12 from "Buttons55")
        /// </summary>
        /// <param name="offsetName"></param>
        /// <returns></returns>
        public static int GetIndex(string offsetName)
        {
            if (!IndexLookup.ContainsKey(offsetName))
                throw new ArgumentOutOfRangeException($"JoystickOffset does not contain the name: {offsetName}.");

            IndexLookup.TryGetValue(offsetName, out var o);
            return o;
        }

        /// <summary>
        /// Returns a 0 based index for the raw button offset range. (ie: MapId of 48 (Buttons0) returns 0, MapId of 103 (Buttons55) returns 55)
        /// Joystick.GetCurrentState is a 0 based array for the list of each capability, so Buttons0 is found at index 0 so we can't use the directX offset values
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        public static int GetButtonIndexForJoystickState(int mapId)
        {
            if (mapId < ButtonStartOffset) return 0;
            if (mapId > (int)JoystickOffset.Button128) return 127;

            return mapId - ButtonStartOffset;
        }

        /// <summary>
        /// get offset enum from cardinal index lookup (ie: JoystickOffset.Buttons0 from 12)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static JoystickOffset GetOffset(int index)
        {
            if (!OffsetLookup.ContainsKey(index))
            {
                Logging.Log.Debug($"JoystickOffset does not contain a value at index: {index}.");
                return OffsetLookup[0];
            }

            OffsetLookup.TryGetValue(index, out var o);
            return o;
        }

        /// <summary>
        /// get offset string name from enum integer value (not index) (ie: "Buttons55" from 103)
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetName(int offset)
        {
            return Enum.GetName(typeof(JoystickOffset), offset);
        }

        /// <summary>
        /// get clean (display) offset string name from offset enum value. (ie: "Buttons55" from JoystickOffset.Buttons55)
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetName(JoystickOffset offset)
        {
            return Enum.GetName(typeof(JoystickOffset), offset);
        }
    }
}
