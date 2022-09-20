using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SierraJSON
{
    static class Read
    {
        private static ISierraJsonConverter _converter;

        enum Tokens
        {
            OBJECT_START,
            OBJECT_END,
            ARRAY_START,
            ARRAY_END,
            COLON,
            STRING,
            NUMBER,
            COMMA,
            BOOLEAN,
            NULL
        };

        class JsonToken
        {
            public Tokens Token { get; set; }
            public string Value { get; set; }
            public override string ToString()
            {
                return $"Token:{Token}, Value:{Value}";
            }
        }

        private static char[] jsonArray;
        private static int index;
        private static int length;
        private static Queue<JsonToken> _tokens;

        public static void SetConverter(ISierraJsonConverter converter)
        {
            _converter = converter;
        }

        public static string GetToken(string name, string json)
        {
            index = 0;
            jsonArray = json.ToCharArray();
            length = jsonArray.Length;

            try
            {

                Tokenize();
                index = 0;
                var list = _tokens.ToArray();

                for (var n = 0; n < list.Length; n++)
                {
                    if (list[n].Value == name)
                    {
                        return list[n + 2].Value;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string GetToken(string name)
        {
            if (_tokens.Count == 0) return "";
            var list = _tokens.ToArray();

            for (var n = 0; n < list.Length; n++)
            {
                if (list[n].Value == name)
                {
                    return list[n + 2].Value;
                }
            }
            return "";
        }

        public static T ToObject<T>(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            index = 0;
            jsonArray = json.ToCharArray();
            length = jsonArray.Length;

            object obj = null;
            if (length == 0) return (T)obj;

            try
            {
                Tokenize();
                if (_tokens.Count == 0) return (T) obj;

                index = 0;

                _tokens.Dequeue();//skip past start
                obj = ParseObject(typeof(T));
            }
            catch (Exception e)
            {
                throw new Serializer.SierraJSONException();
            }
            return (T)obj;
        }

        private static void Tokenize()
        {
            _tokens = new Queue<JsonToken>();

            for (; index < length; index++)
            {
                var c = jsonArray[index];

                switch (c)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;

                    case '\'':
                    case '"':
                        AddToken(Tokens.STRING, TokenizeString());
                        break;

                    case 'f':
                        TokenizeBoolFalse();
                        break;

                    case 't':
                        TokenizeBoolTrue();
                        break;

                    case 'n':
                        TokenizeNull();
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        AddToken(Tokens.NUMBER, TokenizeNumber().ToString());
                        break;

                    case '{':
                        AddToken(Tokens.OBJECT_START, "");
                        break;

                    case '}':
                        AddToken(Tokens.OBJECT_END, "");
                        break;

                    case '[':
                        AddToken(Tokens.ARRAY_START, "");
                        break;

                    case ']':
                        AddToken(Tokens.ARRAY_END, "");
                        break;

                    case ':':
                        AddToken(Tokens.COLON, "");
                        break;

                    case ',':
                        AddToken(Tokens.COMMA, "");
                        break;
                }
            }
        }

        private static void AddToken(Tokens token, string value)
        {
            _tokens.Enqueue(new JsonToken() { Token = token, Value = value });

        }

        private static string TokenizeString()
        {
            StringBuilder s = new StringBuilder();

            index++;//move off of the quote that got us here

            var c = jsonArray[index];
            while (c != '"')
            {
                s.Append(c);
                c = jsonArray[++index];
            }

            return s.ToString();
        }

        private static Int32 TokenizeNumber()
        {
            StringBuilder s = new StringBuilder();

            var c = jsonArray[index];
            while (c == '-' || c == '+' || c == '.' || c == 'e' || (c >= '0' && c <= '9'))
            {
                s.Append(c);
                c = jsonArray[++index];
            }

            index--; //roll back one index otherwise the for loop will advance over the comma which is being pointed at currently
            return Convert.ToInt32(s.ToString());
        }

        private static void TokenizeBoolTrue()
        {
            if (jsonArray[index + 1] == 'r' &&
                jsonArray[index + 2] == 'u' &&
                jsonArray[index + 3] == 'e')
            {
                AddToken(Tokens.BOOLEAN, "true");
            }
        }

        private static void TokenizeBoolFalse()
        {
            if (jsonArray[index + 1] == 'a' &&
                jsonArray[index + 2] == 'l' &&
                jsonArray[index + 3] == 's' &&
                jsonArray[index + 4] == 'e')
            {
                AddToken(Tokens.BOOLEAN, "false");
            }
        }

        private static void TokenizeNull()
        {
            if (jsonArray[index + 1] == 'u' &&
                jsonArray[index + 2] == 'l' &&
                jsonArray[index + 3] == 'l')
            {
                AddToken(Tokens.NULL, "null");
            }
        }


        static object ParseValue(JsonToken token)
        {
            switch (token.Token)
            {
                case Tokens.OBJECT_START:
                    break;

                case Tokens.OBJECT_END:
                    break;

                case Tokens.STRING:
                    return token.Value;

                case Tokens.ARRAY_START:
                    break;

                case Tokens.ARRAY_END:
                    break;

                case Tokens.COLON:
                    break;

                case Tokens.NUMBER:
                    return Convert.ToInt32(token.Value);

                case Tokens.COMMA:
                    break;

                case Tokens.BOOLEAN:
                    return Convert.ToBoolean(token.Value);

                case Tokens.NULL:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private static object ParseObject(Type type)
        {
            //test known types first
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return ParseDictionary(type);
            }

            //treat as class
            var obj = Activator.CreateInstance(type);
            return ParseObject(obj);
        }

        private static object ParseObject(object obj)
        {
            var type = obj.GetType();

            while (_tokens.Count > 0)
            {
                var tok = _tokens.Dequeue(); //should be a key for a property on the object


                if (tok.Token == Tokens.STRING)
                {
                    //this string/key is a property name
                    var propertyName = tok.Value;
                    var propInfo = type.GetTypeInfo().GetDeclaredProperty(propertyName);

                    if (propInfo == null)
                        throw new ArgumentNullException(propertyName, "Property not found on object.");

                    tok = _tokens.Dequeue();//skip the colon
                    if (tok.Token != Tokens.COLON)
                    {
                        Debug.WriteLine($"Expected a colon and a value, but there was non: {propInfo.Name}, from type: {propInfo.PropertyType.Name}");
                        throw new ArgumentOutOfRangeException(propertyName, "Property does not have a value?");
                    }


                    object value = null;

                    //tests for property being a list
                    if (typeof(IDictionary).IsAssignableFrom(propInfo.PropertyType))
                    {
                        _tokens.Dequeue();//remove start brace
                        value = ParseDictionary(propInfo.PropertyType);
                        propInfo.SetValue(obj, value);
                        value = null;
                    }

                    if (propInfo.PropertyType.IsAssignableFrom(typeof(ICollection)))
                    {
                        value = ParseCollection(propInfo.PropertyType, null);
                        propInfo.SetValue(obj, value);
                        value = null;
                    }

                    //next token will either by a value (string, number, bool, etc), or a list
                    tok = _tokens.Dequeue();

                    if (tok.Token == Tokens.OBJECT_END)
                    {
                        return obj;
                    }

                    if (tok.Token == Tokens.ARRAY_START)
                    {
                        if (obj != null)
                        {
                            var collection = obj.GetType().GetProperty(propInfo.Name).GetValue(obj);

                            if (collection != null)
                            {
                                ParseCollection(propInfo.PropertyType, collection);
                            }
                            else
                            {
                                value = ParseCollection(propInfo.PropertyType, null);
                                propInfo.SetValue(obj, value);

                            }
                        }
                    }

                    if (tok.Token == Tokens.OBJECT_START)
                    {
                        value = ParseObject(propInfo.PropertyType);
                        propInfo.SetValue(obj, value);//works on test7 when assigning ActionCatalog (value) to HOTASCollection (obj)
                        value = null;
                    }

                    if (tok.Token == Tokens.STRING || tok.Token == Tokens.BOOLEAN || tok.Token == Tokens.NUMBER)
                    {
                        value = TryCustomConverter(propInfo.PropertyType, tok.Value);
                        if (value == null)
                        {
                            value = ParseValue(tok);
                        }
                    }

                    if (value != null)
                    {
                        try
                        {
                            if (typeof(Guid).IsAssignableFrom(propInfo.PropertyType))
                            {
                                value = TypeDescriptor.GetConverter(propInfo.PropertyType).ConvertFrom(value);
                            }
                            propInfo.SetValue(obj, value);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"couldn't deserialize: {propInfo.Name}, from type: {propInfo.PropertyType.Name}, Exception:{e}");
                            throw;
                        }
                    }


                    tok = _tokens.Dequeue();//either a comma or end brace

                    if (tok.Token == Tokens.OBJECT_END)
                    {
                        return obj;
                    }

                    if (tok.Token != Tokens.COMMA)
                    {
                        Debug.WriteLine($"Expected a comma or the end of an object, but found this instead:{tok.Token}. This was after {propInfo.Name}, from type: {propInfo.PropertyType.Name}");
                        throw new ArgumentOutOfRangeException(propertyName, "Unexpected token");
                    }
                }
            }
            return obj;//does this really hit?
        }

        private static object ParseCollection(Type type, object collection)
        {
            var value = TryCustomConverter(type, null);
            if (value != null)
            {
                return value;
            }

            if (collection == null)
            {
                collection = Activator.CreateInstance(type);
            }

            while (true)
            {
                var tok = _tokens.Dequeue();

                if (tok.Token == Tokens.COMMA)
                {
                    //more items
                    tok = _tokens.Dequeue();
                }
                if (tok.Token == Tokens.ARRAY_END)//empty dictionary
                {
                    return collection;
                }

                switch (tok.Token)
                {
                    case Tokens.OBJECT_START:
                        var childType = type.GenericTypeArguments[0];
                        value = TryCustomConverter(childType, null);
                        if (value == null)
                        {
                            value = ParseObject(childType);
                        }
                        break;
                    case Tokens.OBJECT_END:
                        break;
                    case Tokens.ARRAY_START:
                        break;
                    case Tokens.ARRAY_END:
                        break;
                    case Tokens.COLON:
                        break;
                    case Tokens.STRING:
                        break;
                    case Tokens.NUMBER:
                        break;
                    case Tokens.COMMA:
                        break;
                    case Tokens.BOOLEAN:
                        break;
                    case Tokens.NULL:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                object[] parms = { value };
                collection?.GetType().GetMethod("Add").Invoke(collection, parms);
            }
        }

        private static object ParseDictionary(Type type)
        {
            var value = TryCustomConverter(type, null);
            if (value != null)
            {
                return value;
            }

            var index = 0;

            var dictionary = Activator.CreateInstance(type);

            while (true)
            {
                var tok = _tokens.Dequeue();//either an index or the end of the array

                if (tok.Token == Tokens.COMMA)
                {
                    //more items
                    tok = _tokens.Dequeue();
                }
                if (tok.Token == Tokens.OBJECT_END)//empty dictionary
                {
                    return dictionary;
                }

                if (tok.Token == Tokens.STRING)//convert key to integer to use as index
                {
                    index = Convert.ToInt32(tok.Value);
                }

                tok = _tokens.Dequeue();//skip the colon
                if (tok.Token != Tokens.COLON)
                {
                    Debug.WriteLine($"Expected a colon and a value, but there was none: {type.Name}");
                    throw new ArgumentOutOfRangeException(type.Name, "Property does not have a value?");
                }

                tok = _tokens.Dequeue();

                switch (tok.Token)
                {
                    case Tokens.OBJECT_START:
                        value = ParseObject(type.GenericTypeArguments[1]);
                        break;
                    case Tokens.OBJECT_END:
                        throw new ArgumentOutOfRangeException("shouldn't be handling this here");
                    case Tokens.ARRAY_START:
                        //we have a dictionary of collections
                        //get type of value in dictionary
                        var valueType = type.GenericTypeArguments[1];
                        value = ParseCollection(valueType, null);
                        break;
                    case Tokens.ARRAY_END:
                        throw new ArgumentOutOfRangeException("shouldn't be handling this here");
                    case Tokens.COLON:
                        throw new ArgumentOutOfRangeException("shouldn't be handling this here");
                    case Tokens.STRING:
                        throw new NotImplementedException("need to handle an array of strings");
                    case Tokens.NUMBER:
                        throw new NotImplementedException("need to handle an array of numbers");
                    case Tokens.COMMA:
                        throw new ArgumentOutOfRangeException("shouldn't be handling this here");
                    case Tokens.BOOLEAN:
                        throw new NotImplementedException("need to handle an array of numbers");
                    case Tokens.NULL:
                        throw new ArgumentOutOfRangeException("can't handle null");
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ((IDictionary)dictionary).Add(index, value);
            }
        }

        public static void Populate(object target)
        {
            ParseObject(target);
        }

        private static object TryCustomConverter(Type type, object existingObject)
        {
            if (_converter != null)
            {
                if (_converter.CanConvertOnRead(type))
                {
                    var result = _converter.ReadJson(type, existingObject);
                    return result;
                }
            }
            return null;
        }
    }
}
