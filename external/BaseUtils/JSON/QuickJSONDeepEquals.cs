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

namespace BaseUtils.JSON
{
    public partial class JToken 
    {
        public bool DeepEquals(JToken other)
        {
            switch (TokenType)
            {
                case TType.Array:
                    {
                        JArray us = (JArray)this;
                        if (other.TokenType == TType.Array)
                        {
                            JArray ot = (JArray)other;
                            if (ot.Count == us.Count)
                            {
                                for (int i = 0; i < us.Count; i++)
                                {
                                    if (!us[i].DeepEquals(other[i]))
                                        return false;
                                }
                                return true;
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine("Array count diff");
                                return false;
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Other array not an array");
                            return false;
                        }
                    }

                case TType.Object:
                    {
                        JObject us = (JObject)this;
                        if (other.TokenType == TType.Object)
                        {
                            JObject ot = (JObject)other;
                            if (ot.Count == us.Count)
                            {
                                //System.Diagnostics.Debug.WriteLine("Check {0} keys", us.Count);
                                foreach (var kvp in us)
                                {
                                    if (!ot.Contains(kvp.Key))
                                    {
                                        //System.Diagnostics.Debug.WriteLine("Cannot find key {0}", kvp.Key);
                                        return false;
                                    }
                                    else
                                    {
                                        //System.Diagnostics.Debug.WriteLine("Key {0} : {1} vs {2}", kvp.Key, kvp.Value, ot[kvp.Key]);
                                        if (!kvp.Value.DeepEquals(ot[kvp.Key]))       // order unimportant to kvp)
                                            return false;
                                    }
                                }
                                //System.Diagnostics.Debug.WriteLine("All obj okay");
                                return true;
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine("Key count different");
                                return false;
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Other obj is not an object");
                            return false;
                        }
                    }

                default:
                    // compare this and other value.
                    // if either is double we need to deal with it using an approxequals function due to floating point rep.
                    //      and it allows an integer (30) to be compared with a float (30.0) so we try and convert both to doubles then approx equals
                    // if both same token type, we can just use Equals
                    // if one is a boolean, we convert both to boolean and compare (thus accepting 1/0 int as true/false)

                    if (this.TokenType == TType.Double || other.TokenType == TType.Double)          // either is double
                    {
                        double? usd = (double?)this;          // try and convert us to double
                        double? otherd = (double?)other;      // try and convert the other to double

                        if (usd != null && otherd != null)          // if we could, compare
                        {
                            const double epsilon = 2.2204460492503131E-12;                  // picked to be less than std E print range (14 digits).
                            bool equals = usd.Value.ApproxEquals(otherd.Value, epsilon);
                            //if (equals == false) System.Diagnostics.Debug.WriteLine("Convert to double {0} vs {1} = {2}", this.Value, other.Value, equals);
                            return equals;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("{0} vs {1} Not the same token type", this.TokenType, other.TokenType);
                            return false;
                        }
                    }
                    else if (this.TokenType == TType.Null || other.TokenType == TType.Null)    // if either is null
                    {
                        return this.TokenType == other.TokenType;       // if both are the same, ie. null, its true
                    }
                    else if (other.TokenType == this.TokenType)         // if both the same token type, use Equals (int, string, boolean)
                    {
                        bool equals = this.Value.Equals(other.Value);
                        //if ( equals == false ) System.Diagnostics.Debug.WriteLine("same type {0} vs {1} = {2}", this.Value, other.Value, equals);
                        return equals;
                    }
                    else if (this.TokenType == TType.Boolean || other.TokenType == TType.Boolean)          // either is boolean
                    {
                        bool? usb = (bool?)this;              // try and convert us to bool
                        bool? otherb = (bool?)other;          // try and convert the other to bool
                        if (usb != null && otherb != null)          // if we could, compare
                        {
                            bool equals = usb.Value == otherb.Value;
                            //if (equals == false) System.Diagnostics.Debug.WriteLine("Convert to bool {0} vs {1} = {2}", this.Value, other.Value, equals);
                            return equals;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("{0} vs {1} Not the same token type", this.TokenType, other.TokenType);
                            return false;
                        }
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("{0} vs {1} Not the same token type", this.TokenType, other.TokenType);
                        return false;
                    }
            }
        }

        static public bool DeepEquals(JToken left, JToken right)
        {
            return left != null && right != null && left.DeepEquals(right);
        }

    }
}



