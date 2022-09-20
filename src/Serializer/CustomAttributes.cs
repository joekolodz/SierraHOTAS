using System;

namespace SierraJSON
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SierraJsonObject : Attribute
    {
        public enum MemberSerialization
        {
            OptIn
        }

        public MemberSerialization Option { get; set; }

        public SierraJsonObject(MemberSerialization option)
        {
            Option = option;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SierraJsonProperty : Attribute
    {
        public SierraJsonProperty()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SierraJsonIgnore : Attribute
    {
        public SierraJsonIgnore()
        {
        }
    }
}
