using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SierraHOTAS.Models
{
    static class JoystickOffsetValues
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
            PopulateCleanOffsetNameLookup();
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
            var i = 0;
            foreach (var n in Enum.GetNames(typeof(JoystickOffset)))
            {
                IndexLookup.Add(n, i++);
            }
        }

        //longs names renamed to shorter names
        private static Dictionary<JoystickOffset, string> _cleanOffsetNames;
        private static void PopulateCleanOffsetNameLookup()
        {
            _cleanOffsetNames = new Dictionary<JoystickOffset, string>
            {
                {JoystickOffset.RotationX, "RX"},
                {JoystickOffset.RotationY, "RY"},
                {JoystickOffset.RotationZ, "RZ"},
                {JoystickOffset.PointOfViewControllers0, "POV1"},
                {JoystickOffset.PointOfViewControllers1, "POV2"},
                {JoystickOffset.PointOfViewControllers2, "POV3"},
                {JoystickOffset.PointOfViewControllers3, "POV4"}
            };
        }

        /// <summary>
        /// get cardinal index from offset string name
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
        /// get offset enum from cardinal index lookup
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static JoystickOffset GetOffset(int index)
        {
            if (!OffsetLookup.ContainsKey(index))
            {
                Debug.WriteLine($"JoystickOffset does not contain a value at index: {index}.");
                return OffsetLookup[0];
            }

            OffsetLookup.TryGetValue(index, out var o);
            return o;
        }

        /// <summary>
        /// get offset string name from enum integer value (not index)
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetName(int offset)
        {
            return Enum.GetName(typeof(JoystickOffset), offset);
        }

        /// <summary>
        /// get clean (display) offset string name from offset enum value
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetName(JoystickOffset offset)
        {
            return Enum.GetName(typeof(JoystickOffset), offset);
        }
    }
}
