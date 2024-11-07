using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SierraJSON
{
    static class Write
    {
        private static ISierraJsonConverter _converter;
        private static bool _converterRequiresKeyValueSeparator;
        private static bool _converterRequiresObjectSeparator;
        private static bool pendingCollectionStartFormat;

        private const string OBJECT_START = "{";
        private const string ARRAY_START = "[";

        private const string OBJECT_END = "}";
        private const string ARRAY_END = "]";

        private static int _indentLevel = 0;

        private static StringBuilder _resultJSON { get; set; }

        public static void SetConverter(ISierraJsonConverter converter)
        {
            _converter = converter;
        }

        public static void Reset(StringBuilder builder)
        {
            _resultJSON = builder;
            _indentLevel = 0;
            _converterRequiresKeyValueSeparator = false;
            _converterRequiresObjectSeparator = false;
            pendingCollectionStartFormat = false;
        }

        public static void WriteObjectStart()
        {
            if (_converterRequiresObjectSeparator)
            {
                _resultJSON.Append(',');
                _resultJSON.AppendLine();
                WriteIndent();
            }

            if (pendingCollectionStartFormat)
            {
                _resultJSON.AppendLine();
                PushIndent();
                WriteIndent();
                pendingCollectionStartFormat = false;
            }


            _resultJSON.Append(OBJECT_START);
            _resultJSON.AppendLine();
            PushIndent();
            _converterRequiresKeyValueSeparator = false;
        }

        public static void WriteKeyValue(string key, object value)
        {
            if (HideValue(value)) return;


            if (_converterRequiresKeyValueSeparator)
            {
                _resultJSON.Append(',');
                _resultJSON.AppendLine();
            }

            WriteIndent();
            WriteString(key);
            _resultJSON.Append(": ");
            WriteValue(value);

            _converterRequiresKeyValueSeparator = true;
        }

        public static void WriteObjectEnd()
        {
            _resultJSON.AppendLine();
            PopIndent();
            WriteIndent();
            _resultJSON.Append(OBJECT_END);

            _converterRequiresKeyValueSeparator = false;
            _converterRequiresObjectSeparator = true;
        }

        public static void WriteValue(object obj)
        {
            switch (obj)
            {
                case string s:
                    WriteString(s);
                    break;
                case int i:
                    WriteInt(i);
                    break;
                case double d:
                    WriteDouble(d);
                    break;
                case float f:
                    WriteFloat(f);
                    break;
                case bool b:
                    WriteBool(b);
                    break;
                case Guid guid:
                    WriteString(guid.ToString());
                    break;
                case IDictionary d:
                    WriteDictionary(d);
                    break;
                case ICollection c:
                    WriteCollection(c);
                    break;
                case Enum e:
                    WriteEnum(e);
                    break;
                default:
                    WriteObject(obj);
                    break;
            }
        }

        private static bool HideValue(object value)
        {
            if (value == null) return true;

            switch (value)
            {
                case int i:
                    return i == 0;
                case double d:
                    return Math.Abs(d - 1.0d) < 0.01;
                case float f:
                    return Math.Abs(f - 1.0d) < 0.01;
                case bool b:
                    return b == false;
                case string s:
                    return string.IsNullOrWhiteSpace(s);
                case Enum e:
                    return Convert.ToInt32(e) == 0;
                case ICollection c:
                    return c.Count == 0;
            }
            return false;
        }

        private static void WriteEnum(Enum e)
        {
            _resultJSON.Append(Convert.ToInt32(e));
        }

        private static void WriteString(string s)
        {
            _resultJSON.Append('"');
            _resultJSON.Append(s);
            _resultJSON.Append('"');
        }

        private static void WriteInt(int i)
        {
            _resultJSON.Append(i);
        }

        private static void WriteDouble(double d)
        {
            _resultJSON.Append(d);
        }

        private static void WriteFloat(float f)
        {
            _resultJSON.Append(f);
        }

        private static void WriteBool(bool b)
        {
            _resultJSON.Append(b.ToString().ToLower());
        }


        private static void WriteCollection(ICollection collection)
        {
            if (collection.Count == 0)
            {
                _resultJSON.Append(ARRAY_START);
                _resultJSON.Append(ARRAY_END);
                return;
            }
            _resultJSON.Append(ARRAY_START);
            pendingCollectionStartFormat = true;

            _converterRequiresObjectSeparator = false;
            foreach (var item in collection)
            {
                WriteObject(item);
            }

            //done with the items iteration. close the collection
            if (!pendingCollectionStartFormat)
            {
                PopIndent();
                _resultJSON.AppendLine();
                WriteIndent();
            }
            _resultJSON.Append(ARRAY_END);

            pendingCollectionStartFormat = false;
            //is this really an object comma or a collection comma
            _converterRequiresObjectSeparator = false;
        }

        private static void WriteDictionary(IDictionary dictionary)
        {
            _converterRequiresObjectSeparator = false;
            
            WriteObjectStart(); //write dictionary start instead if we need to maintain different comma states
            
            WriteIndent();

            //iterate key/value list
            var keysCount = dictionary.Keys.Count;
            var iteration = 1;
            foreach (var key in dictionary.Keys)
            {
                WriteDictionaryKey(key);

                var value = dictionary[key];

                var current = _resultJSON.Length;

                _converterRequiresObjectSeparator = false;
                WriteValue(value);

                var delta = _resultJSON.Length - current;
                if (delta == 0) continue; //if nothing was written, then a comma is not needed

                if (iteration < keysCount && delta > 0)
                {
                    _resultJSON.Append(',');
                    _resultJSON.AppendLine();
                    WriteIndent();
                }
                iteration++;
            }

            WriteObjectEnd();
        }

        private static void WriteDictionaryKey(object key)
        {
            _resultJSON.Append('"');
            _resultJSON.Append(key);
            _resultJSON.Append('"');
            _resultJSON.Append(": ");
        }

        private static void WriteObject(object obj)
        {
            var type = obj.GetType();

            if (TryCustomConverter(type, obj))
            {
                return;
            }

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

            WriteObjectStart();

            var requireSeparator = false;
            foreach (var p in propList)
            {
                var value = p.GetValue(obj);
                if (HideValue(value)) continue;

                if (requireSeparator)
                {
                    _resultJSON.Append(',');
                    _resultJSON.AppendLine();
                }

                //if collection is empty, then don't write and don't wait pending comma
                WriteIndent();
                WriteString(p.Name);
                _resultJSON.Append(": ");
                WriteValue(value);

                requireSeparator = true;
            }
            WriteObjectEnd();
            _converterRequiresObjectSeparator = true;
        }

        private static void PushIndent()
        {
            _indentLevel += 2;
        }

        private static void PopIndent()
        {
            _indentLevel -= 2;
            if (_indentLevel < 0)
            {
                _indentLevel = 0;
            }
        }

        private static void WriteIndent()
        {
            _resultJSON.Append(' ', _indentLevel);
        }

        private static void LogStringBuilder()
        {
            //Debug.WriteLine(_resultJSON.ToString());
        }

        private static bool TryCustomConverter(Type type, object existingObject)
        {
            if (_converter != null)
            {
                if (_converter.CanConvertOnWrite(type))
                {
                    _converter.WriteJson(existingObject);
                    return true;
                }
            }
            return false;
        }
    }
}
