/*
 * Copyright © 2020 robby & EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;

namespace BaseUtils.JSON
{
    public static class JTokenExtensionsGet
    {
        // done as extension classes as it allows null to be in tk

        public static JToken I(this JToken tk, object id)           // safe [] allowing previous to be null
        {
            return tk != null ? tk[id] : null;
        }

        public static bool IsNull(this JToken tk)
        {
            return tk == null || tk.IsNull;
        }

        public static string MultiStr(this JObject tk, string[] ids, string def = "")       // multiple lookup in Object of names
        {
            JToken t = tk?.Contains(ids);
            return t != null && t.IsString ? (string)t.Value : def;
        }

        public static string Str(this JToken tk, string def = "")       // if not string, or null, return def.
        {
            return tk != null ? ((string)tk ?? def) : def;
        }

        public static string StrNull(this JToken tk)
        {
            return tk != null ? (string)tk : null;
        }

        public static T Enum<T>(this JToken tk, T def)      // tk is long
        {
            if (tk != null && tk.IsLong)
            {
                var i = (int)(long)tk.Value;
                return (T)System.Enum.ToObject(typeof(T), i);
            }
            else
                return def;
        }


        public static T EnumStr<T>(this JToken tk, T def, bool ignorecase = true) where T:struct    // tk is string
        {
            if (tk != null && tk.IsString)
            {
                string s = (string)tk.Value;
                if (System.Enum.TryParse(s, ignorecase,out T result) )
                {
                    return result;
                }
            }

            return def;
        }


        public static int Int(this JToken tk, int def = 0)
        {
            if (tk != null)
                return (int?)tk ?? def;
            else
                return def;
        }

        public static int? IntNull(this JToken tk)
        {
            return tk != null ? (int?)tk : null;
        }

        public static uint UInt(this JToken tk, uint def = 0)
        {
            if (tk != null)
                return (uint?)tk ?? def;
            else
                return def;
        }

        public static uint? UIntNull(this JToken tk)
        {
            return tk != null ? (uint?)tk : null;
        }

        public static long Long(this JToken tk, long def = 0)
        {
            if (tk != null)
                return (long?)tk ?? def;
            else
                return def;
        }

        public static long? LongNull(this JToken tk)
        {
            return tk != null ? (long?)tk : null;
        }

        public static ulong ULong(this JToken tk, ulong def = 0)
        {
            if (tk != null)
                return (ulong?)tk ?? def;
            else
                return def;
        }

        public static ulong? ULongNull(this JToken tk)
        {
            return tk != null ? (ulong?)tk : null;
        }

        public static double Double(this JToken tk, double def = 0)
        {
            if (tk != null)
                return (double?)tk ?? def;
            else
                return def;
        }

        public static double? DoubleNull(this JToken tk)
        {
            return tk != null ? (double?)tk : null;
        }

        public static float Float(this JToken tk, float def = 0)
        {
            if (tk != null)
                return (float?)tk ?? def;
            else
                return def;
        }

        public static float? FloatNull(this JToken tk)
        {
            return tk != null ? (float?)tk : null;
        }

#if JSONBIGINT
        public static System.Numerics.BigInteger BigInteger(this JToken tk, System.Numerics.BigInteger def)
        {
            if (tk == null)
                return def;
            else if (tk.TokenType == JToken.TType.ULong)
                return (ulong)tk.Value;
            else if (tk.IsLong && (long)tk.Value >= 0)
                return (ulong)(long)tk.Value;
            else if (tk.TokenType == JToken.TType.BigInt)
                return (System.Numerics.BigInteger)tk.Value;
            else
                return def;
        }
#endif

        public static bool Bool(this JToken tk, bool def = false)
        {
            if ( tk != null )
                return (bool?)tk ?? def;
            else
                return def;
        }

        public static bool? BoolNull(this JToken tk)
        {
            return tk != null ? (bool?)tk : null;
        }

        public static DateTime? DateTime(this JToken tk, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (tk != null && tk.IsString && System.DateTime.TryParse((string)tk.Value, ci, ds, out DateTime ret))
                return ret;
            else
                return null;
        }

        public static DateTime DateTime(this JToken tk, DateTime def, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (tk != null && tk.IsString && System.DateTime.TryParse((string)tk.Value, ci, ds, out DateTime ret))
                return ret;
            else
                return def;
        }

        public static DateTime DateTimeUTC(this JToken tk)
        {
            if (tk != null && tk.IsString && System.DateTime.TryParse((string)tk.Value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime ret))
                return ret;
            else
                return System.DateTime.MinValue;
        }

        public static T Enumeration<T>(this JToken tk, T def, Func<string, string> preprocess = null)
        {
            if (tk != null && tk.IsString)
            {
                try
                {
                    string v = (string)tk.Value;
                    if (preprocess != null)
                        v = preprocess(v);
                    return (T)System.Enum.Parse(typeof(T), v, true);
                }
                catch
                {
                }
            }

            return def;
        }

        public static JArray Array(this JToken tk)       // null if not
        {
            return tk as JArray;
        }

        public static JObject Object(this JToken tk)     // null if not
        {
            return tk as JObject;
        }

        public static JObject RenameObjectFields(this JToken jo, string pat, string replace, bool startswith = false)
        {
            JObject o = jo.Object();

            if (o != null)
            {
                JObject ret = new JObject();

                foreach (var kvp in o)
                {
                    string name = kvp.Key;
                    if (startswith)
                    {
                        if (name.StartsWith(pat))
                            name = replace + name.Substring(pat.Length);
                    }
                    else
                    {
                        name = name.Replace(pat, replace);
                    }

                    ret[name] = kvp.Value;
                }

                return ret;
            }
            else
                return null;
        }

        public static JObject RenameObjectFieldsUnderscores(this JToken jo)
        {
            return jo.RenameObjectFields("_", "");
        }

        public static JObject RemoveObjectFieldsKeyPrefix(this JToken jo, string prefix)
        {
            return jo.RenameObjectFields(prefix, "", true);
        }

        public static JToken JSONParse(this string s, JToken.ParseOptions opt = JToken.ParseOptions.None)
        {
            if (s != null)
                return JToken.Parse(s, opt);
            else
                return null;
        }

        public static JObject JSONParseObject(this string s, JToken.ParseOptions opt = JToken.ParseOptions.None)
        {
            if (s != null)
                return JObject.Parse(s, opt);
            else
                return null;
        }
        public static JArray JSONParseArray(this string s, JToken.ParseOptions opt = JToken.ParseOptions.None)
        {
            if (s != null)
                return JArray.Parse(s, opt);
            else
                return null;
        }
    }
}



