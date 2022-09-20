using System;

namespace SierraJSON
{
    public interface ISierraJsonConverter
    {
        bool CanConvertOnRead(Type objectType);
        bool CanConvertOnWrite(Type objectType);
        object ReadJson(Type objectType, object existingValue);
        void WriteJson(object objectToWrite);
    }
}
