/*
 * Copyright © 2016-2018 EDDiscovery development team
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
using System.Collections.Generic;
using System.Linq;

namespace BaseUtils
{
    // Quick formatter using Fluent syntax when you want to do it quickly 

    public class QuickJSONFormatter        
    {
        enum StackType { Array, Object };

        class StackEntry
        {
            public bool precomma;
            public StackType t;
            public StackEntry(StackType a , bool b )
            { precomma = b;t = a; }
        }

        private string json;

        List<StackEntry> stack;
        bool precomma;          // starts false, ever value sets it true.

        public QuickJSONFormatter()
        {
            json = "";
            stack = new List<StackEntry>();
            precomma = false;
        }

        private void Prefix()
        {
            if (precomma)
                json += ", ";
            precomma = true;
        }

        public QuickJSONFormatter V(string name)
        {
            Prefix();
            json += "\"" + name + "\"";
            return this;
        }

        public QuickJSONFormatter V(string name, string data)
        {
            Prefix();
            if (name != null)
                json += "\"" + name + "\":";
            json += "\"" + data + "\"";
            return this;
        }

        public QuickJSONFormatter V(string name, double v)
        {
            Prefix();
            if (name != null)
                json += "\"" + name + "\":";
            json += v.ToString("0.0#####");
            return this;
        }

        public QuickJSONFormatter V(string name, int v)
        {
            Prefix();
            if ( name != null )
                json += "\"" + name + "\":";
            json += v.ToStringInvariant();
            return this;
        }

        public QuickJSONFormatter V(string name, long v)
        {
            Prefix();
            if (name != null)
                json += "\"" + name + "\":";
            json += v.ToStringInvariant();
            return this;
        }

        public QuickJSONFormatter V(string name, bool v)
        {
            Prefix();
            if (name != null)
                json += "\"" + name + "\":";
            json += (v ? "true" : "false");
            return this;
        }

        public QuickJSONFormatter V(string name, DateTime v)
        {
            Prefix();
            if (name != null)
                json += "\"" + name + "\":";
            json += "\"" + v.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\"";
            return this;
        }

        public QuickJSONFormatter Literal(string text)
        {
            Prefix();
            json += text;
            return this;
        }

        public QuickJSONFormatter UTC(string name)
        {
            Prefix();
            DateTime dt = DateTime.UtcNow;
            if (name != null)
                json += "\"" + name + "\":";
            json += "\"" + dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") + "\"";
            return this;
        }

        public QuickJSONFormatter V(string name, int[] array)
        {
            Prefix();

            string s = "";
            foreach (int a in array)
            {
                if (s.Length > 0)
                    s += ", ";

                s += a.ToString();
            }

            json += "\"" + name + "\":[" + s + "] ";

            return this;
        }

        public QuickJSONFormatter V(string name, string[] array)
        {
            Prefix();

            string s = "";
            foreach (string a in array)
            {
                if (s.Length > 0)
                    s += ", ";

                s += "\"" + a  + "\"";
            }

            json += "\"" + name + "\":[" + s + "] ";

            return this;
        }

        public QuickJSONFormatter Array(string name)        // call, add elements, call close. name can be null, unnamed array. Call elements with name=null
        {
            Prefix();

            if ( name != null)
                json += "\"" + name + "\": [";
            else
                json += "[";

            stack.Add(new StackEntry(StackType.Array, precomma));
            precomma = false;
            return this;
        }

        public QuickJSONFormatter Object()                  // call, add elements, call close
        {
            Prefix();

            json += "{ ";
            stack.Add(new StackEntry(StackType.Object, precomma));
            precomma = false;
            return this;
        }

        public QuickJSONFormatter Object(string name)       // call, add elements, call close
        {
            Prefix();

            json += "\"" + name + "\":{ ";
            stack.Add(new StackEntry(StackType.Object, precomma));
            precomma = false;
            return this;
        }

        public QuickJSONFormatter Close( int depth = 1 )    // close one of more Arrays/Objects
        {
            while (depth-- > 0 && stack.Count > 0 )
            {
                StackEntry e = stack.Last();

                if (e.t == StackType.Array)
                    json += "] ";
                else
                    json += "} ";

                precomma = e.precomma;
                stack.RemoveAt(stack.Count - 1);
            }

            return this;
        }

        public QuickJSONFormatter LF()
        {
            json += Environment.NewLine;
            return this;
        }

        public string Get()                                 // Complete string and return JSON!
        {
            Close(int.MaxValue);
            return json;
        }

        public override string ToString()
        {
            return Get();
        }

    }
}
