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
using System.Linq;

namespace BaseUtils.JSON
{
    // small light JSON decoder and encoder.
    // Object class, holds key/value pairs

    public class JObject : JToken, IEnumerable<KeyValuePair<string, JToken>>
    {
        public JObject()
        {
            TokenType = TType.Object;
            Objects = new Dictionary<string, JToken>(16);   // giving a small initial cap seems to help
        }

        public JObject(IDictionary dict) : this()           // convert from a dictionary. Key must be string
        {
            foreach (DictionaryEntry x in dict)
            {
                this.Add((string)x.Key, JToken.CreateToken(x.Value));
            }
        }

        public JObject(JObject other) : this()              // create with deep copy from another object
        {
            foreach( var kvp in other.Objects)
            {
                Objects[kvp.Key] = kvp.Value.Clone();
            }
        }

        private Dictionary<string, JToken> Objects { get; set; }

        // Returns value or null if not present or not string indexor.  jo["fred"].Str() works if fred is not present as Str() is an extension class and can accept null
        public override JToken this[object key]
        {
            get { if (key is string && Objects.TryGetValue((string)key, out JToken v)) return v; else return null; }
            set { System.Diagnostics.Debug.Assert(key is string); Objects[(string)key] = (value == null) ? JToken.Null() : value; }
        }

        // Returns value or null if not present
        public JToken this[string key]
        {
            get { if (Objects.TryGetValue(key, out JToken v)) return v; else return null; }
            set { Objects[key] = (value == null) ? JToken.Null() : value; }
        }

        public string[] PropertyNames() { return Objects.Keys.ToArray(); }

        public bool Contains(string n) { return Objects.ContainsKey(n); }
        public bool TryGetValue(string n, out JToken value) { return Objects.TryGetValue(n, out value); }

        public JToken Contains(string[] ids)     // see if Object contains one of these keys
        {
            foreach (string key in ids)
            {
                if (Objects.ContainsKey(key))
                    return Objects[key];
            }
            return null;
        }

        public override int Count { get { return Objects.Count; } }

        public void Add(string key, JToken value) { this[key] = value; }
        public bool Remove(string key) { return Objects.Remove(key); }
        public void Remove(params string[] key) { foreach( var k in key) Objects.Remove(k); }
        public override void Clear() { Objects.Clear(); }

        public new static JObject Parse(string s, ParseOptions flags = ParseOptions.None)        // null if failed.
        {
            var res = JToken.Parse(s, flags);
            return res as JObject;
        }

        public new static JObject ParseThrowCommaEOL(string s)        // throws if fails, allows trailing commas and checks EOL
        {
            var res = JToken.Parse(s, JToken.ParseOptions.AllowTrailingCommas | JToken.ParseOptions.CheckEOL | JToken.ParseOptions.ThrowOnError);
            return res as JObject;
        }

        public new static JObject Parse(string s, out string error, ParseOptions flags)
        {
            var res = JToken.Parse(s, out error, flags);
            return res as JObject;
        }

        public new IEnumerator<KeyValuePair<string, JToken>> GetEnumerator() { return Objects.GetEnumerator(); }
        public override IEnumerator<JToken> GetSubClassTokenEnumerator() { return Objects.Values.GetEnumerator(); }
        public override IEnumerator GetSubClassEnumerator() { return Objects.GetEnumerator(); }
    }
}



