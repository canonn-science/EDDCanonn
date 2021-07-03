/*
 * Copyright © 2017-2020 EDDiscovery development team
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
using System.Reflection;

namespace BaseUtils
{
    public static class TypeHelpers
    {
        public sealed class PropertyNameAttribute : Attribute // applicable to FromObject and ToObject, don't serialise this
        {
            public string Text { get; set; }
            public PropertyNameAttribute(string text) { Text = text; }
        }

        public class PropertyNameInfo
        {
            public string Name;

            public ConditionEntry.MatchType? DefaultCondition;      // null if don't force, else condition
            public string Help;
            public string Comment;

            public PropertyNameInfo() { }
            public PropertyNameInfo(string name, string help, ConditionEntry.MatchType? defcondition = null, string comment = null)
            {
                Name = name; DefaultCondition = defcondition; Help = help; Comment = comment;
            }
        }

        // bf default is DefaultLookup in the .net code for GetProperties()
        static public List<PropertyNameInfo> GetPropertyFieldNames(Type jtype, string prefix = "", BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, 
                                    bool fields = false, int linelen = 80, string comment = null, Type excludedeclaretype = null , Type[] propexcluded = null, int depth = 5 )       // give a list of properties for a given name
        {
            if (depth < 0)
                return null;

            if (jtype != null)
            {
                List<PropertyNameInfo> ret = new List<PropertyNameInfo>();

                foreach (System.Reflection.PropertyInfo pi in jtype.GetProperties(bf))
                {
                    if ((excludedeclaretype == null || pi.DeclaringType != excludedeclaretype) && (propexcluded == null || !propexcluded.Contains(pi.PropertyType)))
                    {
                        if (pi.GetIndexParameters().GetLength(0) == 0)      // only properties with zero parameters are called
                        {
                            AddToPNI(ret, pi.PropertyType, prefix + pi.Name, pi.GetCustomAttributes(typeof(PropertyNameAttribute), false), bf, fields, linelen, comment, excludedeclaretype, propexcluded, depth - 1);
                        }
                    }
                }

                if (fields)
                {
                    foreach (FieldInfo fi in jtype.GetFields())
                    {
                        if ((excludedeclaretype == null || fi.DeclaringType != excludedeclaretype) && (propexcluded == null || !propexcluded.Contains(fi.FieldType)))
                        {
                            AddToPNI(ret, fi.FieldType, prefix + fi.Name, fi.GetCustomAttributes(typeof(PropertyNameAttribute), false), bf, fields, linelen, comment, excludedeclaretype, propexcluded, depth - 1);
                        }
                    }
                }

                return ret;
            }
            else
                return null;
        }

        static public void AddToPNI(List<PropertyNameInfo> ret, Type pt, string name, object [] ca, BindingFlags bf, bool fields, int linelen, string comment, 
                                                                    Type excludedeclaretype, Type[] propexcluded, int depth)
        {
            if (pt.IsArray)
            {
                Type arraytype = pt.GetElementType();

                ret.Add(new PropertyNameInfo(name + "Count", "Integer Index"));

                var pni = GetPropertyFieldNames(arraytype, name + "[]_", bf, fields, linelen, comment, excludedeclaretype, propexcluded, depth - 1);
                if (pni != null)
                    ret.AddRange(pni);

            }
            else if (pt.IsClass && pt != typeof(string))
            {
                var pni = GetPropertyFieldNames(pt, name + "_", bf, fields, linelen, comment, excludedeclaretype, propexcluded, depth - 1);
                if (pni != null)
                    ret.AddRange(pni);
            }
            else
            {
                string help = ca.Length > 0 ? ((dynamic)ca[0]).Text : "";
                PropertyNameInfo pni = PNI(name, pt, linelen, comment, help);
                ret.Add(pni);
            }
        }

        static public PropertyNameInfo PNI( string name, Type t , int ll, string comment, string help)
        {
            string pname = t.FullName;
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(t))
            {
                help = ("Dictionary Class (" + t.GenericTypeArguments[0].Name + "," + t.GenericTypeArguments[1].Name + ")").AppendPrePad(help, " : ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.NumericGreaterEqual, comment);
            }
            else if (t.IsEnum)
            {
                string[] enums = Enum.GetNames(t);
                help = ("Enumeration:" + enums.FormatIntoLines(ll)).AppendPrePad(help, Environment.NewLine);
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.Equals, comment);
            }
            else if (pname.Contains("System.Double"))
            {
                help = "Floating point value".AppendPrePad(help, " : ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.NumericGreaterEqual, comment);
            }
            else if (pname.Contains("System.Boolean"))
            {
                help = "Boolean value, 1 = true, 0 = false".AppendPrePad(help, " : ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.IsTrue, comment);
            }
            else if (pname.Contains("System.Int"))
            {
                help = "Integer value".AppendPrePad(help, " : ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.NumericEquals, comment);
            }
            else if (pname.Contains("System.DateTime"))
            {
                help = "Date Time Value, US format".AppendPrePad(help, " : ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.DateAfter, comment);
            }
            else
            {
                help = "String value".AppendPrePad(help, " : ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.Contains, comment);
            }
        }

        static public MethodInfo FindMember(this MemberInfo[] methods, Type[] paras)    // Must be MethodInfo's, find matching these paras..
        {
            foreach (var memberinfo in methods)
            {
                MethodInfo mi = (MethodInfo)memberinfo;
                ParameterInfo[] p = mi.GetParameters();
                if (p.Length == paras.Length)
                {
                    int i = 0;
                    for (; i < p.Length; i++)
                    {
                        if (p[i].ParameterType != paras[i])
                            break;
                    }

                    if (i == p.Length)
                        return mi;
                }
            }

            return null;
        }

        static public T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        static public Type FieldPropertyType(this MemberInfo mi)        // from member info for properties/fields return type
        {
            if (mi.MemberType == System.Reflection.MemberTypes.Property)
                return ((System.Reflection.PropertyInfo)mi).PropertyType;
            else if (mi.MemberType == System.Reflection.MemberTypes.Field)
                return ((System.Reflection.FieldInfo)mi).FieldType;
            else
                return null;
        }

        public static bool SetValue(this MemberInfo mi, Object instance,  Object value)   // given a member of fields/property, set value in instance
        {
            if (mi.MemberType == System.Reflection.MemberTypes.Field)
            {
                var fi = (System.Reflection.FieldInfo)mi;
                fi.SetValue(instance, value);
                return true;
            }
            else if (mi.MemberType == System.Reflection.MemberTypes.Property)
            {
                var pi = (System.Reflection.PropertyInfo)mi;
                if (pi.SetMethod != null)
                {
                    pi.SetValue(instance, value);
                    return true;
                }
                else
                    return false;
            }
            else
                throw new NotSupportedException();
        }

        // cls = class type (such as typeof(JTokenExtensions)).. gentype = <T> parameter.  then Invoke with return.Invoke(null,new Object[] { values..}) if static, null = instance if not
        public static MethodInfo CreateGeneric(Type cls, string methodname, Type gentype)
        {
            System.Reflection.MethodInfo method = cls.GetMethod(methodname, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return method.MakeGenericMethod(gentype);
        }

        public static Object ChangeTo(this Type type, Object value)     // this extends ChangeType to handle nullables.
        {
            Type underlyingtype = Nullable.GetUnderlyingType(type);     // test if its a nullable type (double?)
            if (underlyingtype != null)
            {
                if (value == null)
                    return null;
                else
                    return Convert.ChangeType(value, underlyingtype);
            }
            else
            {
                return Convert.ChangeType(value, type);       // convert to element type, which should work since we checked compatibility
            }
        }
    }
}
