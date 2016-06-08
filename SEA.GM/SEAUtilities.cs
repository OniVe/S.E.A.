using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace SEA.GM
{
    public static partial class StringBuilderExtensions
    {
        #region Const

        private const string jObjStart         = "{\"";
        private const string jObjEnd           = "\"}";
        private const string jKeyValueSplit    = "\":\"";
        private const string jPairSplit        = "\",\"";
        private const char   jObjRawStart      = '{';
        private const char   jObjRawEnd        = '}';
        private const char   jArrRawStart      = '[';
        private const char   jArrRawEnd        = ']';
        private const string jKeyRawValueSplit = "\":";
        private const string jPairRawSplit     = ",\"";
        private const char   jSplit            = ',';
        private const string jDoubleQuote      = "\"";
        #endregion

        #region JObject

        public static StringBuilder JObjectKeyValuePair( this StringBuilder self, params string[] param )
        {
            int l = param.Length;
            if (l == 0)
                return self;

            int i = 0;
            while (i < l)
                self
                    .Append(i == 0 ? jObjStart : jPairRawSplit)
                    .Append(param[i++])
                    .Append(jKeyRawValueSplit)
                    .Append(i < l ? param[i++] : string.Empty);

            return self.Append(jObjRawEnd);
        }
        public static StringBuilder JObjectStringKeyValuePair( this StringBuilder self, params string[] param )
        {
            int l = param.Length;
            if (l == 0)
                return self;

            int i = 0;
            while (i < l)
                self
                    .Append(i == 0 ? jObjStart : jPairSplit)
                    .Append(param[i++])
                    .Append(jKeyValueSplit)
                    .Append(i < l ? param[i++] : string.Empty);

            return self.Append(jObjEnd);
        }
        public static StringBuilder JKeyValuePair( this StringBuilder self, params string[] param )
        {
            int l = param.Length;
            if (l == 0)
                return self;

            int i = 0;
            while (i < l)
                self
                    .Append(i == 0 ? jDoubleQuote : jPairRawSplit)
                    .Append(param[i++])
                    .Append(jKeyRawValueSplit)
                    .Append(i < l ? param[i++] : string.Empty);

            return self;
        }
        public static StringBuilder JStringKeyValuePair( this StringBuilder self, params string[] param )
        {
            int l = param.Length;
            if (l == 0)
                return self;

            int i = 0;
            while (i < l)
                self
                    .Append(i == 0 ? jDoubleQuote : jPairSplit)
                    .Append(param[i++])
                    .Append(jKeyValueSplit)
                    .Append(i < l ? param[i++] : string.Empty);

            return self.Append(jDoubleQuote);
        }
        public static StringBuilder JObjectStart( this StringBuilder self )
        {
            return self.Append(jObjRawStart);
        }
        public static StringBuilder JObjectEnd( this StringBuilder self )
        {
            return self.Append(jObjRawEnd);
        }
        public static StringBuilder JArrayStart( this StringBuilder self )
        {
            return self.Append(jArrRawStart);
        }
        public static StringBuilder JArrayEnd( this StringBuilder self )
        {
            return self.Append(jArrRawEnd);
        }
        public static StringBuilder JSplit( this StringBuilder self )
        {
            return self.Append(jSplit);
        }
        public static StringBuilder JSplit( this StringBuilder self, bool first )
        {
            if (first)
                return self;
            else
                return self.Append(jSplit);
        }
        public static StringBuilder JSplit( this StringBuilder self, ref bool first )
        {
            if (first)
            {
                first = false;
                return self;
            }
            else
                return self.Append(jSplit);
        }
        #endregion

        public static int IndexOf( this StringBuilder self, char value )
        {
            if (self.Length > 0)
            {
                int i = 0, l = self.Length;
                while (i < l)
                    if (self[i++] == value)
                        return i - 1;
            }
            return -1;
        }
        public static StringBuilder TrimEmptyEnd( this StringBuilder self )
        {
            if (self.Length > 0)
            {
                int i = 0, l = self.Length;
                while (i < l)
                {
                    if (self[i] == '\0')
                        break;
                    ++i;
                }
                self.Length = i;
            }
            return self;
        }
    }

    public static class MyMath
    {
        public const float PIf = (float)Math.PI;
        public const float PIx2f = (float)(Math.PI * 2d);
        private const double RatioRadToDeg = 180d / Math.PI;
        private const float RatioDegToRad = (float)(Math.PI / 180d);


        public static float ShortestAngle( float startAngle, float endAngle )
        {
            var delta = endAngle - startAngle;
            delta -= (float)Math.Floor(delta / PIx2f) * PIx2f;
            if (delta > PIf)
                return delta - PIx2f;

            return delta;
        }
        public static float RadiansToDegrees( float value )
        {
            return (float)Math.Round((double)value * RatioRadToDeg, 2, MidpointRounding.AwayFromZero);
        }
        public static float DegreesToRadians( float value )
        {
            return value * RatioDegToRad;
        }
    }

    public struct MyId
    {
        private static Random seed = new Random();
        private int a;
        private int b;
        private int c;
        private int d;

        public MyId( int a, int b, int c, int d )
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        public static MyId NewId()
        {
            var r = new Random(seed.Next());
            return new MyId(r.Next(), r.Next(), r.Next(), r.Next());
        }
        public override string ToString()
        {
            return a.ToString("X8") + b.ToString("X8") + c.ToString("X8") + d.ToString("X8");
        }
        public void AppendTo( ref StringBuilder value )
        {
            value.Append(a.ToString("X8"));
            value.Append(b.ToString("X8"));
            value.Append(c.ToString("X8"));
            value.Append(d.ToString("X8"));
        }
    }

    public static class SEAUtilities
    {
        private static CultureInfo cultureInfoUS;
        public static CultureInfo CultureInfoUS
        {
            get
            {
                if (cultureInfoUS == null)
                    cultureInfoUS = new CultureInfo("en-US");
                return cultureInfoUS;
            }
        }
        private static AlphanumComparator<string> naturalNumericComparer;
        public static AlphanumComparator<string> NaturalNumericComparer
        {
            get
            {
                if (naturalNumericComparer == null)
                    naturalNumericComparer = new AlphanumComparator<string>();
                return naturalNumericComparer;
            }
        }
        public class Logging
        {
            private static Logging _static;
            private TextWriter _writer = null;

            public static Logging Static
            {
                get
                {
                    if (MyAPIGateway.Utilities == null) return null;
                    if (_static == null) _static = new Logging("SEA.log");
                    return _static;
                }
            }

            public Logging( string fileName )
            {
                try
                {
                    if (MyAPIGateway.Utilities != null)
                        _writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, typeof(SEAUtilities));
                    _static = this;
                }
                catch { }
            }

            public void WriteLine( string text )
            {
                if (_writer != null)
                    _writer.WriteLine(DateTime.Now.ToString("[HH:mm:ss] - ") + text);
            }
            public void WriteLineWithoutTime( string text )
            {
                if (_writer != null)
                    _writer.WriteLine(text);
            }

            internal void Close()
            {
                if (_writer != null)
                {
                    _writer.Flush();
                    _writer.Dispose();
                    _writer = null;
                }
            }
        }

        public static string GetExceptionString( Exception ex, StringBuilder errorMsg = null )
        {
            if (errorMsg == null)
                errorMsg = new StringBuilder(1024);

            errorMsg
                .Append("Exception occured: ")
                .Append(ex.TargetSite)
                .Append(": ")
                .AppendLine(ex.Message)
                .AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
                errorMsg.AppendLine("Inner exception: ")
                    .Append(ex.InnerException.TargetSite)
                    .Append(": ")
                    .AppendLine(ex.InnerException.Message)
                    .AppendLine(ex.InnerException.StackTrace);

            return errorMsg.ToString();
        }

        /// <summary>
        /// This class encodes and decodes JSON strings.
        /// Spec. details, see http://www.json.org/
        ///
        /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
        /// All numbers are parsed to float.
        /// </summary>
        public class JSON
        {
            public const int TOKEN_NONE = 0;
            public const int TOKEN_CURLY_OPEN = 1;
            public const int TOKEN_CURLY_CLOSE = 2;
            public const int TOKEN_SQUARED_OPEN = 3;
            public const int TOKEN_SQUARED_CLOSE = 4;
            public const int TOKEN_COLON = 5;
            public const int TOKEN_COMMA = 6;
            public const int TOKEN_STRING = 7;
            public const int TOKEN_NUMBER = 8;
            public const int TOKEN_TRUE = 9;
            public const int TOKEN_FALSE = 10;
            public const int TOKEN_NULL = 11;

            private const int BUILDER_CAPACITY = 512;/*2000*/

            /// <summary>
            /// Parses the string json into a value
            /// </summary>
            /// <param name="json">A JSON string.</param>
            /// <returns>An ArrayList, a Hashtable, a float, a string, null, true, or false</returns>
            public static object JsonDecode( string json )
            {
                bool success = true;

                return JsonDecode(json, ref success);
            }

            /// <summary>
            /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
            /// </summary>
            /// <param name="json">A JSON string.</param>
            /// <param name="success">Successful parse?</param>
            /// <returns>An ArrayList, a Hashtable, a float, a string, null, true, or false</returns>
            public static object JsonDecode( string json, ref bool success )
            {
                success = true;
                if (json != null)
                {
                    char[] charArray = json.ToCharArray();
                    int index = 0;
                    object value = ParseValue(charArray, ref index, ref success);
                    return value;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
            /// </summary>
            /// <param name="json">A JSON string.</param>
            /// <param name="success">Successful parse?</param>
            /// <returns>An ArrayList, a Hashtable, a float, a string, null, true, or false</returns>
            public static object JsonDecode( StringBuilder json, ref bool success )
            {
                success = true;
                if (json != null)
                {
                    char[] charArray = new char[json.Length];
                    json.CopyTo(0, charArray, 0, json.Length);
                    int index = 0;
                    object value = ParseValue(charArray, ref index, ref success);
                    return value;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Converts a Hashtable / ArrayList object into a JSON string
            /// </summary>
            /// <param name="json">A Hashtable / ArrayList</param>
            /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
            public static string JsonEncode( object json )
            {
                StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
                bool success = SerializeValue(json, builder);
                return (success ? builder.ToString() : null);
            }
            protected static Hashtable ParseObject( char[] json, ref int index, ref bool success )
            {
                Hashtable table = new Hashtable();
                int token;

                // {
                NextToken(json, ref index);

                bool done = false;
                while (!done)
                {
                    token = LookAhead(json, index);
                    if (token == JSON.TOKEN_NONE)
                    {
                        success = false;
                        return null;
                    }
                    else if (token == JSON.TOKEN_COMMA)
                    {
                        NextToken(json, ref index);
                    }
                    else if (token == JSON.TOKEN_CURLY_CLOSE)
                    {
                        NextToken(json, ref index);
                        return table;
                    }
                    else
                    {

                        // name
                        string name = ParseString(json, ref index, ref success);
                        if (!success)
                        {
                            success = false;
                            return null;
                        }

                        // :
                        token = NextToken(json, ref index);
                        if (token != JSON.TOKEN_COLON)
                        {
                            success = false;
                            return null;
                        }

                        // value
                        object value = ParseValue(json, ref index, ref success);
                        if (!success)
                        {
                            success = false;
                            return null;
                        }

                        table[name] = value;
                    }
                }

                return table;
            }
            protected static ArrayList ParseArray( char[] json, ref int index, ref bool success )
            {
                ArrayList array = new ArrayList();

                // [
                NextToken(json, ref index);

                bool done = false;
                while (!done)
                {
                    int token = LookAhead(json, index);
                    if (token == JSON.TOKEN_NONE)
                    {
                        success = false;
                        return null;
                    }
                    else if (token == JSON.TOKEN_COMMA)
                    {
                        NextToken(json, ref index);
                    }
                    else if (token == JSON.TOKEN_SQUARED_CLOSE)
                    {
                        NextToken(json, ref index);
                        break;
                    }
                    else
                    {
                        object value = ParseValue(json, ref index, ref success);
                        if (!success)
                        {
                            return null;
                        }

                        array.Add(value);
                    }
                }

                return array;
            }
            protected static object ParseValue( char[] json, ref int index, ref bool success )
            {
                switch (LookAhead(json, index))
                {
                    case JSON.TOKEN_STRING:
                        return ParseString(json, ref index, ref success);
                    case JSON.TOKEN_NUMBER:
                        return ParseNumber(json, ref index, ref success);
                    case JSON.TOKEN_CURLY_OPEN:
                        return ParseObject(json, ref index, ref success);
                    case JSON.TOKEN_SQUARED_OPEN:
                        return ParseArray(json, ref index, ref success);
                    case JSON.TOKEN_TRUE:
                        NextToken(json, ref index);
                        return true;
                    case JSON.TOKEN_FALSE:
                        NextToken(json, ref index);
                        return false;
                    case JSON.TOKEN_NULL:
                        NextToken(json, ref index);
                        return null;
                    case JSON.TOKEN_NONE:
                        break;
                }

                success = false;
                return null;
            }
            protected static string ParseString( char[] json, ref int index, ref bool success )
            {
                StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
                char c;

                EatWhitespace(json, ref index);

                // "
                c = json[index++];

                bool complete = false;
                while (!complete)
                {

                    if (index == json.Length)
                    {
                        break;
                    }

                    c = json[index++];
                    if (c == '"')
                    {
                        complete = true;
                        break;
                    }
                    else if (c == '\\')
                    {

                        if (index == json.Length)
                        {
                            break;
                        }
                        c = json[index++];
                        if (c == '"')
                        {
                            s.Append('"');
                        }
                        else if (c == '\\')
                        {
                            s.Append('\\');
                        }
                        else if (c == '/')
                        {
                            s.Append('/');
                        }
                        else if (c == 'b')
                        {
                            s.Append('\b');
                        }
                        else if (c == 'f')
                        {
                            s.Append('\f');
                        }
                        else if (c == 'n')
                        {
                            s.Append('\n');
                        }
                        else if (c == 'r')
                        {
                            s.Append('\r');
                        }
                        else if (c == 't')
                        {
                            s.Append('\t');
                        }
                        else if (c == 'u')
                        {
                            int remainingLength = json.Length - index;
                            if (remainingLength >= 4)
                            {
                                // parse the 32 bit hex into an integer codepoint
                                uint codePoint;
                                //if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))//OniVe >><<
                                if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, SEAUtilities.CultureInfoUS, out codePoint)))
                                {
                                    return "";
                                }
                                // convert the integer codepoint to a unicode char and add to string
                                s.Append(Char.ConvertFromUtf32((int)codePoint));
                                // skip 4 chars
                                index += 4;
                            }
                            else
                            {
                                break;
                            }
                        }

                    }
                    else
                    {
                        s.Append(c);
                    }

                }

                if (!complete)
                {
                    success = false;
                    return null;
                }

                return s.ToString();
            }
            protected static float ParseNumber( char[] json, ref int index, ref bool success )
            {
                EatWhitespace(json, ref index);

                int lastIndex = GetLastIndexOfNumber(json, index);
                int charLength = (lastIndex - index) + 1;

                float number;
                //success = double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);//OniVe >><<
                success = float.TryParse(new string(json, index, charLength), NumberStyles.Any, SEAUtilities.CultureInfoUS, out number);

                index = lastIndex + 1;
                return number;
            }
            protected static int GetLastIndexOfNumber( char[] json, int index )
            {
                int lastIndex;

                for (lastIndex = index; lastIndex < json.Length; lastIndex++)
                {
                    if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                    {
                        break;
                    }
                }
                return lastIndex - 1;
            }
            protected static void EatWhitespace( char[] json, ref int index )
            {
                for (; index < json.Length; index++)
                {
                    if (" \t\n\r".IndexOf(json[index]) == -1)
                    {
                        break;
                    }
                }
            }
            protected static int LookAhead( char[] json, int index )
            {
                int saveIndex = index;
                return NextToken(json, ref saveIndex);
            }
            protected static int NextToken( char[] json, ref int index )
            {
                EatWhitespace(json, ref index);

                if (index == json.Length)
                {
                    return JSON.TOKEN_NONE;
                }

                char c = json[index];
                index++;
                switch (c)
                {
                    case '{':
                        return JSON.TOKEN_CURLY_OPEN;
                    case '}':
                        return JSON.TOKEN_CURLY_CLOSE;
                    case '[':
                        return JSON.TOKEN_SQUARED_OPEN;
                    case ']':
                        return JSON.TOKEN_SQUARED_CLOSE;
                    case ',':
                        return JSON.TOKEN_COMMA;
                    case '"':
                        return JSON.TOKEN_STRING;
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
                        return JSON.TOKEN_NUMBER;
                    case ':':
                        return JSON.TOKEN_COLON;
                }
                index--;

                int remainingLength = json.Length - index;

                // false
                if (remainingLength >= 5)
                {
                    if (json[index] == 'f' &&
                        json[index + 1] == 'a' &&
                        json[index + 2] == 'l' &&
                        json[index + 3] == 's' &&
                        json[index + 4] == 'e')
                    {
                        index += 5;
                        return JSON.TOKEN_FALSE;
                    }
                }

                // true
                if (remainingLength >= 4)
                {
                    if (json[index] == 't' &&
                        json[index + 1] == 'r' &&
                        json[index + 2] == 'u' &&
                        json[index + 3] == 'e')
                    {
                        index += 4;
                        return JSON.TOKEN_TRUE;
                    }
                }

                // null
                if (remainingLength >= 4)
                {
                    if (json[index] == 'n' &&
                        json[index + 1] == 'u' &&
                        json[index + 2] == 'l' &&
                        json[index + 3] == 'l')
                    {
                        index += 4;
                        return JSON.TOKEN_NULL;
                    }
                }

                return JSON.TOKEN_NONE;
            }
            protected static bool SerializeValue( object value, StringBuilder builder )
            {
                bool success = true;

                if (value is string)
                {
                    success = SerializeString((string)value, builder);
                }
                else if (value is Hashtable)
                {
                    success = SerializeObject((Hashtable)value, builder);
                }
                else if (value is ArrayList)
                {
                    success = SerializeArray((ArrayList)value, builder);
                }
                else if ((value is Boolean) && ((Boolean)value == true))
                {
                    builder.Append("true");
                }
                else if ((value is Boolean) && ((Boolean)value == false))
                {
                    builder.Append("false");
                }
                else if (value is ValueType)
                {
                    // thanks to ritchie for pointing out ValueType to me
                    success = SerializeNumber(Convert.ToSingle(value), builder);
                }
                else if (value == null)
                {
                    builder.Append("null");
                }
                else
                {
                    success = false;
                }
                return success;
            }
            protected static bool SerializeObject( Hashtable anObject, StringBuilder builder )
            {
                builder.Append("{");

                IDictionaryEnumerator e = anObject.GetEnumerator();
                bool first = true;
                while (e.MoveNext())
                {
                    string key = e.Key.ToString();
                    object value = e.Value;

                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    SerializeString(key, builder);
                    builder.Append(":");
                    if (!SerializeValue(value, builder))
                    {
                        return false;
                    }

                    first = false;
                }

                builder.Append("}");
                return true;
            }
            protected static bool SerializeArray( ArrayList anArray, StringBuilder builder )
            {
                builder.Append("[");

                bool first = true;
                for (int i = 0; i < anArray.Count; i++)
                {
                    object value = anArray[i];

                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    if (!SerializeValue(value, builder))
                    {
                        return false;
                    }

                    first = false;
                }

                builder.Append("]");
                return true;
            }
            protected static bool SerializeString( string aString, StringBuilder builder )
            {
                builder.Append("\"");

                char[] charArray = aString.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    char c = charArray[i];
                    if (c == '"')
                    {
                        builder.Append("\\\"");
                    }
                    else if (c == '\\')
                    {
                        builder.Append("\\\\");
                    }
                    else if (c == '\b')
                    {
                        builder.Append("\\b");
                    }
                    else if (c == '\f')
                    {
                        builder.Append("\\f");
                    }
                    else if (c == '\n')
                    {
                        builder.Append("\\n");
                    }
                    else if (c == '\r')
                    {
                        builder.Append("\\r");
                    }
                    else if (c == '\t')
                    {
                        builder.Append("\\t");
                    }
                    else
                    {
                        /* OniVe >> All translated as Unicode*/
                        builder.Append(c);

                        /*! int codepoint = Convert.ToInt32(c);
                        if ((codepoint >= 32) && (codepoint <= 126))
                        {
                            builder.Append(c);
                        }
                        else
                        {
                            builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                        } OniVe << */
                    }
                }

                builder.Append("\"");
                return true;
            }
            protected static bool SerializeNumber( float number, StringBuilder builder )
            {
                //builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));//OniVe >><<
                builder.Append(Convert.ToString(number, SEAUtilities.CultureInfoUS));
                return true;
            }
        }

        public class AlphanumComparator<T> : System.Collections.Generic.IComparer<T>
        {
            private enum ChunkType { Alphanumeric, Numeric };
            private bool InChunk( char ch, char otherCh )
            {
                ChunkType type = ChunkType.Alphanumeric;

                if (char.IsDigit(otherCh))
                {
                    type = ChunkType.Numeric;
                }

                if ((type == ChunkType.Alphanumeric && char.IsDigit(ch))
                    || (type == ChunkType.Numeric && !char.IsDigit(ch)))
                {
                    return false;
                }

                return true;
            }
            public int Compare( T x, T y )
            {
                String s1 = x as string;
                String s2 = y as string;
                if (s1 == null || s2 == null)
                {
                    return 0;
                }

                int thisMarker = 0, thisNumericChunk = 0;
                int thatMarker = 0, thatNumericChunk = 0;

                while ((thisMarker < s1.Length) || (thatMarker < s2.Length))
                {
                    if (thisMarker >= s1.Length)
                    {
                        return -1;
                    }
                    else if (thatMarker >= s2.Length)
                    {
                        return 1;
                    }
                    char thisCh = s1[thisMarker];
                    char thatCh = s2[thatMarker];

                    StringBuilder thisChunk = new StringBuilder();
                    StringBuilder thatChunk = new StringBuilder();

                    while ((thisMarker < s1.Length) && (thisChunk.Length == 0 || InChunk(thisCh, thisChunk[0])))
                    {
                        thisChunk.Append(thisCh);
                        thisMarker++;

                        if (thisMarker < s1.Length)
                        {
                            thisCh = s1[thisMarker];
                        }
                    }

                    while ((thatMarker < s2.Length) && (thatChunk.Length == 0 || InChunk(thatCh, thatChunk[0])))
                    {
                        thatChunk.Append(thatCh);
                        thatMarker++;

                        if (thatMarker < s2.Length)
                        {
                            thatCh = s2[thatMarker];
                        }
                    }

                    int result = 0;
                    // If both chunks contain numeric characters, sort them numerically
                    if (char.IsDigit(thisChunk[0]) && char.IsDigit(thatChunk[0]))
                    {
                        thisNumericChunk = Convert.ToInt32(thisChunk.ToString());
                        thatNumericChunk = Convert.ToInt32(thatChunk.ToString());

                        if (thisNumericChunk < thatNumericChunk)
                        {
                            result = -1;
                        }

                        if (thisNumericChunk > thatNumericChunk)
                        {
                            result = 1;
                        }
                    }
                    else
                    {
                        result = thisChunk.ToString().CompareTo(thatChunk.ToString());
                    }

                    if (result != 0)
                    {
                        return result;
                    }
                }

                return 0;
            }
        }
    }
}
