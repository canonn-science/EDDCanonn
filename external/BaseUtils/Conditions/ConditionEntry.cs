/*
 * Copyright © 2017 - 2021 EDDiscovery development team
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

namespace BaseUtils
{
    public class ConditionEntry
    {
        public enum MatchType
        {
            Contains,           // contains
            DoesNotContain,     // doesnotcontain
            Equals,             // ==
            NotEqual,           // !=
            ContainsCaseSensitive,           // contains
            DoesNotContainCaseSensitive,     // doesnotcontain
            EqualsCaseSensitive,             // ==
            NotEqualCaseSensitive,           // !=

            IsEmpty,            // string
            IsNotEmpty,         // string

            IsTrue,             // numeric !=0
            IsFalse,            // numeric == 0

            NumericEquals,      // numeric =
            NumericNotEquals,   // numeric !=
            NumericGreater,            // numeric >
            NumericGreaterEqual,       // numeric >=
            NumericLessThan,           // numeric <
            NumericLessThanEqual,      // numeric <=

            DateAfter,          // Date compare
            DateBefore,         // Date compare.

            IsPresent,          // field is present
            IsNotPresent,       // field is not present

            IsOneOf,            // left, is it one of a quoted comma list on right
            AnyOfAny,           // is any in a comma separ on left, one of a quoted comma list on right
            MatchSemicolon,     // left, is it one of the semicolon list on right
            MatchCommaList,     // left, is it one of the comma list on right
            MatchSemicolonList, // left, is it one of the semicolon list on right, use OR

            AlwaysTrue,         // Always true
            AlwaysFalse,          // never true
        };

        static public string[] MatchNames = {       // used for display
            "Contains", "Not Contains", "== (Str)", "!= (Str)", "Contains(CS)", "Not Contains(CS)", "== (CS)","!= (CS)",
            "Is Empty", "Is Not Empty",
            "Is True (Int)", "Is False (Int)",
            "== (Num)", "!= (Num)", "> (Num)",">= (Num)", "< (Num)",                                   "<= (Num)",
            ">= (Date)", "< (Date)",
            "Is Present", "Not Present",
            "Is One Of", "Any Of Any",  "Match Semicolon",  "Match Comma List",  "Match Semicolon List",
            "Always True/Enable", "Always False/Disable"
        };

        static public string[] OperatorNames = {        // used for ASCII rep .
            "Contains","NotContains","$==","$!=","CSContains","CSNotContains","CS==","CS!=",
            "Empty","IsNotEmpty",
            "IsTrue","IsFalse",
            "==","!=",">",">=","<","<=",
            "D>=","D<",
            "IsPresent","NotPresent",
            "IsOneOf","AnyOfAny","MatchSemicolon","MatchCommaList","MatchSemicolonList",
            "AlwaysTrue","AlwaysFalse"
         };

        public enum Classification { String, Numeric, Logical, Date, Macro }
        public static Classification Classify(MatchType mt)                         // indicates which class of operator
        {
            if (mt <= MatchType.IsNotEmpty)
                return Classification.String;
            else if (mt <= MatchType.IsFalse || mt >= MatchType.AlwaysTrue)
                return Classification.Logical;
            else if (mt <= MatchType.NumericLessThanEqual)
                return Classification.Numeric;
            else if (mt <= MatchType.DateBefore)
                return Classification.Date;
            else if (mt <= MatchType.IsNotPresent)
                return Classification.Macro;
            else
                return Classification.String;
        }

        public static bool IsNullOperation(MatchType matchtype) { return matchtype == MatchType.AlwaysTrue || matchtype == MatchType.AlwaysFalse; }
        public static bool IsUnaryOperation(MatchType matchtype) { return matchtype == MatchType.IsNotPresent || matchtype == MatchType.IsPresent || matchtype == MatchType.IsTrue || matchtype == MatchType.IsFalse || matchtype == MatchType.IsEmpty || matchtype == MatchType.IsNotEmpty; }

        public static bool IsNullOperation(string matchname)
        {
            MatchType mt;
            return MatchTypeFromString(matchname, out mt) && IsNullOperation(mt);
        }

        public static bool IsUnaryOperation(string matchname)
        {
            MatchType mt;
            return MatchTypeFromString(matchname, out mt) && IsUnaryOperation(mt);
        }

        static public bool MatchTypeFromString(string s, out MatchType mt)
        {
            int indexof = Array.FindIndex(MatchNames, x => x.Equals(s, StringComparison.InvariantCultureIgnoreCase));

            if (indexof == -1)
                indexof = Array.FindIndex(OperatorNames, x => x.Equals(s, StringComparison.InvariantCultureIgnoreCase));

            if (indexof >= 0)
            {
                mt = (MatchType)(indexof);
                return true;
            }
            else if (Enum.TryParse<MatchType>(s, out mt))
                return true;
            else
            {
                mt = MatchType.Contains;
                return false;
            }
        }



        public enum LogicalCondition
        {
            Or,     // any true     (DEFAULT)
            And,    // all true
            Nor,    // any true produces a false
            Nand,   // any not true produces a true
            NA,     // not applicable - use for outer condition on first entry
        }

        public ConditionEntry()
        {
            ItemName = MatchString = "";
        }

        public ConditionEntry(string i, MatchType m, string s)
        {
            ItemName = i;
            MatchCondition = m;
            MatchString = s;
        }

        public ConditionEntry(ConditionEntry other)
        {
            ItemName = other.ItemName;
            MatchCondition = other.MatchCondition;
            MatchString = other.MatchString;
        }


        static public string GetLogicalCondition(BaseUtils.StringParser sp, string delimchars, out LogicalCondition value)
        {
            value = LogicalCondition.Or;

            string condi = sp.NextQuotedWord(delimchars);       // next is the inner condition..

            if (condi == null)
                return "Condition operator missing";

            condi = condi.Replace(" ", "");

            if (Enum.TryParse<ConditionEntry.LogicalCondition>(condi.Replace(" ", ""),true, out value))
                return "";
            else
                return "Condition operator " + condi + " is not recognised";
        }

        public string ItemName { get; set; }
        public MatchType MatchCondition { get; set; }               
        public string MatchString { get; set; }                     

        public bool Create(string i, string ms, string v)     // ms can have spaces inserted into enum
        {
            if (MatchTypeFromString(ms, out MatchType matchtypev))
            {
                ItemName = i;
                MatchString = v;
                MatchCondition = matchtypev;

                return true;
            }
            else
                return false;
        }

    };

}
