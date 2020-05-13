using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Markup;

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

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private static void PopulateOffsetIndexLookup()
        {
            //build map of offset indexes and the offset's string name to get them by
            IndexLookup = new Dictionary<string, int>();
            //var names = Enum.GetNames(typeof(JoystickOffset));
            //var values = (int[])Enum.GetValues(typeof(JoystickOffset));

            foreach (var o in Offsets)
            {
                IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), o), (int)o);
            }

            //            var i = 0;
            //            foreach (var n in Enum.GetNames(typeof(JoystickOffset)))
            //            {
            ////                IndexLookup.Add(n, );
            //            }









            //manually insert the first 12 because they're deltas are not contiguous increments of 1
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.X), (int)JoystickOffset.X);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.Y), (int)JoystickOffset.Y);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.Z), (int)JoystickOffset.Z);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.RotationX), (int)JoystickOffset.RotationX);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.RotationY), (int)JoystickOffset.RotationY);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.RotationZ), (int)JoystickOffset.RotationZ);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.Sliders0), (int)JoystickOffset.Sliders0);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.Sliders1), (int)JoystickOffset.Sliders1);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.PointOfViewControllers0), (int)JoystickOffset.PointOfViewControllers0);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.PointOfViewControllers1), (int)JoystickOffset.PointOfViewControllers1);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.PointOfViewControllers2), (int)JoystickOffset.PointOfViewControllers2);
            IndexLookup.Add(Enum.GetName(typeof(JoystickOffset), JoystickOffset.PointOfViewControllers3), (int)JoystickOffset.PointOfViewControllers3);


            //for (var i = IndexLookup.Count; i < names.Length; i++)
            //{
            //    IndexLookup.Add(names[i],i);
            //}
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
        /// get cardinal index from offset enum (ie: 12 from JoystickOffset.Buttons55 [or 103])
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int GetIndex(int offset)
        {
            var buttonName = GetName(offset);
            return GetIndex(buttonName);
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
                Debug.WriteLine($"JoystickOffset does not contain a value at index: {index}.");
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
