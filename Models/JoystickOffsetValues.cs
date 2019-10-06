using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;

namespace SierraHOTAS.Models
{
    
    static class JoystickOffsetValues
    {
        public enum Names
        {
            X,
            Y,
            Z,
            RX,
            RY,
            RZ,
            Sliders0,
            Sliders1,
            POV0,
            POV1,
            POV2,
            POV3,
            Buttons0,
            Buttons1,
            Buttons2,
            Buttons3,
            Buttons4,
            Buttons5,
            Buttons6,
            Buttons7,
            Buttons8,
            Buttons9,
            Buttons10,
            Buttons11,
            Buttons12,
            Buttons13,
            Buttons14,
            Buttons15,
            Buttons16,
            Buttons17,
            Buttons18,
            Buttons19,
            Buttons20,
            Buttons21,
            Buttons22,
            Buttons23,
            Buttons24,
            Buttons25,
            Buttons26,
            Buttons27,
            Buttons28,
            Buttons29,
            Buttons30,
            Buttons31,
            Buttons32,
            Buttons33,
            Buttons34,
            Buttons35,
            Buttons36,
            Buttons37,
            Buttons38,
            Buttons39,
            Buttons40,
            Buttons41,
            Buttons42,
            Buttons43,
            Buttons44,
            Buttons45,
            Buttons46,
            Buttons47,
            Buttons48,
            Buttons49,
            Buttons50,
            Buttons51,
            Buttons52,
            Buttons53,
            Buttons54,
            Buttons55,
            Buttons56,
            Buttons57,
            Buttons58,
            Buttons59,
            Buttons60,
            Buttons61,
            Buttons62,
            Buttons63,
            Buttons64,
            Buttons65,
            Buttons66,
            Buttons67,
            Buttons68,
            Buttons69,
            Buttons70,
            Buttons71,
            Buttons72,
            Buttons73,
            Buttons74,
            Buttons75,
            Buttons76,
            Buttons77,
            Buttons78,
            Buttons79,
            Buttons80,
            Buttons81,
            Buttons82,
            Buttons83,
            Buttons84,
            Buttons85,
            Buttons86,
            Buttons87,
            Buttons88,
            Buttons89,
            Buttons90,
            Buttons91,
            Buttons92,
            Buttons93,
            Buttons94,
            Buttons95,
            Buttons96,
            Buttons97,
            Buttons98,
            Buttons99,
            Buttons100,
            Buttons101,
            Buttons102,
            Buttons103,
            Buttons104,
            Buttons105,
            Buttons106,
            Buttons107,
            Buttons108,
            Buttons109,
            Buttons110,
            Buttons111,
            Buttons112,
            Buttons113,
            Buttons114,
            Buttons115,
            Buttons116,
            Buttons117,
            Buttons118,
            Buttons119,
            Buttons120,
            Buttons121,
            Buttons122,
            Buttons123,
            Buttons124,
            Buttons125,
            Buttons126,
            Buttons127,
            VelocityX,
            VelocityY,
            VelocityZ,
            AngularVelocityX,
            AngularVelocityY,
            AngularVelocityZ,
            VelocitySliders0,
            VelocitySliders1,
            AccelerationX,
            AccelerationY,
            AccelerationZ,
            AngularAccelerationX,
            AngularAccelerationY,
            AngularAccelerationZ,
            AccelerationSliders0,
            AccelerationSliders1,
            ForceX,
            ForceY,
            ForceZ,
            TorqueX,
            TorqueY,
            TorqueZ,
            ForceSliders0,
            ForceSliders1
        }

        public enum JoystickOffsetCleanNames
        {
            X = 0,
            Y = 4,
            Z = 8,
            RX = 12, // 0x0000000C
            RY = 16, // 0x00000010
            RZ = 20, // 0x00000014
            Sliders0 = 24, // 0x00000018
            Sliders1 = 28, // 0x0000001C
            POV0 = 32, // 0x00000020
            POV1 = 36, // 0x00000024
            POV2 = 40, // 0x00000028
            POV3 = 44, // 0x0000002C
            Buttons0 = 48, // 0x00000030
            Buttons1 = 49, // 0x00000031
            Buttons2 = 50, // 0x00000032
            Buttons3 = 51, // 0x00000033
            Buttons4 = 52, // 0x00000034
            Buttons5 = 53, // 0x00000035
            Buttons6 = 54, // 0x00000036
            Buttons7 = 55, // 0x00000037
            Buttons8 = 56, // 0x00000038
            Buttons9 = 57, // 0x00000039
            Buttons10 = 58, // 0x0000003A
            Buttons11 = 59, // 0x0000003B
            Buttons12 = 60, // 0x0000003C
            Buttons13 = 61, // 0x0000003D
            Buttons14 = 62, // 0x0000003E
            Buttons15 = 63, // 0x0000003F
            Buttons16 = 64, // 0x00000040
            Buttons17 = 65, // 0x00000041
            Buttons18 = 66, // 0x00000042
            Buttons19 = 67, // 0x00000043
            Buttons20 = 68, // 0x00000044
            Buttons21 = 69, // 0x00000045
            Buttons22 = 70, // 0x00000046
            Buttons23 = 71, // 0x00000047
            Buttons24 = 72, // 0x00000048
            Buttons25 = 73, // 0x00000049
            Buttons26 = 74, // 0x0000004A
            Buttons27 = 75, // 0x0000004B
            Buttons28 = 76, // 0x0000004C
            Buttons29 = 77, // 0x0000004D
            Buttons30 = 78, // 0x0000004E
            Buttons31 = 79, // 0x0000004F
            Buttons32 = 80, // 0x00000050
            Buttons33 = 81, // 0x00000051
            Buttons34 = 82, // 0x00000052
            Buttons35 = 83, // 0x00000053
            Buttons36 = 84, // 0x00000054
            Buttons37 = 85, // 0x00000055
            Buttons38 = 86, // 0x00000056
            Buttons39 = 87, // 0x00000057
            Buttons40 = 88, // 0x00000058
            Buttons41 = 89, // 0x00000059
            Buttons42 = 90, // 0x0000005A
            Buttons43 = 91, // 0x0000005B
            Buttons44 = 92, // 0x0000005C
            Buttons45 = 93, // 0x0000005D
            Buttons46 = 94, // 0x0000005E
            Buttons47 = 95, // 0x0000005F
            Buttons48 = 96, // 0x00000060
            Buttons49 = 97, // 0x00000061
            Buttons50 = 98, // 0x00000062
            Buttons51 = 99, // 0x00000063
            Buttons52 = 100, // 0x00000064
            Buttons53 = 101, // 0x00000065
            Buttons54 = 102, // 0x00000066
            Buttons55 = 103, // 0x00000067
            Buttons56 = 104, // 0x00000068
            Buttons57 = 105, // 0x00000069
            Buttons58 = 106, // 0x0000006A
            Buttons59 = 107, // 0x0000006B
            Buttons60 = 108, // 0x0000006C
            Buttons61 = 109, // 0x0000006D
            Buttons62 = 110, // 0x0000006E
            Buttons63 = 111, // 0x0000006F
            Buttons64 = 112, // 0x00000070
            Buttons65 = 113, // 0x00000071
            Buttons66 = 114, // 0x00000072
            Buttons67 = 115, // 0x00000073
            Buttons68 = 116, // 0x00000074
            Buttons69 = 117, // 0x00000075
            Buttons70 = 118, // 0x00000076
            Buttons71 = 119, // 0x00000077
            Buttons72 = 120, // 0x00000078
            Buttons73 = 121, // 0x00000079
            Buttons74 = 122, // 0x0000007A
            Buttons75 = 123, // 0x0000007B
            Buttons76 = 124, // 0x0000007C
            Buttons77 = 125, // 0x0000007D
            Buttons78 = 126, // 0x0000007E
            Buttons79 = 127, // 0x0000007F
            Buttons80 = 128, // 0x00000080
            Buttons81 = 129, // 0x00000081
            Buttons82 = 130, // 0x00000082
            Buttons83 = 131, // 0x00000083
            Buttons84 = 132, // 0x00000084
            Buttons85 = 133, // 0x00000085
            Buttons86 = 134, // 0x00000086
            Buttons87 = 135, // 0x00000087
            Buttons88 = 136, // 0x00000088
            Buttons89 = 137, // 0x00000089
            Buttons90 = 138, // 0x0000008A
            Buttons91 = 139, // 0x0000008B
            Buttons92 = 140, // 0x0000008C
            Buttons93 = 141, // 0x0000008D
            Buttons94 = 142, // 0x0000008E
            Buttons95 = 143, // 0x0000008F
            Buttons96 = 144, // 0x00000090
            Buttons97 = 145, // 0x00000091
            Buttons98 = 146, // 0x00000092
            Buttons99 = 147, // 0x00000093
            Buttons100 = 148, // 0x00000094
            Buttons101 = 149, // 0x00000095
            Buttons102 = 150, // 0x00000096
            Buttons103 = 151, // 0x00000097
            Buttons104 = 152, // 0x00000098
            Buttons105 = 153, // 0x00000099
            Buttons106 = 154, // 0x0000009A
            Buttons107 = 155, // 0x0000009B
            Buttons108 = 156, // 0x0000009C
            Buttons109 = 157, // 0x0000009D
            Buttons110 = 158, // 0x0000009E
            Buttons111 = 159, // 0x0000009F
            Buttons112 = 160, // 0x000000A0
            Buttons113 = 161, // 0x000000A1
            Buttons114 = 162, // 0x000000A2
            Buttons115 = 163, // 0x000000A3
            Buttons116 = 164, // 0x000000A4
            Buttons117 = 165, // 0x000000A5
            Buttons118 = 166, // 0x000000A6
            Buttons119 = 167, // 0x000000A7
            Buttons120 = 168, // 0x000000A8
            Buttons121 = 169, // 0x000000A9
            Buttons122 = 170, // 0x000000AA
            Buttons123 = 171, // 0x000000AB
            Buttons124 = 172, // 0x000000AC
            Buttons125 = 173, // 0x000000AD
            Buttons126 = 174, // 0x000000AE
            Buttons127 = 175, // 0x000000AF
            VelocityX = 176, // 0x000000B0
            VelocityY = 180, // 0x000000B4
            VelocityZ = 184, // 0x000000B8
            AngularVelocityX = 188, // 0x000000BC
            AngularVelocityY = 192, // 0x000000C0
            AngularVelocityZ = 196, // 0x000000C4
            VelocitySliders0 = 200, // 0x000000C8
            VelocitySliders1 = 204, // 0x000000CC
            AccelerationX = 208, // 0x000000D0
            AccelerationY = 212, // 0x000000D4
            AccelerationZ = 216, // 0x000000D8
            AngularAccelerationX = 220, // 0x000000DC
            AngularAccelerationY = 224, // 0x000000E0
            AngularAccelerationZ = 228, // 0x000000E4
            AccelerationSliders0 = 232, // 0x000000E8
            AccelerationSliders1 = 236, // 0x000000EC
            ForceX = 240, // 0x000000F0
            ForceY = 244, // 0x000000F4
            ForceZ = 248, // 0x000000F8
            TorqueX = 252, // 0x000000FC
            TorqueY = 256, // 0x00000100
            TorqueZ = 260, // 0x00000104
            ForceSliders0 = 264, // 0x00000108
            ForceSliders1 = 268, // 0x0000010C
        }
        public static JoystickOffset[] Offsets { get; set; }

        private static Dictionary<int, JoystickOffset> OffsetLookup { get; set; }
        private static Dictionary<string, int> IndexLookup { get; set; }

        /// <summary>
        /// Get the SharpDX enum value list as an array
        /// </summary>
        static JoystickOffsetValues()
        {
            Offsets = (JoystickOffset[])Enum.GetValues(typeof(JoystickOffset));

            //build map of offset indexes and the offset's' string name to get them by
            IndexLookup = new Dictionary<string, int>();
            var i = 0;
            foreach (var n in Enum.GetNames(typeof(JoystickOffset)))
            {
                IndexLookup.Add(n,i++);
            }

            //build map of offset enumerated names and a cardinal index to get them by
            var index = 0;
            OffsetLookup = Offsets.ToDictionary(name => index++);
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
                throw new ArgumentOutOfRangeException($"JoystickOffset does not contain a value at index: {index}.");

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
        /// get offset string name from offset enum value
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetName(JoystickOffset offset)
        {
            return Enum.GetName(typeof(JoystickOffset), offset);
        }

        /// <summary>
        /// get clean offset string name from offset enum value
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetCleanName(JoystickOffset offset)
        {
            return Enum.GetName(typeof(JoystickOffsetCleanNames), offset);
        }
    }
}
