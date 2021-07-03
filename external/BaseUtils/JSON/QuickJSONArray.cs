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
    // small light JSON decoder and encoder.

    public class JArray : JToken
    {
        public JArray()
        {
            TokenType = TType.Array;
            Elements = new List<JToken>(16);
        }

        public JArray(params Object[] data) : this()
        {
            foreach (Object o in data)
                this.Add(JToken.CreateToken(o));
        }

        public JArray(IEnumerable data) : this()
        {
            foreach (Object o in data)
                this.Add(JToken.CreateToken(o));
        }

        public JArray(JToken o) : this()        // construct with this token at start
        {
            Add(o);
        }

        private List<JToken> Elements { get; set; }

        // if out of range, or indexer not int,  null
        public override JToken this[object key]
        {
            get { if (key is int && (int)key >= 0 && (int)key < Elements.Count) return Elements[(int)key]; else return null; }
            set { System.Diagnostics.Debug.Assert(key is int); Elements[(int)key] = (value == null) ? JToken.Null() : value; }
        }

        // must be in range.
        public JToken this[int element] { get { return Elements[element]; } set { Elements[element] = value; } }

        // try and get a value.
        public bool TryGetValue(int n, out JToken value) { if (n >= 0 && n < Elements.Count) { value = Elements[n]; return true; } else { value = null; return false; } }

        public override int Count { get { return Elements.Count; } }

        public void Add(JToken o) { Elements.Add(o); }
        public void AddRange(IEnumerable<JToken> o) { Elements.AddRange(o); }
        public void RemoveAt(int index) { Elements.RemoveAt(index); }
        public void RemoveRange(int index,int count) { Elements.RemoveRange(index,count); }
        public override void Clear() { Elements.Clear(); }

        public JToken Find(System.Predicate<JToken> predicate) { return Elements.Find(predicate); }       // find an entry matching the predicate
        public T Find<T>(System.Predicate<JToken> predicate) { Object r = Elements.Find(predicate); return (T)r; }       // find an entry matching the predicate

        public List<string> String() { return Elements.ConvertAll<string>((o) => { return o.TokenType == TType.String ? ((string)o.Value) : null; }); }
        public List<int> Int() { return Elements.ConvertAll<int>((o) => { return (int)((long)o.Value); }); }
        public List<long> Long() { return Elements.ConvertAll<long>((o) => { return ((long)o.Value); }); }
        public List<double> Double() { return Elements.ConvertAll<double>((o) => { return ((double)o.Value); }); }

        public override IEnumerator<JToken> GetSubClassTokenEnumerator() { return Elements.GetEnumerator(); }
        public override IEnumerator GetSubClassEnumerator() { return Elements.GetEnumerator(); }

        public new static JArray Parse(string s, ParseOptions flags = ParseOptions.None)        // null if failed.
        {
            var res = JToken.Parse(s,flags);
            return res as JArray;
        }

        public new static JArray ParseThrowCommaEOL(string s)        // throws if fails, allows trailing commas and checks EOL
        {
            var res = JToken.Parse(s, JToken.ParseOptions.AllowTrailingCommas | JToken.ParseOptions.CheckEOL | JToken.ParseOptions.ThrowOnError);
            return res as JArray;
        }

        public new static JArray Parse(string s, out string error, ParseOptions flags)
        {
            var res = JToken.Parse(s, out error, flags);
            return res as JArray;
        }
    }

}



