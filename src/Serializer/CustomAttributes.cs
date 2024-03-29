using System;
using System.Linq;
using System.Reflection;

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
    
    public class SierraJsonAttributes
    {
        public static PropertyInfo[] GetSerializableProperties(Object obj)
        {
            var type = obj.GetType();
            var sierraObjectAttribute = (SierraJsonObject)type.GetCustomAttribute(typeof(SierraJsonObject));
            var isOptIn = sierraObjectAttribute?.Option == SierraJsonObject.MemberSerialization.OptIn;

            PropertyInfo[] propList;
            if (isOptIn)
            {
                propList = type.GetProperties().Where(p => p.IsDefined(typeof(SierraJsonProperty))).ToArray();
            }
            else
            {
                propList = type.GetProperties().Where(p => !p.IsDefined(typeof(SierraJsonIgnore))).ToArray();
            }
            return propList;
        }
    }

}
