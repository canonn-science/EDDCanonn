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
using System.Collections;
using System.Collections.Generic;

namespace BaseUtils.JSON
{
    public partial class JToken 
    {
        public override string ToString()   // back to JSON form
        {
            return ToString(this, "", "", "", false);
        }

        public string ToStringLiteral()     // data as is, without quoting/escaping strings. Used for data extraction
        {
            return ToString(this, "", "", "", true);
        }

        public string ToString(bool verbose = false, string oapad = "  ")           // verbose, if so, oapad is configurable
        {
            return verbose ? ToString(this, "", "\r\n", oapad, false) : ToString(this, "", "", "", false);
        }

        public string ToString(string oapad)            // not verbose, but prefix in from of obj/array is configuration
        {
            return ToString(this, "", "", oapad, false);
        }

        // prepad = padding before value, or padding before { or [
        // postpad = passing after value, or padding after } or ]
        // oadpad = additonal text to add to prepad for objects/arrays

        public static string ToString(JToken o, string prepad, string postpad, string oapad, bool stringliterals)
        {
            if (o.TokenType == TType.String)
            {
                if (stringliterals)       // used if your extracting the value of the data as a string, and not turning it back to json.
                    return prepad + (string)o.Value + postpad;
                else
                    return prepad + "\"" + ((string)o.Value).EscapeControlCharsFull() + "\"" + postpad;
            }
            else if (o.TokenType == TType.Double)
                return prepad + ((double)o.Value).ToStringInvariant("0.0############################") + postpad;         // new! preserve that its a double by insisting on at least a single decimalm digit
            else if (o.TokenType == TType.Long)
                return prepad + ((long)o.Value).ToStringInvariant() + postpad;
            else if (o.TokenType == TType.ULong)
                return prepad + ((ulong)o.Value).ToStringInvariant() + postpad;
#if JSONBIGINT
            else if (o.TokenType == TType.BigInt)
                return prepad + ((System.Numerics.BigInteger)o.Value).ToString(System.Globalization.CultureInfo.InvariantCulture) + postpad;
#endif
            else if (o.TokenType == TType.Boolean)
                return prepad + ((bool)o.Value).ToString().ToLower() + postpad;
            else if (o.TokenType == TType.Null)
                return prepad + "null" + postpad;
            else if (o.TokenType == TType.Array)
            {
                string s = prepad + "[" + postpad;
                string arrpad = prepad + oapad;
                JArray ja = o as JArray;
                for (int i = 0; i < ja.Count; i++)
                {
                    bool notlast = i < ja.Count - 1;
                    s += ToString(ja[i], arrpad, postpad, oapad, stringliterals);
                    if (notlast)
                    {
                        s = s.Substring(0, s.Length - postpad.Length) + "," + postpad;
                    }
                }
                s += prepad + "]" + postpad;
                return s;
            }
            else if (o.TokenType == TType.Object)
            {
                string s = prepad + "{" + postpad;
                string objpad = prepad + oapad;
                int i = 0;
                JObject jo = ((JObject)o);
                foreach (var e in jo)
                {
                    bool notlast = i++ < jo.Count - 1;
                    if (e.Value is JObject || e.Value is JArray)
                    {
                        if (stringliterals)
                            s += objpad + e.Key.EscapeControlCharsFull() + ":" + postpad;
                        else
                            s += objpad + "\"" + e.Key.EscapeControlCharsFull() + "\":" + postpad;

                        s += ToString(e.Value, objpad, postpad, oapad, stringliterals);
                        if (notlast)
                        {
                            s = s.Substring(0, s.Length - postpad.Length) + "," + postpad;
                        }
                    }
                    else
                    {
                        if (stringliterals)
                            s += objpad + e.Key.EscapeControlCharsFull() + ":";
                        else
                            s += objpad + "\"" + e.Key.EscapeControlCharsFull() + "\":";

                        s += ToString(e.Value, "", "", oapad, stringliterals) + (notlast ? "," : "") + postpad;
                    }
                }
                s += prepad + "}" + postpad;
                return s;
            }
            else if (o.TokenType == TType.Error)
                return "ERROR:" + (string)o.Value;
            else
                return null;
        }
    }
}



